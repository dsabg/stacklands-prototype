using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Systems
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        public static Controls Controls;

        public TextMeshProUGUI foodText;
        public TextMeshProUGUI moneyText;


        public static Transform CardContainer { get; private set; }
        [SerializeField]
        private Transform _CardContainer;

        public static Transform PackageContainer { get; private set; }
        [SerializeField]
        private Transform _PackageContainer;

        [Header("Prefabs")]
        public GameObject CardPrefab;
        public GameObject PackagePrefab;
        public GameObject PackageSlotPrefab;

       

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogWarning($"A second game manager was created while one was already registered on {gameObject.name}");
                Destroy(this);
            }

            if (_CardContainer == null) throw new System.NullReferenceException("_CardContainer is null. Set from inspector.");
            CardContainer = _CardContainer;
            PackageContainer = _PackageContainer;
            Controls = new Controls();
            Controls.Enable();

            DataManager.LoadModules();
            
            StartGame();
            
        }
        public void StartGame()
        {

            GameObject package = Instantiate(PackagePrefab, Vector3.zero, Quaternion.identity,PackageContainer);
            CardPackage cardPackage = package.GetComponent<CardPackage>();
            if (cardPackage != null)
            {
                cardPackage.Initialise(CardManager.LoadedPackages["Start Package"]);
                cardPackage.switchOpen();
            }


            Vector3 pos = new Vector3(-3, 4.45f, 0);
            for (int i = 0; i < 5; i++)
            {
                GameObject Slot = Instantiate(PackageSlotPrefab, pos, Quaternion.identity);
                PackageSlot packageSlot = Slot.GetComponent<PackageSlot>();
                if (i == 0)
                {
                    packageSlot.Initialise(CardManager.LoadedPackages["Start Package"]);
                }
                pos.x += 3;
            }
        }
        public void RestartGame()
        {
            foreach(CardClass cardClass in Enum.GetValues(typeof(CardClass)))
            {
                if (CardManager.CardsByClass.ContainsKey(cardClass))
                {
                    CardManager.DestroyCards(CardManager.CardsByClass[cardClass].ToArray());
                }
            }
            for (int i = PackageContainer.childCount - 1; i >= 0; i--) // ÄæÐòÉ¾³ý
            {
                GameObject.Destroy(PackageContainer.GetChild(i).gameObject);
            }


            GameObject package = Instantiate(PackagePrefab, Vector3.zero, Quaternion.identity, PackageContainer);
            CardPackage cardPackage = package.GetComponent<CardPackage>();
            if (cardPackage != null)
            {
                cardPackage.Initialise(CardManager.LoadedPackages["Start Package"]);
                cardPackage.switchOpen();
            }
        }

        private void Update()
        {
                moneyText.text = " : " + CardManager.Money;
                foodText.text = " : " + CardManager.Food + " / " + CardManager.People*2;
        }
        private void SpawnAllCardsInGrid()
        {
            const float xExtent = 10;
            const float yExtent = 4;
            const float xSpacing = 1.5f;
            const float ySpacing = 2f;

            const int xCount = (int)(xExtent / xSpacing);

            int current = 0;
            foreach (CardData card in CardManager.LoadedCards.Values)
            {
                int x = current % xCount;
                int y = current / xCount;
                CardManager.SpawnCard(card, new Vector3(-xExtent + x * xSpacing, yExtent - y * ySpacing, 0));
                current++;
            }
        }
    }
}