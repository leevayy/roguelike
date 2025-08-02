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

    public void RemoveModification(Modification modification)
    {
        _modifications.Remove(modification);
    }

    public void RemoveAllModifiersOfType(ModificationType type)
    {
        _modifications.RemoveAll(mod => mod.type == type);
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
        return _modifications.Exists(mod => mod.type == modType);
    }

    public int CountMod(ModificationType modType)
    {
        return _modifications.Count(mod => mod.type == modType);
    }

    public float GetModifiedValue(float baseValue, ModificationType type)
    {
        var mods = _modifications.Where(mod => mod.type == type).ToList();
        if (mods.Count == 0)
        {
            return baseValue;
        }

        var result = baseValue;
        foreach (var mod in mods)
        {
            result = mod.type switch
            {
                ModificationType.AddFlatValue => result + mod.value,
                ModificationType.AddMultiplyValue => result * mod.value,
                ModificationType.MultiplyMultiplyValue => result * mod.value,
                _ => result
            };
        }

        return result;
    }
}
