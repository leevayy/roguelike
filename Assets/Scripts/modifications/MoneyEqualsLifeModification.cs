using UnityEngine;

public class MoneyEqualsLifeModification : BaseModification
{
    public override string Name => "Money Equals Life";
    public override string Description => "Uses money instead of HP";
    public override Material Material => Resources.Load<Material>("Materials/MoneyEqualsLifeMaterial");

    public override float ModifyIncomingDamage(Player player, float damage)
    {
        GameManager.instance.score -= (int)damage;
        return 0; // Prevent health damage
    }
}
