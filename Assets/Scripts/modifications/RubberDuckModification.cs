using UnityEngine;
using utility;

public class RubberDuckModification : BaseModification
{
    public override string Name => "Утка-уборщик";
    public override string Description => "Призывает утку-уборщика";
    public override Material Material => Resources.Load<Material>("Materials/OtherLens");
    public override Rarity Rarity => Rarity.Common;

    // This modification might need special handling during game initialization
    // For now, we'll keep it simple and implement the basic interface
    public override void ApplyOnUpdate(AliveState aliveState)
    {
        // RubberDuck logic would go here - possibly spawning a companion
        // This might need to be handled differently, possibly in GameManager
    }
}