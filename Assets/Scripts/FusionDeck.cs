using System.Collections.Generic;

public class FusionDeck : Collection
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
            AddCard(cardList[i]);
            if (tag == "Player") slotList[i].container.ownedByPlayer = true;
            else if (tag == "Opponent") cardList[i].ownedByPlayer = false;
        }
    }
}
