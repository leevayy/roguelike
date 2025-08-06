using UnityEngine;
using utility;

public abstract class BaseModification : IModification
{
    public virtual string Name => "";
    public virtual string Description => "";
    public virtual Material Material => null;
    public virtual bool IsNotModifyingDamage => false;
    public virtual Rarity Rarity => Rarity.Common;

    public virtual void ApplyOnShoot(Weapon weapon, float damage) { }
    public virtual float ModifyIncomingDamage(AliveState aliveState, float damage) => damage;
    public virtual void ApplyOnUpdate(AliveState aliveState) { }
    public virtual void ApplyOnPickUp(AliveState aliveState) { }
    public virtual void ApplyOnDrop(AliveState aliveState) { }
    public virtual void ApplyOnKill(AliveState aliveState) { }
    public virtual float GetModifiedValue(AliveState aliveState, float baseValue) => baseValue;
    public virtual int GetProjectileCount(int baseCount) => baseCount;
    public virtual void ApplyOnTakeDamage(AliveState aliveState, float damage) { }
    public virtual void ApplyOnEnemySpawn(AliveState aliveState) { }
}
