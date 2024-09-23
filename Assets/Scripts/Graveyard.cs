using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Graveyard : Collection, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    bool preview;
    bool forcedPreview;

    public override void AddCard(Card card)
    {
        Monster monster = card.GetComponent<Monster>();

        if (monster)
            foreach (Card equip in monster.equips)
                AddCard(equip);

        base.AddCard(card);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (preview || forcedPreview) return;
        preview = true;

        ArrangeCards(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!preview || forcedPreview) return;
        preview = false;

        ArrangeCards(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!engine.activatedCard) return;
        SpellTrap spellTrap = engine.activatedCard as SpellTrap;
        Card targetCard =  eventData.pointerCurrentRaycast.gameObject.GetComponent<Card>();

        if (spellTrap.targetType == Target.Monster && targetCard.GetType() == typeof(Monster))
        {
            spellTrap.target = targetCard;
            spellTrap.TriggerEffects(true);
            engine.activatedCard = null;
            engine.AlertText("");
            engine.PlaySound("magic");
            ManualTrigger(false);
        }
        else engine.AlertText("Invalid target", true);
    }

    void ArrangeCards(bool state)
    {
        if (state)
        {
            for (int i = 0; i < cardList.Count; i++)
                cardList[i].transform.position += new Vector3(1, 0.5f, 0) * 0.5f * i;
        }
        else
        {
            foreach (Card card in cardList)
                card.transform.position = transform.position;
        }
    }

    public void ManualTrigger(bool state, bool selectBoth = false) //cards that target the graveyard should call this function
    {
        if (!state || selectBoth) //if turning off, turn off both
        {
            foreach (Graveyard graveyard in FindObjectsOfType<Graveyard>())
            {
                if (preview)
                {
                    graveyard.ArrangeCards(false);
                    graveyard.preview = false;
                }
                graveyard.forcedPreview = state;
                graveyard.ArrangeCards(state);
            }
        }
        else //only turn on the selected one
        {
            if (preview)
            {
                ArrangeCards(false);
                preview = false;
            }
            forcedPreview = state;
            ArrangeCards(state);
        }
    }

    public bool CheckValidTarget(SpellTrap card)
    {
        foreach (Graveyard graveyard in FindObjectsOfType<Graveyard>())
        {
            if (card.cantTargetPlayerCards && graveyard.tag == "Player") continue;
            else if (card.cantTargetOpponentCards && graveyard.tag == "Opponent") continue;

            foreach (Card target in graveyard.cardList)
            {
                if (target.GetType() == typeof(Monster) && card.targetType == Target.Monster) return true;
                if (target.GetType() == typeof(SpellTrap) && card.targetType == Target.SpellTrap) return true;
            }
        }

        engine.AlertText("No valid target", true);
        engine.PlaySound("cant");
        return false;
    }
}
