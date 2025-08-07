using UnityEngine;
using utility;

public class HitscanModification : BaseModification
{
    public override string Name => "Хитскан";
    public override string Description => "Превращает оружие в хитскан";
    public override Material Material => Resources.Load<Material>("Materials/OtherLens");
    public override Rarity Rarity => Rarity.Rare;

    public override void ApplyOnPickUp(AliveState aliveState)
    {
        // hack
        var player = aliveState.Transform.GetComponent<Player>();
        if (!player) return;
        player.ChangeWeaponType();
    }
    public override void ApplyOnDrop(AliveState aliveState)
    {
        // hack
        var player = aliveState.Transform.GetComponent<Player>();
        if (!player) return;
        player.ChangeWeaponType();
    }
}
