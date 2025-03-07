using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using Utility;

public class SellRegion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        GameObject cardObject = other.transform.parent.gameObject;
        if (cardObject != null && cardObject.CompareTag("Card"))
        {

            Card card = cardObject.GetComponent<Card>();
            if (card != null )
            {
                int value = 0;
                foreach (Card c in card.StackNode.Stack.EnumeratorFrom(card.StackNode.Node))
                {
                   if(!c.Sellable)return;
                    value += c.Data.Value;
                }

                while (card.StackNode.Next!=null)
                {

                    Card c = card;
                    card = card.StackNode.Next.Value;
                    CardManager.DestroyCards(c);
                }
                CardManager.DestroyCards(card);
                
                if(value > 0)
                {
                    Vector3 position=transform.position;
                    position.y -= 2;
                    
                    for(int i = 0;i<value;i++)
                        CardManager.SpawnCard("Coin", position);
                }
            }


        }
    }
}
