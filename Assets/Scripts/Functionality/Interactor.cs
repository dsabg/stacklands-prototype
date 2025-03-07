﻿using Data;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Systems;
using UnityAsync;
using UnityEngine;

namespace Interactions
{
    public class InteractionProcess
    {
        public readonly InteractionData Data;
        public ProcessCardListener Listener;
        private Card _AttachedCard;
        private Interactor _Interactor;
        private int _Times=0;

        public List<Card> Consumed = new List<Card>();

        private CancellationTokenSource _TokenSource;

        public bool IsRunning { get; private set; }

        public void Cancel()
        {
            if (!IsRunning) return;
            _TokenSource?.Cancel();
            IsRunning = false;
            Consumed.Clear();
            Listener.RemoveAllListeners();
        }

        public void Start()
        {
            Cancel();
            _TokenSource = new CancellationTokenSource();
            IsRunning = true;
            Run(_TokenSource.Token);
        }

        private async void Run(CancellationToken token)
        {
            if (Data.Duration == 0) RequestCompletion();
            UI.ProgressBar progressBar = _AttachedCard.RequestProgressBar();
            float lastTick = Time.time;
            float elapsed;
            while (!token.IsCancellationRequested)
            {
                elapsed = Time.time - lastTick;
                progressBar.UpdateValue(elapsed / Data.Duration);

                if (elapsed > Data.Duration)
                {
                    RequestCompletion();
                    break;
                }
                await Await.NextUpdate();
            }
            // TODO: better method of handling progress bars
            GameObject.Destroy(progressBar.gameObject);
            if (_Times == Data.Times)
            {
                CardManager.DestroyCards(_AttachedCard);
            }
        }

        private void RequestCompletion()
        {
            _Interactor.RequestCompletion(this);
        }

        public void Complete()
        {
            Systems.CardManager.DestroyCards(_AttachedCard.transform.position, Consumed.ToArray());
            System.Random rand = new System.Random();
            int randomIndex = rand.Next(Data.Create.Count);
            CardRef card=Data.Create[randomIndex];
            
                for (int i = 0; i < card.Quantity; i++)
                {
                    _AttachedCard.CreateCard(card.CardName, _AttachedCard.transform.position, new Vector3(1.5f, 0, 0));
                }
            
            IsRunning = false;
            _Times++;
            
        }

        public InteractionProcess(InteractionData data, Card attached, Interactor interactor)
        {
            Data = data;
            _AttachedCard = attached;
            _Interactor = interactor;
            Listener = new ProcessCardListener(Data.ID, interactor);
        }
    }
    public class ProcessCardListener
    {
        private List<Card> Listeners = new List<Card>();
        public readonly uint InteractionID;
        private readonly Interactor _Interactor;

        public void RemoveAllListeners()
        {
            foreach (Card card in Listeners)
            {
                if (card == null) continue;
                card.StackChanged -= OnCardStateChange;
            }
            Listeners.Clear();
        }

        public void AddListener(Card card)
        {
            if (Listeners.Contains(card)) return;
            Listeners.Add(card);
            card.StackChanged += OnCardStateChange;
        }

        public void OnCardStateChange(Card card)
        {
            _Interactor.OnCardChange(card, InteractionID);
        }

        public ProcessCardListener(uint interactionID, Interactor interactor)
        {
            InteractionID = interactionID;
            _Interactor = interactor;
        }
    }

    public enum InteractorState
    {
        All,
        DoNotStartNew,
        None,
    }
    public class Interactor
    {

        public InteractorState State { get; protected set; }

        public Card AttachedCard { get; protected set; }

        public Dictionary<uint, InteractionProcess> Processes = new Dictionary<uint, InteractionProcess>();

        
        public void SetState(InteractorState state)
        {
            State = state;
            
            if (State == InteractorState.None)
            {
                AttachedCard.StackChanged -= CheckProcesses;
                foreach (InteractionProcess process in Processes.Values)
                {
                    process.Cancel();
                }
            }
            else
            {
                AttachedCard.StackChanged += CheckProcesses;
            }
        }

        public void RequestCompletion(InteractionProcess process)
        {
            UnityEngine.Debug.Assert(State != InteractorState.None, "Completion requested while interactor was disabled.");
            process.Listener.RemoveAllListeners();
            process.Complete();

            if (process.Data.IsValidForRun(AttachedCard, out Card[] required, out Card[] consumed))
            {
                AddListeners(process, required, consumed);
                process.Start();
            }
        }

        public void CheckProcesses(Card _)
        {
           UnityEngine.Debug.Assert(State != InteractorState.None, "CheckProcesses called while interactor was disabled.");
            if (Processes.Values.Count==1)
            {
                InteractionProcess process = Processes.First().Value;
                bool isValid = process.Data.IsValidForRun(AttachedCard, out Card[] required, out Card[] consumed);
                if (State == InteractorState.All && isValid && !process.IsRunning)
                {
                    AddListeners(process, required, consumed);
                    process.Start();
                }
                if (!isValid && process.IsRunning)
                {
                    process.Cancel();
                }
            }
            else if (Processes.Values.Count > 1)
            {
                List<InteractionProcess> interactionProcesses = Processes.Values.ToList();
                interactionProcesses.Sort((a, b) => b.Data.Priority.CompareTo(a.Data.Priority));//降序排序
                foreach (InteractionProcess process in interactionProcesses)
                {
                    bool isValid = process.Data.IsValidForRun(AttachedCard, out Card[] required, out Card[] consumed);
                    if (State == InteractorState.All && isValid && !process.IsRunning)
                    {
                        AddListeners(process, required, consumed);
                        process.Start();
                        break;
                    }
                    if (!isValid && process.IsRunning)
                    {
                        process.Cancel();
                    }
                }

            }
        }

        private void AddListeners(InteractionProcess process, Card[] required, Card[] consumed)
        {
            process.Listener.RemoveAllListeners();
            foreach (Card c in required)
            {
                process.Listener.AddListener(c);
            }
            process.Consumed.Clear();
            foreach (Card c in consumed)
            {
                process.Listener.AddListener(c);
                process.Consumed.Add(c);
            }
        }

        public void OnCardChange(Card card, uint interactionID)
        {
            UnityEngine.Debug.Assert(State != InteractorState.None, "OnCardChange called while interactor was disabled.");
            if (!card.InSameStack(AttachedCard))
            {
                InteractionProcess process = Processes[interactionID];
                if (process.Data.IsValidForRun(AttachedCard, out Card[] required, out Card[] consumed))
                {
                    AddListeners(process, required, consumed);
                }
            }
        }
        
        public Interactor(Card attached)
        {
            AttachedCard = attached;
            foreach (InteractionData data in attached.Data.OnStack)
            {
                Processes.Add(data.ID, new InteractionProcess(data, attached, this));
            }
            AttachedCard.StackChanged += CheckProcesses;
        }
    }
}
