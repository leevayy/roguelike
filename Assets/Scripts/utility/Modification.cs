using System;
using Random = UnityEngine.Random;

namespace utility
{
    public enum ModificationType
    {
        AddFlatValue,
        AddMultiplyValue,
        MultiplyMultiplyValue,
        RubberDuck,
        MoneyEqualsLife,
        MoveSpeedIncrease,
        HealOnKill,
        ReflectDamage,
        DoubleDamageAndTaken,
        InvulnerabilityOnHit,
        BurnEffect,
        GhostLaser,
        // not implemented
        // ---------------
        // GlassLens,
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
                ModificationType.DoubleDamageAndTaken => 2f,
                // ModificationType.GlassLens => 10f,
                _ => 1f,
            }; 
        }
    }
}