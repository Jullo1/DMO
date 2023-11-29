using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graveyard : Collection
{
    public override void AddCard(Card card, bool shuffle = true)
    {
        count++;
        foreach (Slot slot in slotList)
            if (!slot.container)
            {
                slot.AddCard(Instantiate(card, slot.transform));
                slot.container.ToggleFaceUp(true);
                break;
            }
    }
}
