using UnityEngine;
using utility;

public class BerserkerModification : BaseModification
{
    public override string Name => "Берсерк";
    public override string Description => "Меньше хп - больше урона";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Rare;

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        float healthPercentage = aliveState.HealthPercentage;
        float missingHealthPercentage = 1f - healthPercentage;

        float damageMultiplier = 1f + (missingHealthPercentage * 2f);

        return baseValue * damageMultiplier;
    }
}
