using UnityEngine;
using utility;

public class ReflectDamageModification : BaseModification
{
    public override string Name => "Отражение урона";
    public override string Description => "Отражает часть полученного урона";
    public override Material Material => Resources.Load<Material>("Materials/ReflectDamageLens");

    public override void ApplyOnTakeDamage(utility.AliveState aliveState, float damage)
    {
        var weapon = aliveState.Transform.GetComponentInChildren<Weapon>();
        if (weapon != null)
        {
            weapon.Shoot(aliveState.Transform.rotation, damage * 5f, aliveState.ModManager.GetModifications());
        }
    }
}
