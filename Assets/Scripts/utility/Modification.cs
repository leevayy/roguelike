using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public enum ModificationType
{
    AddFlatValue,
    AddMultiplyValue,
    MultiplyMultiplyValue,
}

public class Modification
{
    public ModificationType type { get; private set; }
    public float value { get; private set; }

    public Modification()
    {
        var values = Enum.GetValues(typeof(ModificationType));
        
        type = (ModificationType)values.GetValue(Random.Range(0, values.Length));
        value = type switch
        {
            ModificationType.AddFlatValue => 15f,
            ModificationType.AddMultiplyValue => 2f,
            ModificationType.MultiplyMultiplyValue => 2f,
            _ => 1f,
        }; 
    }
}

