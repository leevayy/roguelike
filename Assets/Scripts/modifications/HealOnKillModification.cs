using UnityEngine;

public class HealOnKillModification : BaseModification
{
    public override string Name => "Лечение за убийство";
    public override string Description => "Восстанавливает здоровье при убийстве врага";
    public override Material Material => Resources.Load<Material>("Materials/HealOnKillLens");

    public override void ApplyOnKill(Player player)
    {
        player.Heal(0.1f);
    }
}
