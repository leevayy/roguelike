using UnityEngine;
using utility;

public class MultiplyMultiplyValueModification : BaseModification
{
    public override string Name => "Желтая линза";
    public override string Description => "Умножает количество снарядов в 2 раза";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Uncommon;

    public override int GetProjectileCount(int baseCount)
    {
        return baseCount * 2;
    }
}
