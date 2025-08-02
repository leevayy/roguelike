using UnityEngine;

public class MoveSpeedIncreaseModification : BaseModification
{
    public override string Name => "Move Speed Increase";
    public override string Description => "Increases movement speed";
    public override Material Material => Resources.Load<Material>("Materials/MoveSpeedIncreaseMaterial");

    public override float GetModifiedValue(float baseValue)
    {
        return baseValue + 5f;
    }
}
