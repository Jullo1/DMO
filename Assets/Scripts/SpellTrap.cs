public enum SpellType { None, Normal, Equip, Continuous, Quick, Field };
public enum TrapType { None, Normal, Continuous, Counter };
public enum Target { None, Monster, Spell, Trap, SpellTrap, All };
public enum EffectType { None, Equip, ChangePosition, LP , MoveCard, Restrict, Draw }
public class SpellTrap : Card
{
    public bool usesTarget;
    public Card target;

    public SpellType spellType;
    public TrapType trapType;

    public MonsterType requiredType;
    public Target targetType;
    public Zone targetZone;
    public EffectType effectType;

    public int[] boostValue = new int[2]; //index 0 = attack/player, index 1 = defence/enemy

    public void TriggerEffects(bool activate = true) //true to apply effects, false to remove
    {
        switch(effectType)
        {
            case EffectType.Equip:
                int multiplier = 1;
                if (!activate) multiplier = -1; //if activate is false, then it will apply the opposite boost

                Monster targetMonster = target as Monster;

                targetMonster.atk += boostValue[0] * multiplier;
                targetMonster.def += boostValue[1] * multiplier;
                break;

            case EffectType.LP:
                if (boostValue[0] != 0) engine.ChangeLP(ownedByPlayer, boostValue[0]);
                if (boostValue[1] != 0) engine.ChangeLP(!ownedByPlayer, boostValue[1]);
                engine.PlaySound("magic");
                engine.MoveCard(GetComponent<Card>(), Zone.Graveyard, false);
                break;

            case EffectType.MoveCard:
                if (targetZone == Zone.Deck)
                {
                    engine.Draw(ownedByPlayer, boostValue[0]);
                    engine.MoveCard(GetComponent<Card>(), Zone.Graveyard, false);
                    break;
                }
                if (cardName == "Dark Hole")
                {
                    foreach (Slot slot in FindObjectsOfType<Slot>())
                        if (slot.container)
                            if (slot.container.GetType() == typeof(Monster))
                                engine.MoveCard(slot.container, Zone.Graveyard, false, true, true);
                }
                else if (cardName == "Raigeki")
                {
                    foreach (Slot slot in FindObjectsOfType<Slot>())
                        if (slot.container)
                            if (slot.container.GetType() == typeof(Monster) && slot.container.ownedByPlayer != ownedByPlayer)
                                engine.MoveCard(slot.container, Zone.Graveyard, false, true, true);
                }
                else if (cardName == "Fissure")
                {
                    Monster target = null;
                    int maxAtk = int.MaxValue;
                    foreach (Slot slot in FindObjectsOfType<Slot>())
                    {
                        if (slot.container)
                            if (slot.container.GetType() == typeof(Monster) && slot.container.ownedByPlayer != ownedByPlayer)
                            {
                                Monster monster = slot.container as Monster;
                                if (monster.atk < maxAtk)
                                {
                                    target = monster;
                                    maxAtk = monster.atk;
                                }
                            }
                    }
                    if (target != null) engine.MoveCard(target, Zone.Graveyard, false, true, true);
                }
                engine.PlaySound("magic2");
                engine.MoveCard(GetComponent<Card>(), Zone.Graveyard, false);
                break;
        }
    }
}
