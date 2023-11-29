using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collection : MonoBehaviour
{
    public int count;
    protected DuelEngine engine;
    public List<Slot> slotList = new List<Slot>();

    protected virtual void Awake()
    {
        engine = FindObjectOfType<DuelEngine>();
    }

    public virtual void AddCard(Card card, bool shuffle = false)
    {
        foreach (Slot slot in slotList)
            if (!slot.container)
            {
                slot.AddCard(Instantiate(card, slot.transform));
                slot.container.ToggleFaceUp(false);
                break;
            }
    }

    public void SwapCard(Slot slot1, Slot slot2)
    {
        Card card1 = Instantiate(slot1.container, slot2.gameObject.transform);
        Card card2 = Instantiate(slot2.container, slot1.gameObject.transform);

        Destroy(slot1.container.gameObject);
        Destroy(slot2.container.gameObject);

        slot1.container = card2;
        slot2.container = card1;
    }
}
