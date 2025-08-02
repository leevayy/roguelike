using UnityEngine;

namespace utility
{
    /// <summary>
    /// Interface for entities that can be alive/dead and have health
    /// </summary>
    public interface IAliveEntity
    {
        bool IsAlive { get; }
        bool IsGrounded { get; }
        float HealthPoints { get; }
        float MaxHealthPoints { get; }
        Vector3 Position { get; }
        Transform Transform { get; }
        ComposableModificationManager ModManager { get; }
        
        /// <summary>
        /// Gets the current alive state of this entity
        /// </summary>
        AliveState GetAliveState();
    }
}
