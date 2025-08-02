using UnityEngine;

public class MultiplyMultiplyValueModification : BaseModification
{
    public override string Name => "Умножение снарядов";
    public override string Description => "Умножает количество снарядов в 2 раза";
    public override Material Material => Resources.Load<Material>("Materials/MultiplyMultiplyValueLens");

    public override int GetProjectileCount(int baseCount)
    {
        return baseCount * 2;
    }
}
