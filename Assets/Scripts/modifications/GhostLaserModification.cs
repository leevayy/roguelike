using UnityEngine;
using utility;

public class GhostLaserModification : BaseModification
{
    public override string Name => "Призрачный лазер";
    public override string Description => "Снаряды проходят сквозь врагов";
    public override Material Material => Resources.Load<Material>("Materials/OtherLens");
    public override Rarity Rarity => Rarity.Rare;

    public override void ApplyOnShoot(Weapon weapon, float damage)
    {
        weapon.SetGhostEffect(true);
    }
}
