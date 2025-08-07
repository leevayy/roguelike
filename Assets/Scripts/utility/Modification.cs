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
        // turns out too OP + too much pain in the ass when you try to think about other mods interactions
        // MoneyEqualsLife,
        MoveSpeedIncrease,
        HealOnKill,
        ReflectDamage,
        DoubleDamageAndTaken,
        InvulnerabilityOnHit,
        BurnEffect,
        GhostLaser,
        OneshotInTheAir,
        Desperation,
        // not implemented
        // BulletTime,
        GlassCannon,
        MultishotMadness,
        Berserker,
        SpeedDemon,
        // buggy 
        // ExplosiveFinish,
        Rampage,
        Momentum,
        HalfDamageAndTaken,
        PredictionModification,
        HitscanModification
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
                // ModificationType.MoneyEqualsLife => new MoneyEqualsLifeModification(),
                ModificationType.MoveSpeedIncrease => new MoveSpeedIncreaseModification(),
                ModificationType.MultiplyMultiplyValue => new MultiplyMultiplyValueModification(),
                ModificationType.ReflectDamage => new ReflectDamageModification(),
                ModificationType.RubberDuck => new RubberDuckModification(),
                ModificationType.OneshotInTheAir => new OneshotInTheAirModification(),
                ModificationType.Desperation => new DesperationModification(),
                ModificationType.SpeedDemon => new SpeedDemonModification(),
                ModificationType.Rampage => new RampageModification(),
                // ModificationType.BulletTime => new BulletTimeModification(),
                ModificationType.GlassCannon => new GlassCannonModification(),
                ModificationType.MultishotMadness => new MultishotMadnessModification(),
                ModificationType.Berserker => new BerserkerModification(),
                // ModificationType.ExplosiveFinish => new ExplosiveFinishModification(),
                ModificationType.Momentum => new MomentumModification(),
                ModificationType.HalfDamageAndTaken => new HalfDamageAndTakenModification(),
                ModificationType.PredictionModification => new PredictionModification(),
                ModificationType.HitscanModification => new HitscanModification(),
                _ => throw new ArgumentException($"Unknown modification type: {modType}"),
            };
        }
    }
}
