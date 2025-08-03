using UnityEngine;
using utility;

public class HealOnKillModification : BaseModification
{
    public override string Name => "Heal on Kill";
    public override string Description => "Восстанавливает хп при убийстве";
    public override Material Material => Resources.Load<Material>("Materials/DefenseLens");
    public override Rarity Rarity => Rarity.Rare;

    public override void ApplyOnKill(AliveState aliveState)
    {
        // TODO: Architectural violation - this should ideally use an event system or interface method
        // Currently needed because healing is player-specific functionality not available through AliveState
        var player = aliveState.Transform.GetComponent<Player>();
        if (player != null)
        {
            player.Heal(0.1f);
        }
    }
}
