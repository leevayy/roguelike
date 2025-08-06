// IModification.cs
using UnityEngine;

namespace utility
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public interface IModification
    {
        string Name { get; }
        string Description { get; }
        Material Material { get; }
        bool IsNotModifyingDamage { get; }
        Rarity Rarity { get; }

        void ApplyOnShoot(Weapon weapon, float damage);
        float ModifyIncomingDamage(AliveState aliveState, float damage);
        void ApplyOnUpdate(AliveState aliveState);
        void ApplyOnPickUp(AliveState aliveState);
        void ApplyOnDrop(AliveState aliveState);
        void ApplyOnKill(AliveState aliveState);
        float GetModifiedValue(AliveState aliveState, float baseValue);
        int GetProjectileCount(int baseCount);
        void ApplyOnTakeDamage(AliveState aliveState, float damage);
    }
}

