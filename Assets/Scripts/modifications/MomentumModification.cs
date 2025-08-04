using UnityEngine;
using utility;

public class MomentumModification : BaseModification
{
    public override string Name => "Моментум";
    public override string Description => "Урон увеличивается в зависимости от скорости движения";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Legendary;

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        // Get rigidbody directly from the transform (works for any entity)
        var rigidbody = aliveState.Transform.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            float currentSpeed = rigidbody.linearVelocity.magnitude;

            // Convert speed to damage multiplier (capped at 3x)
            float speedMultiplier = 1f + Mathf.Min(currentSpeed * 0.3f, 2f);

            return baseValue * speedMultiplier;
        }

        return baseValue;
    }
}
