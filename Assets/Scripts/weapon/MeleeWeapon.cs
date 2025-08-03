using System;
using System.Collections.ObjectModel;
using UnityEngine;
using utility;

public class MeleeWeapon : Weapon
{
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeAngle = 90f; // Attack cone angle
    [SerializeField] private LayerMask targetLayers = -1; // What can be hit
    [SerializeField] private AudioSource meleeSound;
    [SerializeField] private GameObject meleeEffect; // Visual effect for melee attack
    
    public void SetMeleeRange(float range)
    {
        meleeRange = range;
    }
    
    public float GetMeleeRange()
    {
        return meleeRange;
    }
    
    public override void Shoot(AliveState aliveState, Quaternion rotation, ReadOnlyCollection<Modification> modifications)
    {
        var damage = GetDamage(aliveState, modifications);
        MeleeAttack(rotation, damage, modifications);
    }

    public override void Shoot(Quaternion rotation, float damage, ReadOnlyCollection<Modification> modifications, int projectileCount = 1)
    {
        // For melee, projectileCount doesn't make sense, but we'll treat it as attack multiplier
        var totalDamage = damage * projectileCount;
        MeleeAttack(rotation, totalDamage, modifications);
    }

    private void MeleeAttack(Quaternion rotation, float damage, ReadOnlyCollection<Modification> modifications)
    {
        // Check cooldown
        if (_sinceLastShot < cooldown)
        {
            return;
        }

        _sinceLastShot = 0;

        Debug.Log("MeleeWeapon: Attacking with melee.");
        OnShoot?.Invoke();
        // Play melee sound
        if (meleeSound)
        {
            meleeSound.Play();
        }

        // Show visual effect
        if (meleeEffect)
        {
            var effect = Instantiate(meleeEffect, transform.position, rotation);
            Destroy(effect, 1f); // Destroy effect after 1 second
        }

        // Perform melee attack
        PerformMeleeHit(rotation, damage, modifications);
    }
    
    private void PerformMeleeHit(Quaternion rotation, float damage, ReadOnlyCollection<Modification> modifications)
    {
        var attackPosition = transform.position;
        
        // Debug logging to help identify issues
        Debug.Log($"Melee attack at {attackPosition}, range: {meleeRange}");
        
        // Find all colliders in range - use -1 for all layers initially
        var hitColliders = Physics.OverlapSphere(attackPosition, meleeRange, -1);
        
        Debug.Log($"Found {hitColliders.Length} potential targets");
        
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log($"Checking target: {hitCollider.name}, tag: {hitCollider.tag}");
            
            // For enemy weapons attacking player, skip angle check - just hit if in range
            if (!isOwnerPlayer && hitCollider.CompareTag("Player"))
            {
                Debug.Log($"Enemy melee weapon hitting player directly (no angle check)");
                DealDamageToTarget(hitCollider, damage, modifications);
                continue;
            }
            
            // For player weapons or other cases, use angle checking
            var attackDirection = rotation * Vector3.forward;
            var directionToTarget = (hitCollider.transform.position - attackPosition).normalized;
            var angleToTarget = Vector3.Angle(attackDirection, directionToTarget);
            
            Debug.Log($"Angle to target {hitCollider.name}: {angleToTarget}° (max: {meleeAngle / 2f}°)");
            
            if (angleToTarget <= meleeAngle / 2f)
            {
                // Check if it's a valid target
                if (IsValidTarget(hitCollider))
                {
                    Debug.Log($"Valid target found: {hitCollider.name}, dealing {damage} damage");
                    DealDamageToTarget(hitCollider, damage, modifications);
                }
                else
                {
                    Debug.Log($"Invalid target: {hitCollider.name}");
                }
            }
            else
            {
                Debug.Log($"Target {hitCollider.name} outside attack angle");
            }
        }
    }    private bool IsValidTarget(Collider target)
    {
        // Check if target has appropriate tags/components
        if (isOwnerPlayer)
        {
            // Player's melee weapon should hit enemies
            var isEnemy = target.CompareTag("Enemy");
            var hasHitbox = target.GetComponent<Hitbox>() != null;
            Debug.Log($"Player weapon checking {target.name}: isEnemy={isEnemy}, hasHitbox={hasHitbox}");
            return isEnemy && hasHitbox;
        }
        else
        {
            // Enemy's melee weapon should hit player
            var isPlayer = target.CompareTag("Player");
            Debug.Log($"Enemy weapon checking {target.name}: isPlayer={isPlayer}");
            return isPlayer;
        }
    }
    
    private void DealDamageToTarget(Collider target, float damage, ReadOnlyCollection<Modification> modifications)
    {
        // Create a temporary "melee projectile" to simulate laser damage system
        var meleeHit = new GameObject("MeleeHit");
        meleeHit.tag = "EnemyProjectile";
        meleeHit.transform.position = target.transform.position;
        
        // Add necessary components to simulate laser hit
        var collider = meleeHit.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.1f;

        var rb = meleeHit.AddComponent<Rigidbody>();
        
        var laser = meleeHit.AddComponent<Laser>();
        laser.damage = damage;
        laser.shotId = ShotManager.Instance.GenerateNewShotId();

        // Apply burn effect if weapon has it
        if (_shouldApplyBurn)
        {
            laser.isBurn = true;
        }

        foreach (var mod in modifications)
        {
            mod.Strategy.ApplyOnShoot(this, damage);
        }
        
        // Use reflection to call the private OnTriggerEnter method or use the public interface
        var hitboxComponent = target.GetComponent<Hitbox>();
        if (hitboxComponent)
        {
            // Trigger collision manually by calling the hitbox's trigger method
            hitboxComponent.SendMessage("OnTriggerEnter", collider, SendMessageOptions.DontRequireReceiver);
        }
        
        // Clean up the temporary object
        Destroy(meleeHit, 0.1f);
    }
    
    // Debug visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        
        // Draw attack cone
        Gizmos.color = Color.yellow;
        var forward = transform.forward;
        var leftBoundary = Quaternion.AngleAxis(-meleeAngle / 2f, transform.up) * forward;
        var rightBoundary = Quaternion.AngleAxis(meleeAngle / 2f, transform.up) * forward;
        
        Gizmos.DrawRay(transform.position, leftBoundary * meleeRange);
        Gizmos.DrawRay(transform.position, rightBoundary * meleeRange);
    }
}
