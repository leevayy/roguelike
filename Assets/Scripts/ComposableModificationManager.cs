using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using utility;

public class ComposableModificationManager : MonoBehaviour
{
    private readonly List<Modification> _modifications = new();

    public void AddModification(Modification modification)
    {
        _modifications.Add(modification);
    }

    public void RemoveAllModifiersOfType(ModificationType type)
    {
        _modifications.RemoveAll(mod => mod.Type == type);
    }

    public ReadOnlyCollection<Modification> GetModifications()
    {
        return _modifications.AsReadOnly();
    }

    public void Clear()
    {
        _modifications.Clear();
    }

    public bool HasMod(ModificationType modType)
    {
        return _modifications.Exists(mod => mod.Type == modType);
    }

    public int CountMod(ModificationType modType)
    {
        return _modifications.Count(mod => mod.Type == modType);
    }

    // --- New Hook-based Methods ---

    public void ApplyOnUpdate(utility.AliveState aliveState)
    {
        foreach (var mod in _modifications)
        {
            mod.Strategy.ApplyOnUpdate(aliveState);
        }
    }

    public void ApplyOnKill(utility.AliveState aliveState)
    {
        foreach (var mod in _modifications)
        {
            mod.Strategy.ApplyOnKill(aliveState);
        }
    }

    public void ApplyOnShoot(Weapon weapon, float damage)
    {
        foreach (var mod in _modifications)
        {
            mod.Strategy.ApplyOnShoot(weapon, damage);
        }
    }

    public float ModifyIncomingDamage(utility.AliveState aliveState, float damage)
    {
        var modifiedDamage = damage;
        foreach (var mod in _modifications)
        {
            modifiedDamage = mod.Strategy.ModifyIncomingDamage(aliveState, modifiedDamage);
        }
        return modifiedDamage;
    }

    public float GetModifiedValue(utility.AliveState aliveState, float baseValue, ModificationType type)
    {
        var modifiedValue = baseValue;
        var mods = _modifications.Where(mod => mod.Type == type);
        foreach (var mod in mods)
        {
            modifiedValue = mod.Strategy.GetModifiedValue(aliveState, modifiedValue);
        }
        return modifiedValue;
    }

    public int GetProjectileCount(int baseCount)
    {
        var modifiedCount = baseCount;
        foreach (var mod in _modifications)
        {
            modifiedCount = mod.Strategy.GetProjectileCount(modifiedCount);
        }
        return modifiedCount;
    }

    public void ApplyOnTakeDamage(utility.AliveState aliveState, float damage)
    {
        foreach (var mod in _modifications)
        {
            mod.Strategy.ApplyOnTakeDamage(aliveState, damage);
        }
    }

    // Legacy methods for backward compatibility with Player objects
    public void ApplyOnUpdate(Player player)
    {
        ApplyOnUpdate(player.GetAliveState());
    }

    public void ApplyOnKill(Player player)
    {
        ApplyOnKill(player.GetAliveState());
    }

    public float ModifyIncomingDamage(Player player, float damage)
    {
        return ModifyIncomingDamage(player.GetAliveState(), damage);
    }

    public void ApplyOnTakeDamage(Player player, float damage)
    {
        ApplyOnTakeDamage(player.GetAliveState(), damage);
    }
}
