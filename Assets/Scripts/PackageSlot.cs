using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackageSlot : MonoBehaviour
{
    public Button Button;
    public Image TitleBacking;
    public TextMeshProUGUI PackageName;
    public TextMeshProUGUI PriceText;
    public PackageData PackageData;
    public GameObject PackagePrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (Button != null)
        {
            // 绑定点击事件
            Button.onClick.AddListener(OnButtonClick);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialise(PackageData packageData)
    {
        PackageData = packageData;
        PriceText.text = packageData.Price.ToString();
        PackageName.text = packageData.Name;
    }

    void OnButtonClick()
    {
        Debug.Log("Click");
        if (PackageData != null && CardManager.Money >= PackageData.Price)
        {
            int price = PackageData.Price;
            
                while (price > 0) 
                {
                    Card card = CardManager.CardsByClass[Data.CardClass.Money][0];
                    price -= card.Data.Value;
                    CardManager.DestroyCards(card);
                }

            Vector3 position=transform.position;
            position.y -= 3;
            GameObject package = Instantiate(PackagePrefab, position, Quaternion.identity,GameManager.PackageContainer);
            CardPackage cardPackage = package.GetComponent<CardPackage>();
            if (cardPackage != null)
            {
                cardPackage.Initialise(PackageData);
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
       // GameObject card = other.transform.parent.gameObject;
        //if (card!=null&&card.CompareTag("Card"))
        {
         
            
           
        }
    }

}
