using UnityEngine;

public class Card : MonoBehaviour
{
    public string cardName;
    public string cardText;

    public bool ownedByPlayer;
    public bool isFaceUp;

    public GameObject cardBack;
    public string fullCardText;
    protected Quaternion startingRotation;

    void Awake()
    {
        startingRotation = transform.rotation;
        fullCardText = cardName + "\n" + cardText;
    }

    public void ToggleFaceUp(bool faceUp, bool isForced = false)
    { 
        cardBack.SetActive(!faceUp);
        this.isFaceUp = faceUp;
    }
}
