using UnityEngine;
using utility;

/// <summary>
/// Example modification that demonstrates how to use AliveState to work with both players and enemies
/// This modification increases speed when health is low
/// </summary>
public class DesperationModification : BaseModification
{
    public override string Name => "Отчаяние";
    public override string Description => "Увеличивает скорость когда здоровье низкое";
    public override Material Material => Resources.Load<Material>("Materials/MoveSpeedLens");

    public override void ApplyOnUpdate(utility.AliveState aliveState)
    {
        // This modification works for both players and enemies using AliveState
        if (aliveState.IsLowHealth && aliveState.IsAlive)
        {
            // Increase movement speed based on how low health is
            var speedBoost = (1.0f - aliveState.HealthPercentage) * 10f;
            
            // For players, use the MovementManager
            var player = aliveState.Transform.GetComponent<Player>();
            if (player != null)
            {
                player.SetAdditionalSpeed(speedBoost);
                return;
            }
            
            // For enemies, we could apply speed boost differently
            var enemy = aliveState.Transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Enemy speed boost would be applied here
                // This demonstrates how we can handle different entity types
                Debug.Log($"Enemy {aliveState.Transform.name} gains speed boost: {speedBoost}");
            }
        }
    }

    public override float ModifyIncomingDamage(utility.AliveState aliveState, float damage)
    {
        // Reduce damage when health is very low (last stand effect)
        if (aliveState.HealthPercentage < 0.1f)
        {
            return damage * 0.5f; // 50% damage reduction
        }
        
        return damage;
    }
}
