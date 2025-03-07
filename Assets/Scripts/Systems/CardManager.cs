using Data;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

namespace Systems
{
    public static class CardManager
    {//静态类是 C# 中的一种特殊类，专门用于存储全局的工具方法、常量或共享数据。它不能被实例化，所有成员都必须是静态的。
        /// <summary>
        /// Loaded cards in data form.
        /// </summary>
        public static Dictionary<string, CardData> LoadedCards = new Dictionary<string, CardData>();

        public static Dictionary<string,PackageData>LoadedPackages = new Dictionary<string,PackageData>();

        /// <summary>
        /// Existing card instances indexed by name.
        /// </summary>
        public static Dictionary<string, List<Card>> CardsByName = new Dictionary<string, List<Card>>();
        /// <summary>
        /// Existing card instances indexed by class.
        /// </summary>
        public static Dictionary<CardClass, List<Card>> CardsByClass = new Dictionary<CardClass, List<Card>>();

        public static int Money;

        public static int People;

        public static int Food;

        public static Card SpawnCard(string name, Vector3 position = default, Quaternion rotation = default)
        {
            if (LoadedCards.TryGetValue(name, out CardData data))
            {
                return SpawnCard(data, position, rotation);
            }

            throw new ArgumentOutOfRangeException($"Card '{name}' does not exist.");
        }

        public static Card SpawnCard(CardData data, Vector3 position = default, Quaternion rotation = default)
        {
            Card card = UnityEngine.Object.Instantiate(GameManager.instance.CardPrefab, position, rotation, GameManager.CardContainer).GetComponent<Card>();
            card.gameObject.name = data.Name;
            card.Initialise(data);

            if (!CardsByName.ContainsKey(data.Name))
            {
                CardsByName.Add(data.Name, new List<Card>());
            }
            if (!CardsByClass.ContainsKey(data.CardClass))
            {
                CardsByClass.Add(data.CardClass, new List<Card>());
            }

            if (data.CardClass == CardClass.Money)
            {
                Money += data.Value;
            }
            if(data.CardClass == CardClass.People)
            {
                People++;
            }
            if(data.CardClass == CardClass.Food)
            {
                Food += data.Food;
            }

            // TODO: Add to dictionary by tags.            
            CardsByName[data.Name].Add(card);
            CardsByClass[data.CardClass].Add(card);
            return card;
        }

        public static void RemoveCard(Card card)
        {
            if (CardsByName.ContainsKey(card.CardName))
            {
                CardsByName[card.CardName].Remove(card);
            }
            if (CardsByClass.ContainsKey(card.Data.CardClass))
            {
                CardsByClass[card.Data.CardClass].Remove(card);
            }
            /*
            else
            {
                Debug.LogError($"Card '{card.CardName}' does not exist.");
            }
            */
        }

        private static void PrepareCardForDestroy(Card card)
        {
            //CardsByName.Remove(card.CardName);
            CardsByName[card.CardName].Remove(card);
            CardsByClass[card.Data.CardClass].Remove(card);
            card.PrepareForDestroy();
            card.Extract();
        }


        public static void DestroyCards(params Card[] cards)//允许传入一个或多个 Card 类型的参数
        {
            foreach(Card toDestroy in cards)
            {
                PrepareCardForDestroy(toDestroy);
                if (toDestroy.Data.CardClass == CardClass.Money)
                {
                    Money -= toDestroy.Data.Value;

                }
                if (toDestroy.Data.CardClass == CardClass.People)
                {
                    People--;
                }
                if (toDestroy.Data.CardClass == CardClass.Food)
                {
                    Food -= toDestroy.Data.Food;
                }
                UnityEngine.Object.Destroy(toDestroy.gameObject);
            }
        }

        public static void DestroyCards(Vector3 moveTo, params Card[] cards)
        {
            foreach (Card toDestroy in cards)
            {
                PrepareCardForDestroy(toDestroy);

                //Debug.Log($"{Time.frameCount}: Start destroy: {toDestroy.gameObject.name}");
                _ = toDestroy.MoveComponent.MoveTo(moveTo, () => { UnityEngine.Object.Destroy(toDestroy.gameObject); /*Debug.Log($"{Time.frameCount}: Destroying {toDestroy.gameObject.name}"); */});
            }
        }
    }
}
