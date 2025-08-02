using System;
using UnityEngine;
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
    }

    public class Modification
    {
        public IModification Strategy { get; }
        public ModificationType Type { get; }
        public float Value { get; }

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
            Type = modType;
            Strategy = CreateStrategy(modType);
            Value = GetValue(modType);
        }

        private static float GetValue(ModificationType modType)
        {
            return modType switch
            {
                ModificationType.AddFlatValue => 15f,
                ModificationType.AddMultiplyValue => 2f,
                ModificationType.MultiplyMultiplyValue => 2f,
                ModificationType.DoubleDamageAndTaken => 2f,
                _ => 1f,
            };
        }

        private static IModification CreateStrategy(ModificationType modType)
        {
            return modType switch
            {
                ModificationType.AddFlatValue => new AddFlatValueModification(),
                ModificationType.AddMultiplyValue => new AddMultiplyValueModification(),
                ModificationType.BurnEffect => new BurnEffectModification(),
                ModificationType.DoubleDamageAndTaken => new DoubleDamageAndTakenModification(),
                ModificationType.GhostLaser => new GhostLaserModification(),
                ModificationType.HealOnKill => new HealOnKillModification(),
                ModificationType.InvulnerabilityOnHit => new InvulnerabilityOnHitModification(),
                ModificationType.MoneyEqualsLife => new MoneyEqualsLifeModification(),
                ModificationType.MoveSpeedIncrease => new MoveSpeedIncreaseModification(),
                ModificationType.MultiplyMultiplyValue => new MultiplyMultiplyValueModification(),
                ModificationType.ReflectDamage => new ReflectDamageModification(),
                // ModificationType.RubberDuck => new RubberDuckModification(), // Not created yet
                _ => new BaseModification(), // Default case
            };
        }
    }

    // We still need to create these two missing concrete modification classes
    public class AddFlatValueModification : BaseModification
    {
        public override float GetModifiedValue(float baseValue) => baseValue + 15f;
    }

    public class AddMultiplyValueModification : BaseModification
    {
        public override float GetModifiedValue(float baseValue) => baseValue * 2f;
    }
}
