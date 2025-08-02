// BurnEffectModification.cs
using UnityEngine;

public class BurnEffectModification : BaseModification
{
    public override string Name => "Burn Effect";
    public override string Description => "Projectiles set enemies on fire";
    public override Material Material => Resources.Load<Material>("Materials/BurningMaterial");

    public override void ApplyOnShoot(Weapon weapon, float damage)
    {
        var laser = weapon.GetComponent<Laser>();
        if (laser != null)
        {
            laser.isBurn = true;
        }
    }
}
