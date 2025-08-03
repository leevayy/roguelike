using UnityEngine;
using utility;

public class MoneyEqualsLifeModification : BaseModification
{
    public override string Name => "Деньги = жизнь";
    public override string Description => "Деньги вместо хп";
    public override Material Material => Resources.Load<Material>("Materials/DefenseLens");
    public override Rarity Rarity => Rarity.Legendary;

    public override float ModifyIncomingDamage(AliveState aliveState, float damage)
    {
        // Reduce money instead of health
        GameManager.instance.score -= (int)damage;

        // If money goes below 0, kill the player by dealing fatal damage
        if (GameManager.instance.score < 0)
        {
            GameManager.instance.score = 0; // Reset to 0 to avoid negative money
            return 999999f; // Return fatal damage to trigger death
        }

        return 0; // Prevent health damage if money is sufficient
    }
}
