using UnityEngine;
using utility;

public class HealOnKillModification : BaseModification
{
    public override string Name => "Лечение за убийство";
    public override string Description => "Восстанавливает здоровье при убийстве врага";
    public override Material Material => Resources.Load<Material>("Materials/HealOnKillLens");

    public override void ApplyOnKill(utility.AliveState aliveState)
    {
        // This modification only works for players - check if we can cast to player
        var player = aliveState.Transform.GetComponent<Player>();
        if (player != null)
        {
            player.Heal(0.1f);
        }
    }
}
