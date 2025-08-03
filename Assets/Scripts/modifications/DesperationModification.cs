using UnityEngine;
using utility;

/// <summary>
/// Example modification that demonstrates how to use AliveState to work with both players and enemies
/// This modification increases speed when health is low
/// </summary>
public class DesperationModification : BaseModification
{
    public override string Name => "Отчаяние";
    public override string Description => "Увеличивает скорость когда здоровье низкое + дает второй шанс";
    public override Material Material => Resources.Load<Material>("Materials/MovementLens");
    public override bool IsNotModifyingDamage => false;
    public override Rarity Rarity => Rarity.Rare;

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        // Only modify speed values for low health entities
        if (aliveState.IsLowHealth && aliveState.IsAlive)
        {
            // Increase movement speed based on how low health is
            var speedBoost = (1.0f - aliveState.HealthPercentage) * 10f;
            return baseValue + speedBoost;
        }

        return baseValue;
    }

    public override float ModifyIncomingDamage(AliveState aliveState, float damage)
    {
        // Reduce damage when health is very low (last stand effect)
        if (aliveState.IsLowHealth)
        {
            return damage * 0.5f; // 50% damage reduction
        }
        
        return damage;
    }
}
