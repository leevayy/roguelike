// BurnEffectModification.cs
using UnityEngine;

public class BurnEffectModification : BaseModification
{
    public override string Name => "Поджог";
    public override string Description => "Снаряды поджигают врагов";
    public override Material Material => Resources.Load<Material>("Materials/BurningLens");

    public override void ApplyOnShoot(Weapon weapon, float damage)
    {
        var laser = weapon.GetComponent<Laser>();
        if (laser != null)
        {
            laser.isBurn = true;
        }
    }
}
