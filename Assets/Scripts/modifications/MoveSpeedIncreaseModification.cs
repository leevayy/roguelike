using UnityEngine;
using utility;

public class MoveSpeedIncreaseModification : BaseModification
{
    public override string Name => "Увеличение скорости";
    public override string Description => "Увеличивает скорость передвижения";
    public override Material Material => Resources.Load<Material>("Materials/MovementLens");
    public override Rarity Rarity => Rarity.Uncommon;
    public override bool IsNotModifyingDamage => true;

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        return baseValue + 15f;
    }
}
