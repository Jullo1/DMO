using UnityEngine;
using UnityEngine.EventSystems;

public class Hand : Collection, IPointerClickHandler
{
    [SerializeField] Field field;
    public bool canNormalSummon;

    public void PlayCard(int handIndex, bool set)
    {
        Card selectedCard = cardList[handIndex];

        if (selectedCard.GetComponent<SpellTrap>())
        {
            SpellTrap selectedSpellTrap = selectedCard as SpellTrap;
            if (!set)
            {
                if (CheckEquipTarget(selectedSpellTrap))
                {
                    engine.MoveCard(selectedSpellTrap, Zone.Field, set, (tag == "Player"));
                    /*ChooseTarget();
                    selectedCard.GetComponent<SpellTrap>().TriggerEffects(true);*/
                }
            } else engine.MoveCard(selectedSpellTrap, Zone.Field, set, (tag == "Player"));

            return;
        }

        if ((engine.playerTurn && tag == "Player") || (!engine.playerTurn && tag == "Opponent"))
        {
            if (engine.currentPhase == Phase.Main || engine.currentPhase == Phase.Main2)
            {
                if (canNormalSummon)
                {
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

    public bool CheckEquipTarget(SpellTrap card)
    {
        bool targetAvailable = false;
        foreach (Slot slots in field.monsterSlots)
        {
            if (!slots.container) continue;
            Monster monster = slots.container.GetComponent<Monster>();
            if (monster.type == card.requiredType) targetAvailable = true;
        }

        if (!targetAvailable) engine.AlertText("No valid target", true);
        return targetAvailable;
    }

    void ChooseTarget()
    {
        engine.AlertText("Choose a target");

        /*switch (card.GetType())
        {
            case (istypeof(EquipSpell)):
                EquipSpell equip = (EquipSpell)card;
                playerField.selectedMonsterSlot = -1;
                do
                {
                    if (playerField.selectedMonsterSlot == -1) continue;
                    equip.target = (Monster)playerField.monsterSlots[playerField.selectedMonsterSlot].container;
                } while (target.type != equip.requiredType);
                equip.TriggerEffects(true);
                break;
        }*/
    }
}
