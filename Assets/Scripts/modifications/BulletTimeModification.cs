using UnityEngine;
using utility;

public class BulletTimeModification : BaseModification
{
    public override string Name => "Время пуль";
    public override string Description => "Утраивает скорость стрельбы, но удваивает получаемый урон";
    public override Material Material => Resources.Load<Material>("Materials/DefenseLens");

    public override void ApplyOnUpdate(AliveState aliveState)
    {
    }

    public override float ModifyIncomingDamage(AliveState aliveState, float damage)
    {
        return damage * 2f;
    }
}
