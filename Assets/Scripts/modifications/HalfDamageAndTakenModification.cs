using UnityEngine;
using utility;

public class HalfDamageAndTakenModification : BaseModification
{
    public override string Name => "Половинчатый урон";
    public override string Description => "Уполовинивает весь урон (получаемый и наносимый)";
    public override Material Material => Resources.Load<Material>("Materials/DefenseLens");
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
