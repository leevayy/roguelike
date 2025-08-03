using UnityEngine;
using utility;

public class DoubleDamageAndTakenModification : BaseModification
{
    public override string Name => "Двойной урон";
    public override string Description => "Удваивает весь урон (наносимый и получаемый)";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Common;

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        return baseValue * 2f;
    }

    public override float ModifyIncomingDamage(AliveState aliveState, float damage)
    {
        return base.ModifyIncomingDamage(aliveState, damage * 2f);
    }
}
