using UnityEngine;

public class DoubleDamageAndTakenModification : BaseModification
{
    public override string Name => "Double Damage & Taken";
    public override string Description => "Doubles all damage (dealt and taken)";
    public override Material Material => Resources.Load<Material>("Materials/DoubleDamageAndTakenMaterial");

    public override float GetModifiedValue(float baseValue)
    {
        return baseValue * 2f;
    }
}
