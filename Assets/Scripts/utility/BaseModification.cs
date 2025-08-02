// BaseModification.cs
using UnityEngine;

public abstract class BaseModification : IModification
{
    public virtual string Name => "";
    public virtual string Description => "";
    public virtual UnityEngine.Material Material => null;

    public virtual void ApplyOnShoot(Weapon weapon, float damage) { }
    public virtual float ModifyIncomingDamage(Player player, float damage) => damage;
    public virtual void ApplyOnUpdate(Player player) { }
    public virtual void ApplyOnKill(Player player) { }
    public virtual float GetModifiedValue(float baseValue) => baseValue;
    public virtual int GetProjectileCount(int baseCount) => baseCount;
    public virtual void ApplyOnTakeDamage(Player player, float damage) { }
}
