public class Deck : Collection
{
    protected override void Awake()
    {
        base.Awake();
        LoadDeck();
    }

    public void LoadDeck()
    {
        for (int i = 0; i < cardList.Count; i++) //Instantiate the cards in the board using the cardList in each deck
        {
            Card card = Instantiate(cardList[i], transform);
            card.index = i;
            card.ToggleFaceUp(false);
            cardList[i] = card;
            if (tag == "Player") cardList[i].ownedByPlayer = true;
            else if (tag == "Opponent") cardList[i].ownedByPlayer = false;
        }
        Shuffle();
    }

    void Shuffle()
    {
        for (int i = 0; i < cardList.Count; i++)
            SwapCard(i, UnityEngine.Random.Range(0, cardList.Count - 1));
    }

}