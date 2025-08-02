using UnityEngine;

namespace utility
{
    /// <summary>
    /// Represents the current state of an alive entity that modifications need to access
    /// </summary>
    public struct AliveState
    {
        public bool IsAlive { get; }
        public bool IsGrounded { get; }
        public float HealthPoints { get; }
        public float MaxHealthPoints { get; }
        public Vector3 Position { get; }
        public Transform Transform { get; }
        public ComposableModificationManager ModManager { get; }
        
        public AliveState(bool isAlive, bool isGrounded, float healthPoints, float maxHealthPoints, 
                         Vector3 position, Transform transform, ComposableModificationManager modManager)
        {
            IsAlive = isAlive;
            IsGrounded = isGrounded;
            HealthPoints = healthPoints;
            MaxHealthPoints = maxHealthPoints;
            Position = position;
            Transform = transform;
            ModManager = modManager;
        }
        
        /// <summary>
        /// Creates an AliveState from an IAliveEntity
        /// </summary>
        public static AliveState FromEntity(IAliveEntity entity)
        {
            return new AliveState(
                entity.IsAlive,
                entity.IsGrounded,
                entity.HealthPoints,
                entity.MaxHealthPoints,
                entity.Position,
                entity.Transform,
                entity.ModManager
            );
        }
        
        /// <summary>
        /// Gets the health percentage (0.0 to 1.0)
        /// </summary>
        public float HealthPercentage => MaxHealthPoints > 0 ? HealthPoints / MaxHealthPoints : 0f;
        
        /// <summary>
        /// Returns true if the entity is at low health (below 25%)
        /// </summary>
        public bool IsLowHealth => HealthPercentage < 0.25f;
        
        /// <summary>
        /// Returns true if the entity is at full health
        /// </summary>
        public bool IsFullHealth => Mathf.Approximately(HealthPoints, MaxHealthPoints);
    }
}
