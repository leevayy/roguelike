using UnityEngine;
using utility;

public class ReflectDamageModification : BaseModification
{
    public override string Name => "Отражение урона";
    public override string Description => "Перенаправляет полученный урон";
    public override Material Material => Resources.Load<Material>("Materials/DefenseLens");
    public override Rarity Rarity => Rarity.Common;

    public override void ApplyOnTakeDamage(AliveState aliveState, float damage)
    {
        var weapon = aliveState.Transform.GetComponentInChildren<Weapon>();
        if (weapon != null)
        {
            weapon.Shoot(aliveState.Transform.rotation, damage * 5f, aliveState.ModManager.GetModifications());
        }
    }
}
