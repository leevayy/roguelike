using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using utility;

public class OneshotInTheAirModification : BaseModification
{
    const int AIRSHOT_DAMAGE = 9999;
    public override string Name => "Oneshot Trickshot";
    public override string Description => $"Выстрелы в воздухе наносят {AIRSHOT_DAMAGE} урона";
    public override Material Material => Resources.Load<Material>("Materials/MoveSpeedLens");

    public override float GetModifiedValue(AliveState aliveState, float baseValue)
    {
        if (aliveState.IsGrounded)
        {
            return baseValue;
        }

        return AIRSHOT_DAMAGE;
    }
}
