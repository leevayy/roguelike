// IModification.cs
using UnityEngine;

namespace utility
{
    public interface IModification
    {
        string Name { get; }
        string Description { get; }
        Material Material { get; }

        void ApplyOnShoot(Weapon weapon, float damage);
        float ModifyIncomingDamage(AliveState aliveState, float damage);
        void ApplyOnUpdate(AliveState aliveState);
        void ApplyOnKill(AliveState aliveState);
        float GetModifiedValue(AliveState aliveState, float baseValue);
        int GetProjectileCount(int baseCount);
        void ApplyOnTakeDamage(AliveState aliveState, float damage);
    }
}

