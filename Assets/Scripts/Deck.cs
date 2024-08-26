using System.Collections.Generic;

public class Deck : Collection
{
    public List<Card> cardList = new List<Card>();

    protected override void Awake()
    {
        base.Awake();
        LoadDeck();
    }

    public void LoadDeck()
    {
        for (int i = 0; i < cardList.Count; i++) //add multiple cards without shuffling on every iteration
        {
            AddCard(cardList[i], false);
            if (tag == "Player") slotList[i].container.ownedByPlayer = true;
            else if (tag == "Opponent") cardList[i].ownedByPlayer = false;
        }
        Shuffle();
    }

    public override void AddCard(Card card, bool shuffle = false)
    {
        count++;
        base.AddCard(card);
        if (shuffle) Shuffle();
    }

    void Shuffle()
    {
        for (int i = 0; i < count; i++)
        {
            if (!slotList[i].container) return; //no more cards

            int num = UnityEngine.Random.Range(0, count - 1);
            if (num != i) SwapCard(slotList[i], slotList[num]);
        }
    }

}