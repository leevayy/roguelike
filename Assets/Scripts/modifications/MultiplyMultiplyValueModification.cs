using UnityEngine;

public class MultiplyMultiplyValueModification : BaseModification
{
    public override string Name => "Multiply Projectiles";
    public override string Description => "Multiplies projectile count by x2";
    public override Material Material => Resources.Load<Material>("Materials/MultiplyMultiplyValueMaterial");

    public override int GetProjectileCount(int baseCount)
    {
        return baseCount * 2;
    }
}
