using UnityEngine;

public class Hand : Collection
{
    public bool canNormalSummon;

    public void PlayCard(int handIndex, bool set)
    {
        if ((engine.playerTurn && tag == "Player") || (!engine.playerTurn && tag == "Opponent"))
        {
            if (engine.currentPhase == Phase.Main || engine.currentPhase == Phase.Main2)
            {
                if (canNormalSummon)
                {
                    Card selectedCard = slotList[handIndex - 1].container;
                    if (selectedCard is Monster)
                    {
                        Monster monsterCard = (Monster)selectedCard;
                        if (monsterCard.level <= 4)
                        {
                            engine.MoveCard(selectedCard, Zone.Field, set, (tag == "Player"));
                            engine.CancelTribute(false);
                            canNormalSummon = false;
                        }
                        else if (monsterCard.level <= 6)
                        {
                            engine.AlertText("Select 1 card to tribute");
                            engine.InitiateTribute((Monster)selectedCard, 1, set);
                        }
                        else //level 7 or higher
                        {
                            engine.AlertText("Select 2 cards to tribute");
                            engine.InitiateTribute((Monster)selectedCard, 2, set);
                        }
                    }
                }
                else engine.AlertText("Can only normal summon 1 monster per turn", true);
            } else engine.AlertText("You can only play monsters in your Main Phase", true);
        } else if (tag == "Player") engine.AlertText("It's not your turn yet!", true);
    }

    public override void AddCard(Card card, bool shuffle = true)
    {
        count++;
        foreach (Slot slot in slotList)
            if (!slot.container)
            {
                slot.AddCard(Instantiate(card, slot.transform));
                slot.container.ToggleFaceUp(true);
                if (tag == "Opponent") slot.container.cardBack.SetActive(true);
                break;
            }
    }
}
