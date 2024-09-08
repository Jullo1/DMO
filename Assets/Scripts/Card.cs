using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string cardName;
    public string cardText;

    public bool ownedByPlayer;
    public bool isFaceUp;

    Image cardImage;
    public GameObject cardBack;
    public string fullCardText;

    public int index = -1;
    GameManager game;

    void Awake()
    {
        game = FindObjectOfType<GameManager>();
        cardImage = GetComponent<Image>();
        fullCardText = cardName + "\n" + cardText;
        cardBack.GetComponent<Image>().raycastTarget = false; //disable cardback raycast
    }

    public void ToggleFaceUp(bool faceUp)
    {
        cardBack.SetActive(!faceUp);
        isFaceUp = faceUp;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isFaceUp || ownedByPlayer) game.ChangeTextUI(fullCardText);

        if (index != -1) //if not in a slot
        {
            transform.SetAsLastSibling(); //bring card to front layer for preview, if not already
            cardImage.raycastPadding = new Vector4(0, 0, 40, 0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (index != -1) //if not in a slot
        {
            transform.SetSiblingIndex(index); //reset to original layer level
            cardImage.raycastPadding = new Vector4(0, 0, 0, 0);
        }
    }
}
