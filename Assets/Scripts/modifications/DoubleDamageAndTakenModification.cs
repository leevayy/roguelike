using UnityEngine;

public class DoubleDamageAndTakenModification : BaseModification
{
    public override string Name => "Двойной урон";
    public override string Description => "Удваивает весь урон (наносимый и получаемый)";
    public override Material Material => Resources.Load<Material>("Materials/DoubleDamageLens");

    public override float GetModifiedValue(float baseValue)
    {
        return baseValue * 2f;
    }
}
