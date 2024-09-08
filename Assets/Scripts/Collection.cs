using System.Collections.Generic;
using UnityEngine;

public class Collection : MonoBehaviour
{
    protected GameManager game;
    protected DuelEngine engine;
    public List<Card> cardList = new List<Card>();

    protected virtual void Awake()
    {
        engine = FindObjectOfType<DuelEngine>();
        game = FindObjectOfType<GameManager>();
    }

    public virtual void AddCard(Card card)
    {
        if (card.index != -1) card.GetComponentInParent<Collection>().RemoveCard(card); //remove card from list if it's in a collection (not in field slot)
        else card.GetComponentInParent<Slot>().RemoveCard();

        card.transform.SetParent(transform);
        card.transform.position = transform.position;
        card.transform.rotation = transform.rotation;
        cardList.Add(card);
        card.index = cardList.Count - 1;
        card.gameObject.transform.SetAsLastSibling();
    }

    public void SwapCard(int index1, int index2)
    {
        cardList.Add(cardList[index1]);

        cardList[index1] = cardList[index2];
        cardList[index2] = cardList[cardList.Count-1];

        cardList.RemoveAt(cardList.Count - 1);
    }
    
    public virtual void RemoveCard(Card card)
    {
        cardList.Remove(card); //remove card from the card list to keep track of collection

        for (int i = 0; i < cardList.Count; i++)
            cardList[i].index = i; //update index for all other cards
    }
}
