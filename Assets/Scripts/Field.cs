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
            else Debug.Log("It's not your turn yet!");
    }

    public void PlayMonster(Monster card, bool set)
    {
        card.TogglePosition(!set, true);
        card.ToggleFaceUp(!set, true);

        if (!monsterSlots[2].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, monsterSlots[2].gameObject.transform);
            monsterSlots[2].AddCard(newCardObject.GetComponent<Monster>());
        }
        else if (!monsterSlots[3].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, monsterSlots[3].gameObject.transform);
            monsterSlots[3].AddCard(newCardObject.GetComponent<Monster>());
        }
        else if (!monsterSlots[1].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, monsterSlots[1].gameObject.transform);
            monsterSlots[1].AddCard(newCardObject.GetComponent<Monster>());
        }
        else if (!monsterSlots[4].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, monsterSlots[4].gameObject.transform);
            monsterSlots[4].AddCard(newCardObject.GetComponent<Monster>());
        }
        else if (!monsterSlots[0].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, monsterSlots[0].gameObject.transform);
            monsterSlots[0].AddCard(newCardObject.GetComponent<Monster>());
        }

        previousTributeSlot = -1;
        if (card.cardName == "Blue-Eyes White Dragon") engine.ChangeBackgroundMusic(4);
        else if (card.cardName == "Dark Magician") engine.ChangeBackgroundMusic(3);
    }

    public void SetSpellTrap(Card card)
    {
        if (!spellTrapSlots[2].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, spellTrapSlots[2].gameObject.transform);
            spellTrapSlots[2].AddCard(newCardObject.GetComponent<Monster>());
        }
        else if (!spellTrapSlots[3].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, spellTrapSlots[3].gameObject.transform);
            spellTrapSlots[3].AddCard(newCardObject.GetComponent<Monster>());
        }
        else if (!spellTrapSlots[1].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, spellTrapSlots[1].gameObject.transform);
            spellTrapSlots[1].AddCard(newCardObject.GetComponent<Monster>());
        }
        else if (!spellTrapSlots[4].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, spellTrapSlots[4].gameObject.transform);
            spellTrapSlots[4].AddCard(newCardObject.GetComponent<Monster>());
        }
        else if (!spellTrapSlots[0].container)
        {
            GameObject newCardObject = Instantiate(card.gameObject, spellTrapSlots[0].gameObject.transform);
            spellTrapSlots[0].AddCard(newCardObject.GetComponent<Monster>());
        }
    }

    public bool CheckFull(Card card) //check for full field
    {
        if (card.GetType() == typeof(Monster))
            foreach (Slot mSlot in monsterSlots)
                if (!mSlot.container)
                    return false;

        else if (card.GetType() == typeof(SpellTrap))
            foreach (Slot stSlot in spellTrapSlots)
                if (!stSlot.container)
                    return false;

        return true;
    }
}
