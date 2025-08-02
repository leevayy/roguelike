using UnityEngine;
using utility;

public class DoubleDamageAndTakenModification : BaseModification
{
    public override string Name => "Двойной урон";
    public override string Description => "Удваивает весь урон (наносимый и получаемый)";
    public override Material Material => Resources.Load<Material>("Materials/DoubleDamageLens");

    public override float GetModifiedValue(utility.AliveState aliveState, float baseValue)
    {
        return baseValue * 2f;
    }

    public override float ModifyIncomingDamage(utility.AliveState aliveState, float damage)
    {
        return base.ModifyIncomingDamage(aliveState, damage * 2f);
    }
}
