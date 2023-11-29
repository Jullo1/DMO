using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string cardName;
    public string cardText;

    public bool ownedByPlayer;
    public bool isFaceUp;

    public GameObject cardBack;
    Text descriptionUI;
    protected string fullCardText;
    protected Quaternion startingRotation;

    void Awake()
    {
        startingRotation = transform.rotation;
        descriptionUI = GameObject.FindGameObjectWithTag("CardTextUI").gameObject.GetComponent<Text>();
        fullCardText = cardName + "\n" + cardText;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isFaceUp) descriptionUI.text = fullCardText;
        transform.parent.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.name != "SetButton")
            transform.parent.SetSiblingIndex(GetComponentInParent<Slot>().slotNumber);
    }

    public void ToggleFaceUp(bool faceUp, bool isForced = false)
    { 
        cardBack.SetActive(!faceUp);
        this.isFaceUp = faceUp;
    }
}
