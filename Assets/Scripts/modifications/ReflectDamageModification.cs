using UnityEngine;

public class ReflectDamageModification : BaseModification
{
    public override string Name => "Отражение урона";
    public override string Description => "Отражает часть полученного урона";
    public override Material Material => Resources.Load<Material>("Materials/ReflectDamageLens");

    public override void ApplyOnTakeDamage(Player player, float damage)
    {
        var weapon = player.GetComponentInChildren<Weapon>();
        if (weapon != null)
        {
            weapon.Shoot(player.transform.rotation, damage * 5f, player.GetModifications());
        }
    }
}
