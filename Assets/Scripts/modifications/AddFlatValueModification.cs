using UnityEngine;
using utility;

public class AddFlatValueModification : BaseModification
{
    public override string Name => "Синяя линза";
    public override string Description => "Добавляет 15 базового урона";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Common;

    public override float GetModifiedValue(AliveState aliveState, float baseValue) => baseValue + 15f;
}