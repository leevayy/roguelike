using UnityEngine;

public class HealOnKillModification : BaseModification
{
    public override string Name => "Heal On Kill";
    public override string Description => "Each kill restores HP";
    public override Material Material => Resources.Load<Material>("Materials/HealOnKillMaterial");

    public override void ApplyOnKill(Player player)
    {
        player.Heal(0.1f);
    }
}
