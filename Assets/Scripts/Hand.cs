using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public class Hand : Collection
{
    public bool canNormalSummon;

    public void PlayCard(int handIndex)
    {
        UseCard(handIndex, false);
    }

    public void SetCard(int handIndex)
    { 
        UseCard(handIndex, true);
    }

    void UseCard(int handIndex, bool set)
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
                            engine.CancelTribute();
                            canNormalSummon = false;
                        }
                        else if (monsterCard.level <= 6)
                        {
                            Debug.Log("Select 1 card to tribute");
                            engine.InitiateTribute((Monster)selectedCard, 1, set);
                        }
                        else //level 7 or higher
                        {
                            Debug.Log("Select 2 cards to tribute");
                            engine.InitiateTribute((Monster)selectedCard, 2, set);
                        }
                    }
                }
                else Debug.Log("Can only normal summon 1 monster per turn");
            } else Debug.Log("You can only play monsters in your Main Phase");
        } else Debug.Log("It's not your turn yet!");
    }

    public override void AddCard(Card card, bool shuffle = true)
    {
        count++;
        foreach (Slot slot in slotList)
            if (!slot.container)
            {
                slot.AddCard(Instantiate(card, slot.transform));
                slot.container.ToggleFaceUp(true);
                break;
            }
    }
}
