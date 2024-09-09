public enum SpellType { None, Normal, Equip, Continuous, Quick, Field };
public enum TrapType { None, Normal, Continuous, Counter };
public class SpellTrap : Card
{
    public bool usesTarget;
    public Card target;

    public SpellType spellType;
    public TrapType trapType;

    public MonsterType requiredType;
    public int[] boostValue = new int[2]; //index 0 = attack, index 1 = defence

    public void TriggerEffects(bool activate) //true to apply effects, false to remove
    {
        int multiplier = 1;
        if (!activate) multiplier = -1; //if activate is false, then it will apply the opposite boost

        Monster targetMonster = target as Monster;

        targetMonster.atk += boostValue[0] * multiplier;
        targetMonster.def += boostValue[1] * multiplier;
    }
}
