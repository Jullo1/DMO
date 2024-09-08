using UnityEngine;
using UnityEngine.EventSystems;

public class Hand : Collection, IPointerClickHandler
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
                    Card selectedCard = cardList[handIndex];
                    if (selectedCard is Monster)
                    {
                        Monster monsterCard = (Monster)selectedCard;
                        if (monsterCard.level <= 4)
                        {
                            engine.MoveCard(selectedCard, Zone.Field, set, (tag == "Player"));
                            engine.CancelTribute(false);
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

    public override void AddCard(Card card)
    {
        card.ToggleFaceUp(tag != "Opponent"); //opponent cards are face down
        base.AddCard(card);
        RearrangeCards();
    }

    public override void RemoveCard(Card card)
    {
        base.RemoveCard(card);
        RearrangeCards();
    }

    void RearrangeCards()
    {
        for (int i = 0; i < cardList.Count; i++)
            cardList[i].gameObject.transform.localPosition = new Vector3((i - ((float)cardList.Count / 2)) * 50, cardList[i].gameObject.transform.position.y, 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Card selectedCard = eventData.pointerCurrentRaycast.gameObject.GetComponent<Card>();
        PlayCard(selectedCard.index, eventData.button == PointerEventData.InputButton.Right); //right click = set
    }
}
