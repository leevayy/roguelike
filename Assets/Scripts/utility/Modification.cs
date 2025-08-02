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
        // turns out too OP, too much pain in the ass when you try to think about other mods interactions
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
                _ => throw new System.ArgumentException($"Unknown modification type: {modType}"),
            };
        }
    }

    // We still need to create these two missing concrete modification classes
    public class AddFlatValueModification : BaseModification
    {
        public override string Name => "Плоский урон";
        public override string Description => "Добавляет 15 единиц урона";
        public override UnityEngine.Material Material => Resources.Load<Material>("Materials/AddFlatValueLens");

        public override float GetModifiedValue(utility.AliveState aliveState, float baseValue) => baseValue + 15f;
    }

    public class AddMultiplyValueModification : BaseModification
    {
        public override string Name => "Множитель урона";
        public override string Description => "Умножает урон в 2 раза";
        public override UnityEngine.Material Material => Resources.Load<Material>("Materials/AddMultiplyValueLens");

        public override float GetModifiedValue(utility.AliveState aliveState, float baseValue) => baseValue * 2f;
    }

    public class RubberDuckModification : BaseModification
    {
        public override string Name => "Резиновая утка";
        public override string Description => "Призывает полезного компаньона-утку";
        public override UnityEngine.Material Material => Resources.Load<Material>("Materials/RubberDuckLens");

        // This modification might need special handling during game initialization
        // For now, we'll keep it simple and implement the basic interface
        public override void ApplyOnUpdate(utility.AliveState aliveState)
        {
            // RubberDuck logic would go here - possibly spawning a companion
            // This might need to be handled differently, possibly in GameManager
        }
    }
}
