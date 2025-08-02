using UnityEngine;
using utility;

public abstract class BaseModification : IModification
{
    public virtual string Name => "";
    public virtual string Description => "";
    public virtual UnityEngine.Material Material => null;

    public virtual void ApplyOnShoot(Weapon weapon, float damage) { }
    public virtual float ModifyIncomingDamage(AliveState aliveState, float damage) => damage;
    public virtual void ApplyOnUpdate(AliveState aliveState) { }
    public virtual void ApplyOnKill(AliveState aliveState) { }
    public virtual float GetModifiedValue(AliveState aliveState, float baseValue) => baseValue;
    public virtual int GetProjectileCount(int baseCount) => baseCount;
    public virtual void ApplyOnTakeDamage(AliveState aliveState, float damage) { }

    // Legacy methods for backward compatibility with Player objects
    public virtual float ModifyIncomingDamage(Player player, float damage) => ModifyIncomingDamage(player.GetAliveState(), damage);
    public virtual void ApplyOnUpdate(Player player) => ApplyOnUpdate(player.GetAliveState());
    public virtual void ApplyOnKill(Player player) => ApplyOnKill(player.GetAliveState());
    public virtual void ApplyOnTakeDamage(Player player, float damage) => ApplyOnTakeDamage(player.GetAliveState(), damage);
}
