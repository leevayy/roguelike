using UnityEngine;
using utility;

public class RampageModification : BaseModification
{
    private float _killStreak = 0f;
    private const float DAMAGE_MULTIPLIER_PER_KILL = 0.50f;

    public override string Name => "Killstreak";
    public override string Description => $"Каждое убийство увеличивает урон на {DAMAGE_MULTIPLIER_PER_KILL * 100}%. Сбрасывается при получении урона";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Epic;

    public override void ApplyOnKill(AliveState aliveState)
    {
        _killStreak++;
    }

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        return baseValue * (1f + (_killStreak * DAMAGE_MULTIPLIER_PER_KILL));
    }

    public override float ModifyIncomingDamage(AliveState aliveState, float damage)
    {
        if (_killStreak > 0f)
        {
            _killStreak = 0f;
        }
        return damage;
    }
}
