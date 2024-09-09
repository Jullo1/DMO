public class Graveyard : Collection
{
    public override void AddCard(Card card)
    {
        Monster monster = card.GetComponent<Monster>();

        if (monster)
            foreach (Card equip in monster.equips)
                AddCard(equip);

        base.AddCard(card);
    }
}
