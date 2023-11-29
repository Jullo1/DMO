using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class Monster : Card
{
    public int atk;
    public int def;
    public int level;

    public bool isAttackPosition;
    public bool canChangePos;
    public bool hasBattled;

    void Start()
    {
        fullCardText += "\n\nATK/" + atk.ToString() + "  DEF/" + def.ToString();

        if (!isFaceUp) //fix starting rotation if it's a monster that was set
        {
            transform.Rotate(0, 0, 90);
            startingRotation = transform.rotation;
            transform.Rotate(0, 0, -90);
        }
    }

    public void TogglePosition(bool isAttack, bool isForced = false)
    {
        if (GetComponentInParent<Slot>().location == Zone.Field || isForced)
        {
            if ((canChangePos && (!hasBattled)) || isForced) //if is changed by engine
            {
                isAttackPosition = isAttack;
                if (isAttack && !isFaceUp) ToggleFaceUp(true);
                canChangePos = false;
                UpdateCardRotation();
            }
            else Debug.Log("Can't change battle position this turn");
        }
    }

    void UpdateCardRotation()
    {
        if (isAttackPosition) transform.rotation = startingRotation;
        else transform.Rotate(0, 0, -90);
    }
}
