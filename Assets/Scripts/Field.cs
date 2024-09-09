using UnityEngine;

public class Field : MonoBehaviour
{
    DuelEngine engine;
    public Slot[] monsterSlots = new Slot[5];
    public Slot[] spellTrapSlots = new Slot[5];
    public int previousTributeSlot = -1;

    void Awake()
    {
        engine = FindObjectOfType<DuelEngine>();
    }

    public void UseCard(int fieldIndex, bool activateEffect)
    {
        if (activateEffect)
        {
            //card effect code here
            return;
        }

        if (engine.currentPhase == Phase.Battle)
            Attack(fieldIndex);

        else if (engine.currentPhase == Phase.Main || engine.currentPhase == Phase.Main2)
        {
            if (engine.tributesLeft > 0)
            {
                if (fieldIndex != previousTributeSlot)
                {
                    engine.SelectTribute((Monster)monsterSlots[fieldIndex - 1].container);
                    previousTributeSlot = fieldIndex;
                }
                else
                {
                    engine.CancelTribute();
                    previousTributeSlot = -1;
                }
            }
            else
            {
                ChangePosition(fieldIndex);
            }
        }
    }

    public void Attack(int fieldIndex)
    {
        if (monsterSlots[fieldIndex - 1].container)
        {
            if ((engine.playerTurn && tag == "Player") || (!engine.playerTurn && tag == "Opponent")) //if it's the current player's turn and selecting a card on their field, setup battle
            {
                engine.InitiateAttack((Monster)monsterSlots[fieldIndex - 1].container);
            }

            if ((engine.playerTurn && tag == "Opponent") || (!engine.playerTurn && tag == "Player")) //if it's the current player's turn and selecting a card on their opponent's field, initiate battle (engine will look for a previously selected card, if it's null, nothing will happen)
            {
                engine.Attack(fieldIndex);
            }
        }
    }

    public void ChangePosition(int fieldIndex)
    {
        if (monsterSlots[fieldIndex-1].container)
            if ((engine.playerTurn && tag == "Player") || (!engine.playerTurn && tag == "Opponent"))
            {
                Monster selectedCard = (Monster)monsterSlots[fieldIndex - 1].container;
                selectedCard.TogglePosition(!selectedCard.isAttackPosition);
            }
            else if (tag == "Player") FindObjectOfType<DuelEngine>().AlertText("It's not your turn yet!", true);
    }

    public void PlayMonster(Monster card, bool set)
    {
        FindObjectOfType<DuelEngine>().AlertText("");
        previousTributeSlot = -1;

        if (!monsterSlots[2].container)
        {
            monsterSlots[2].AddCard(card);
        }
        else if (!monsterSlots[3].container)
        {
            monsterSlots[3].AddCard(card);
        }
        else if (!monsterSlots[1].container)
        {
            monsterSlots[1].AddCard(card);
        }
        else if (!monsterSlots[4].container)
        {
            monsterSlots[4].AddCard(card);
        }
        else if (!monsterSlots[0].container)
        {
            monsterSlots[0].AddCard(card);
        }
        card.TogglePosition(!set, true);
        card.ToggleFaceUp(!set);
        card.index = -1;

        if (set) engine.PlaySound("send");
        else engine.PlaySound("play");

        /*if (card.cardName == "Blue-Eyes White Dragon") engine.ChangeBackgroundMusic(4);
        else if (card.cardName == "Dark Magician") engine.ChangeBackgroundMusic(3);*/
    }

    public void PlaySpellTrap(Card card, bool set)
    {
        card.index = -1;

        if (!spellTrapSlots[2].container)
        {
            spellTrapSlots[2].AddCard(card);
        }
        else if (!spellTrapSlots[3].container)
        {
            spellTrapSlots[3].AddCard(card);
        }
        else if (!spellTrapSlots[1].container)
        {
            spellTrapSlots[1].AddCard(card);
        }
        else if (!spellTrapSlots[4].container)
        {
            spellTrapSlots[4].AddCard(card);
        }
        else if (!spellTrapSlots[0].container)
        {
            spellTrapSlots[0].AddCard(card);
        }
        card.ToggleFaceUp(!set);
    }

    public bool CheckFull(Card card, bool monster) //check for full field
    {
        if (monster)
        {
            foreach (Slot mSlot in monsterSlots)
                if (!mSlot.container)
                    return false;
        }
        else
        {
            foreach (Slot stSlot in spellTrapSlots)
                if (!stSlot.container)
                    return false;
        }
        return true;
    }

    public bool CheckEquipTarget(SpellTrap card)
    {
        bool targetAvailable = false;
        foreach (Slot slots in monsterSlots)
        {
            if (!slots.container) continue;
            Monster monster = slots.container.GetComponent<Monster>();
            if (monster.type == card.requiredType) targetAvailable = true;
        }

        if (!targetAvailable) engine.AlertText("No valid target", true);
        return targetAvailable;
    }
}
