using UnityEngine;
using utility;

public class AddMultiplyValueModification : BaseModification
{
    public override string Name => "Красная линза";
    public override string Description => "Умножает урон в 2 раза";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Uncommon;

    public override float GetModifiedValue(AliveState aliveState, float baseValue) => baseValue * 2f;
}