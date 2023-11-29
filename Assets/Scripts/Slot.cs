using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Card container;
    public Zone location;
    public int slotNumber;
    protected List<Button> controls = new List<Button>();

    void Awake()
    {
        foreach (Button button in GetComponentsInChildren<Button>())
        {
            if (button.tag == "Controls")
            {
                controls.Add(button);
                button.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (container)
        {
            foreach (Button button in controls)
                button.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (Button button in controls)
            button.gameObject.SetActive(false);
    }

    public void RemoveCard()
    {
        foreach (Button button in controls)
            button.gameObject.SetActive(false);
        Destroy(container.gameObject);
    }

    public void AddCard(Card card)
    {
        container = card;
        container.gameObject.transform.SetAsFirstSibling();
    }
}
