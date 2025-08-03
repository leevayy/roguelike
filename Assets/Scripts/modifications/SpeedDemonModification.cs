using UnityEngine;
using utility;

public class SpeedDemonModification : BaseModification
{
    private float _speedStacks = 0f;
    private const float SPEED_PER_KILL = 0.5f;

    public override string Name => "Молния МакКуин";
    public override string Description => $"Каждое убийство навсегда увеличивает скорость";
    public override Material Material => Resources.Load<Material>("Materials/MovementLens");
    public override bool IsNotModifyingDamage => true;
    public override Rarity Rarity => Rarity.Legendary;

    public override void ApplyOnKill(AliveState aliveState)
    {
        _speedStacks += SPEED_PER_KILL;
    }

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        return baseValue + _speedStacks;
    }
}
