using Data;
using System;
using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlasticPipe.PlasticProtocol.Messages.Serialization.ItemHandlerMessagesSerialization;

[RequireComponent(typeof(Draggable), typeof(Moveable))]
public class CardPackage : MonoBehaviour
{
    

    public Moveable MoveComponent { get; private set; }
    private Draggable _DragComponent;
    private PackageData _PackageData;
   
    [SerializeField]
    private TextMeshProUGUI _NameText;

    private void Awake()
    {
        _DragComponent = GetComponent<Draggable>();
        MoveComponent = GetComponent<Moveable>();
        
        _DragComponent.PointerDownCallback += open;
        
    }
    // Start is called before the first frame update
    void Start()
    {
      
    }
    public void Initialise(PackageData packageData)
    {
        _PackageData = packageData;
        _NameText.text = _PackageData.Name;
    }
    public void switchOpen()
    {
        _DragComponent.PointerDownCallback -= open;
        _DragComponent.PointerDownCallback += firstOpen;
    }
    private void OnMouseDown()
    {
       
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void SpawnAllCardsInGrid(PointerEventData pointerEventData)
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
        Destroy(gameObject);
    }

    private  CardRarity GetRandomRarity()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f); 
        Array rarity = Enum.GetValues(typeof(CardRarity));
        float cumulativeProbability = 0f;
        for (int i = 0; i < rarity.Length; i++)
        {
            cumulativeProbability +=(int)rarity.GetValue(i);
            if (randomValue < cumulativeProbability)
            {
                return (CardRarity)rarity.GetValue(i); // 返回对应的稀有度
            }
        }

        return CardRarity.Common; // 默认返回普通卡
    }
    private void firstOpen(PointerEventData pointerEventData)
    {
        float radius = 3f;
        string[] names = { "Villager", "Pickaxe", "Rock", "Apple", "Smeltery" , "Miner Formula","Apple", "Rock Formula" };
        CardData cardData = null;
        for (int i = 0; i < 8; i++)
        {
            // 计算每个顶点的角度
            float angle = i * 45f;  // ，每个顶点的角度
            float angleInRadians = Mathf.Deg2Rad * angle; // 转为弧度

            // 计算顶点的 x 和 y 坐标
            float x = transform.position.x + radius * Mathf.Cos(angleInRadians);
            float y = transform.position.y + radius * Mathf.Sin(angleInRadians);

            // 创建一个新的物体在每个顶点的位置
            Vector3 position = new Vector3(x, y, 0);

            cardData = CardManager.LoadedCards[names[i]];

            if (cardData != null)
            {
                CardManager.SpawnCard(cardData, position);
            }

        }
        Destroy(gameObject);

    }
    private void open(PointerEventData pointerEventData) 
    {

        float radius = 3f;
        for (int i = 0; i < 5; i++)
        {
            // 计算每个顶点的角度
            float angle = i * 72f;  // 360° / 5 = 72°，每个顶点的角度
            float angleInRadians = Mathf.Deg2Rad * angle; // 转为弧度

            // 计算顶点的 x 和 y 坐标
            float x = transform.position.x + radius * Mathf.Cos(angleInRadians);
            float y = transform.position.y + radius * Mathf.Sin(angleInRadians);

            // 创建一个新的物体在每个顶点的位置
            Vector3 position = new Vector3(x, y, 0);
            CardRarity cardRarity = GetRandomRarity();
            CardData cardData = null;
            System.Random rand = new System.Random();
            switch (cardRarity)
            {
                case CardRarity.Common:
                if (_PackageData.CommonCards.Count > 0)
                {
                        
                    int randomIndex = rand.Next(_PackageData.CommonCards.Count);
                    cardData = CardManager.LoadedCards[_PackageData.CommonCards[randomIndex].CardName];
                    if (cardData.CardClass == CardClass.Formula)
                    {
                        _PackageData.CommonCards.Remove(_PackageData.CommonCards[randomIndex]);
                    }

                }
                break;
                case CardRarity.Uncommon:
                if (_PackageData.UncommonCards.Count > 0)
                {
                        
                    int randomIndex = rand.Next(_PackageData.UncommonCards.Count);
                    cardData = CardManager.LoadedCards[_PackageData.UncommonCards[randomIndex].CardName];
                    if (cardData.CardClass == CardClass.Formula)
                    {
                        _PackageData.UncommonCards.Remove(_PackageData.UncommonCards[randomIndex]);
                    }
                }
                break;

                case CardRarity.Rare:
                if (_PackageData.RareCards.Count > 0)
                {
                        
                    int randomIndex = rand.Next(_PackageData.RareCards.Count);
                    cardData = CardManager.LoadedCards[_PackageData.RareCards[randomIndex].CardName];
                    if (cardData.CardClass == CardClass.Formula)
                    {
                        _PackageData.RareCards.Remove(_PackageData.RareCards[randomIndex]);
                    }
                }
                    
                break;

                case CardRarity.Legendary:
                if (_PackageData.LegendaryCards.Count > 0)
                {
                       
                    int randomIndex = rand.Next(_PackageData.LegendaryCards.Count);
                    cardData = CardManager.LoadedCards[_PackageData.LegendaryCards[randomIndex].CardName];
                    if (cardData.CardClass == CardClass.Formula)
                    {
                        _PackageData.LegendaryCards.Remove(_PackageData.LegendaryCards[randomIndex]);
                    }
                }
                break;
            }
            if (cardData != null)
            {
                CardManager.SpawnCard(cardData, position);
            }
        }
        Destroy(gameObject);
    }


}
