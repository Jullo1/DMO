using UnityEngine;
using UnityEngine.EventSystems;

public class Hand : Collection, IPointerClickHandler
{
    public bool canNormalSummon;

    public void PlayCard(int handIndex, bool set)
    {
        Card selectedCard = cardList[handIndex];

        if ((engine.playerTurn && tag == "Player") || (!engine.playerTurn && tag == "Opponent"))
        {
            if (engine.currentPhase == Phase.Main || engine.currentPhase == Phase.Main2)
            {
                if (selectedCard is Monster)
                {
                    if (canNormalSummon)
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
                    else { engine.AlertText("Can only normal summon 1 monster per turn", true); engine.PlaySound("cant"); }
                }
                else if (selectedCard is SpellTrap)
                {
                    SpellTrap selectedSpellTrap = selectedCard as SpellTrap;
                    if (!set)
                    {
                        if (!selectedSpellTrap.usesTarget)
                        {
                            engine.MoveCard(selectedSpellTrap, Zone.Field, set, tag == "Player");
                            selectedSpellTrap.TriggerEffects();
                        }
                        else if (selectedSpellTrap.targetZone == Zone.Field)
                        {
                            if (CheckValidTarget(selectedSpellTrap))
                            {
                                engine.MoveCard(selectedSpellTrap, Zone.Field, set, tag == "Player");
                                engine.InitiateSelectTarget(selectedSpellTrap);
                            }
                        }
                        else if (selectedSpellTrap.targetZone == Zone.Graveyard)
                        {
                            bool activate = false;
                            foreach (Graveyard graveyard in FindObjectsOfType<Graveyard>())
                            {
                                if (selectedSpellTrap.cantTargetPlayerCards && graveyard.tag == "Player") continue;
                                else if (selectedSpellTrap.cantTargetOpponentCards && graveyard.tag == "Opponent") continue;

                                if (graveyard.CheckValidTarget(selectedSpellTrap)) //check if card can target both players, then show both graveyards
                                {
                                    activate = true;
                                    graveyard.ManualTrigger(true, (!selectedSpellTrap.cantTargetOpponentCards && !selectedSpellTrap.cantTargetPlayerCards));
                                }
                            }
                            if (activate)
                            {
                                engine.MoveCard(selectedSpellTrap, Zone.Field, set, tag == "Player");
                                engine.InitiateSelectTarget(selectedSpellTrap);
                            }
                        }
                    }
                    else engine.MoveCard(selectedSpellTrap, Zone.Field, set, tag == "Player"); //set spell/trap card

                    return;
                }
            } else { engine.AlertText("You can only play cards in your Main Phase", true); engine.PlaySound("cant"); }
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

    public bool CheckValidTarget(SpellTrap card)
    {
        foreach (Field field in FindObjectsOfType<Field>())
        {
            if (card.cantTargetPlayerCards && field.tag == "Player") continue;
            else if (card.cantTargetOpponentCards && field.tag == "Opponent") continue;

            foreach (Slot slots in field.monsterSlots)
            {
                if (!slots.container) continue;
                Monster monster = slots.container.GetComponent<Monster>();
                if (monster.type == card.requiredType && monster.isFaceUp) return true;
                else if (card.effectType == EffectType.ChangePosition && monster.isAttackPosition == card.requiresAtkPos) return true;
            }
        }

        engine.AlertText("No valid target", true);
        engine.PlaySound("cant");
        return false;
    }
}
