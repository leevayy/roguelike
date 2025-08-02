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

    public void ApplyOnUpdate(Player player)
    {
        foreach (var mod in _modifications)
        {
            mod.Strategy.ApplyOnUpdate(player);
        }
    }

    public void ApplyOnKill(Player player)
    {
        foreach (var mod in _modifications)
        {
            mod.Strategy.ApplyOnKill(player);
        }
    }

    public void ApplyOnShoot(Weapon weapon, float damage)
    {
        foreach (var mod in _modifications)
        {
            mod.Strategy.ApplyOnShoot(weapon, damage);
        }
    }

    public float ModifyIncomingDamage(Player player, float damage)
    {
        var modifiedDamage = damage;
        foreach (var mod in _modifications)
        {
            modifiedDamage = mod.Strategy.ModifyIncomingDamage(player, modifiedDamage);
        }
        return modifiedDamage;
    }

    public float GetModifiedValue(float baseValue, ModificationType type)
    {
        var modifiedValue = baseValue;
        var mods = _modifications.Where(mod => mod.Type == type);
        foreach (var mod in mods)
        {
            modifiedValue = mod.Strategy.GetModifiedValue(modifiedValue);
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
}
