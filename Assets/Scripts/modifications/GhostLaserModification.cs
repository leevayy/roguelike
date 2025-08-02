using UnityEngine;

public class GhostLaserModification : BaseModification
{
    public override string Name => "Ghost Laser";
    public override string Description => "Projectiles pass through enemies";
    public override Material Material => Resources.Load<Material>("Materials/GhostLaserMaterial");

    public override void ApplyOnShoot(Weapon weapon, float damage)
    {
        var laser = weapon.GetComponent<Laser>();
        if (laser != null)
        {
            laser.isSolid = true;
        }
    }
}
