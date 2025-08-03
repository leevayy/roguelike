using UnityEngine;
using utility;

public class BurnEffectModification : BaseModification
{
    public override string Name => "Поджог";
    public override string Description => "Снаряды поджигают врагов";
    public override Material Material => Resources.Load<Material>("Materials/OtherLens");
    public override Rarity Rarity => Rarity.Common;

    public override void ApplyOnShoot(Weapon weapon, float damage)
    {
        weapon.SetBurnEffect(true);
    }
}
