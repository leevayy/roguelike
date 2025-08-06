using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using utility;

public class OneshotInTheAirModification : BaseModification
{
    const float AIRSHOT_DAMAGE = 1000f;
    public override string Name => "Trickshot";
    public override string Description => $"Выстрелы в воздухе наносят {AIRSHOT_DAMAGE} урона";
    public override Material Material => Resources.Load<Material>("Materials/DamageLens");
    public override Rarity Rarity => Rarity.Legendary;

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        if (aliveState.IsGrounded)
        {
            return baseValue;
        }

        return AIRSHOT_DAMAGE;
    }
}
