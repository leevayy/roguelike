using UnityEngine;
using utility;

public class GlassCannonModification : BaseModification
{
    public override string Name => "Стеклянная линза";
    // maybe change to brake the lens instead of player? 
    public override string Description => "Увеличивает урон в 10 раз, но любой урон убивает мгновенно";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Rare;

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        return baseValue * 10f;
    }

    public override float ModifyIncomingDamage(AliveState aliveState, float damage)
    {
        if (damage > 0)
        {
            return 999999f;
        }
        return damage;
    }
}
