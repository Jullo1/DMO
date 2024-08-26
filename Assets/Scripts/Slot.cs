using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Card container;
    public Zone location;
    public int slotNumber;
    Text descriptionUI;
    Hand hand;
    Field field;

    void Awake()
    {
        descriptionUI = GameObject.FindGameObjectWithTag("CardTextUI").gameObject.GetComponent<Text>();
        hand = GetComponentInParent<Hand>();
        field = GetComponentInParent<Field>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (container)
            if (container.isFaceUp || container.ownedByPlayer) descriptionUI.text = container.fullCardText;

        if (location == Zone.Hand) //change ui layer to bring card to front
            transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (location == Zone.Hand)
            transform.SetSiblingIndex(slotNumber - 1);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (container)
            switch (location)
            {
                case Zone.Hand:
                    hand.PlayCard(slotNumber, Input.GetMouseButtonUp(1)); //right click = set
                    break;
                case Zone.Field:
                    field.UseCard(slotNumber, Input.GetMouseButtonUp(1)); //right click = activate effect
                    break;
            }
    }

    public void RemoveCard()
    {
        Destroy(container.gameObject);
    }

    public void AddCard(Card card)
    {
        container = card;
        container.gameObject.transform.SetAsFirstSibling();
    }
}
