using UnityEngine;

public class ReflectDamageModification : BaseModification
{
    public override string Name => "Reflect Damage";
    public override string Description => "Redirects 500% of incoming damage";
    public override Material Material => Resources.Load<Material>("Materials/ReflectDamageMaterial");

    public override void ApplyOnTakeDamage(Player player, float damage)
    {
        var weapon = player.GetComponentInChildren<Weapon>();
        if (weapon != null)
        {
            weapon.Shoot(player.transform.rotation, damage * 5f, player.GetModifications());
        }
    }
}
