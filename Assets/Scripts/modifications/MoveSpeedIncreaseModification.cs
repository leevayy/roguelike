using UnityEngine;

public class MoveSpeedIncreaseModification : BaseModification
{
    public override string Name => "Увеличение скорости";
    public override string Description => "Увеличивает скорость передвижения";
    public override Material Material => Resources.Load<Material>("Materials/MoveSpeedLens");

    public override float GetModifiedValue(float baseValue)
    {
        return baseValue + 5f;
    }
}
