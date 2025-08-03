using UnityEngine;
using utility;

public class MomentumModification : BaseModification
{
    public override string Name => "Моментум";
    public override string Description => "Урон увеличивается в зависимости от скорости движения";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Rare;

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

    public override void ApplyOnUpdate(AliveState aliveState)
    {
        // Get rigidbody directly from the transform (works for any entity)
        var rigidbody = aliveState.Transform.GetComponent<Rigidbody>();
        if (rigidbody != null && Time.time % 1f < Time.deltaTime)
        {
            float speed = rigidbody.linearVelocity.magnitude;
            float bonus = Mathf.Min(speed * 30f, 200f);
            if (bonus > 10f)
            {
                Debug.Log($"Momentum: {bonus:F0}% damage bonus from movement speed!");
            }
        }
    }
}
