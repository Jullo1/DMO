public class FusionDeck : Collection
{

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
            if (tag == "Player") cardList[i].ownedByPlayer = true;
            else if (tag == "Opponent") cardList[i].ownedByPlayer = false;
        }
    }
}
