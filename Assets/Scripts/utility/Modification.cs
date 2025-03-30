using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public enum ModificationType
{
    AddFlatValue,
    AddMultiplyValue,
    MultiplyMultiplyValue,
    RubberDuck,
    MoneyEqualsLife,
}

public class Modification
{
    public ModificationType type { get; private set; }
    public float value { get; private set; }

    public Modification() : this(GetRandomModificationType())
    {
    }
    
    private static ModificationType GetRandomModificationType()
    {
        var values = Enum.GetValues(typeof(ModificationType));
        return (ModificationType)values.GetValue(Random.Range(0, values.Length));
    }
    
    public Modification(ModificationType modType)
    {
        type = modType;
        value = type switch
        {
            ModificationType.AddFlatValue => 15f,
            ModificationType.AddMultiplyValue => 2f,
            ModificationType.MultiplyMultiplyValue => 2f,
            _ => 1f,
        }; 
    }
}

