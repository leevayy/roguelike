// IModification.cs
public interface IModification
{
    string Name { get; }
    string Description { get; }
    UnityEngine.Material Material { get; }

    void ApplyOnShoot(Weapon weapon, float damage);
    float ModifyIncomingDamage(Player player, float damage);
    void ApplyOnUpdate(Player player);
    void ApplyOnKill(Player player);
    float GetModifiedValue(float baseValue);
    int GetProjectileCount(int baseCount);
    void ApplyOnTakeDamage(Player player, float damage);
}

