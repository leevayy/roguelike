using UnityEngine;

public class MoneyEqualsLifeModification : BaseModification
{
    public override string Name => "Деньги = жизнь";
    public override string Description => "Здоровье равно количеству денег";
    public override Material Material => Resources.Load<Material>("Materials/MoneyEqualsLifeLens");

    public override float ModifyIncomingDamage(Player player, float damage)
    {
        GameManager.instance.score -= (int)damage;
        return 0; // Prevent health damage
    }
}
