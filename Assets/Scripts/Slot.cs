using UnityEngine;

public class Slot : MonoBehaviour
{
    public Card container;
    public Zone location;
    public int slotNumber;
    Field field;
    DuelEngine engine;

    void Awake()
    {
        field = GetComponentInParent<Field>();
        engine = FindObjectOfType<DuelEngine>();
    }

    public void UseCard()
    {
        if (!container) return;

        if (container.GetType() == typeof(Monster))
            field.UseCard(slotNumber, Input.GetMouseButtonUp(1)); //right click = activate effect  
        else if (container.GetType() == typeof(SpellTrap) && !container.isFaceUp)
        {
            SpellTrap spellTrap = container as SpellTrap;
            if (!spellTrap.usesTarget)
            {
                spellTrap.ToggleFaceUp(true);
                spellTrap.TriggerEffects(true);
            }
            else if (spellTrap.targetZone == Zone.Field)
            {
                if (field.CheckValidTarget(spellTrap))
                {
                    spellTrap.ToggleFaceUp(true);
                    engine.InitiateSelectTarget(spellTrap);
                }
            }
            else if (spellTrap.targetZone == Zone.Graveyard)
            {
                foreach (Graveyard graveyard in FindObjectsOfType<Graveyard>())
                {
                    if (spellTrap.cantTargetPlayerCards && graveyard.tag == "Player") continue;
                    else if (spellTrap.cantTargetOpponentCards && graveyard.tag == "Opponent") continue;

                    if (graveyard.CheckValidTarget(spellTrap))
                    {
                        spellTrap.ToggleFaceUp(true);
                        graveyard.ManualTrigger(true, (!spellTrap.cantTargetOpponentCards && !spellTrap.cantTargetPlayerCards)); //if card can target both players, then trigger both graveyards
                        engine.InitiateSelectTarget(spellTrap);
                    }
                }
            }
        }

    }

    public void RemoveCard()
    {
        container = null;
    }

    public void AddCard(Card card)
    {
        card.GetComponentInParent<Collection>().RemoveCard(card);

        container = card;
        container.transform.SetParent(transform, false);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.gameObject.transform.SetAsLastSibling();
    }
}
