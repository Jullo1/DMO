using System.Collections.Generic;
using UnityEngine;

public enum MonsterType { Warrior, Fiend, Dragon, Zombie, Spellcaster, Insect, Dinosaur, Beast, BeastWarrior, Pyro, Plant, WingedBeast, Aqua, Fairy, Rock, Fish, Thunder, Reptile, Machine, SeaSerpent, None };

public class Monster : Card
{
    public int atk;
    public int def;
    public int level;

    public bool isAttackPosition;
    public bool canChangePos;
    public bool hasBattled;
    public MonsterType type;
    string printType;

    public List<Card> equips = new List<Card>();

    void Start()
    {
        FixPrints();

        fullCardText += "\n\nATK/" + atk.ToString() + "  DEF/" + def.ToString() + "\n\nType: " + printType;

        if (!isFaceUp) //fix starting rotation if it's a monster that was set
        {
            transform.Rotate(0, 0, 90);
            transform.Rotate(0, 0, -90);
        }
    }

    public void TogglePosition(bool isAttack, bool isForced = false)
    {
        if ((canChangePos && (!hasBattled)) || isForced) //if is changed by engine
        {
            isAttackPosition = isAttack;
            if (isAttack && !isFaceUp) { ToggleFaceUp(true); if (!isForced) FindObjectOfType<DuelEngine>().PlaySound("play"); }
            else if (!isForced) FindObjectOfType<DuelEngine>().PlaySound("send");
            canChangePos = false;
            UpdateCardRotation();
        }
        else if (ownedByPlayer) { FindObjectOfType<DuelEngine>().AlertText("Can't change battle position this turn", true); FindObjectOfType<DuelEngine>().PlaySound("cant"); }
    }

    void UpdateCardRotation()
    {
        if (isAttackPosition) transform.localRotation = Quaternion.identity;
        else transform.Rotate(0, 0, -90);
    }

    void FixPrints()
    {
        switch (type)
        {
            default:
                printType = type.ToString();
                break;
            case MonsterType.BeastWarrior:
                printType = "Beast-Warrior";
                break;
            case MonsterType.WingedBeast:
                printType = "Winged Beast";
                break;
            case MonsterType.SeaSerpent:
                printType = "Sea-Serpent";
                break;
        }
    }
}
