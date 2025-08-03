using UnityEngine;
using utility;

public class MultishotMadnessModification : BaseModification
{
    public override string Name => "Tripleshot";
    public override string Description => "Утраивает количество снарядов за выстрел";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Epic;

    public override int GetProjectileCount(int baseCount)
    {
        return baseCount * 3;
    }
}
