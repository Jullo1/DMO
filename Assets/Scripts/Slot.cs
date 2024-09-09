using UnityEngine;

public class Slot : MonoBehaviour
{
    public Card container;
    public Zone location;
    public int slotNumber;
    Field field;

    void Awake()
    {
        field = GetComponentInParent<Field>();
    }

    public void UseCard()
    {
        if (!container) return;

        if (container.GetType() == typeof(Monster))
            field.UseCard(slotNumber, Input.GetMouseButtonUp(1)); //right click = activate effect  
        else if (container.GetType() == typeof(SpellTrap))
        {
            if(field.CheckEquipTarget(container as SpellTrap))
            {
                container.ToggleFaceUp(true);
                FindObjectOfType<DuelEngine>().PlaySound("play");
                //trigger effect script here
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
        container.gameObject.transform.SetAsFirstSibling();
    }
}
