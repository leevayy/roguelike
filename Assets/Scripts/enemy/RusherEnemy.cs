using System.Runtime.CompilerServices;
using System;
using UnityEngine;
using utility;

public class RusherEnemy : Enemy
{
    [SerializeField] private float rushSpeed = 12f;
    [SerializeField] private float rushDistance = 15f;
    [SerializeField] private float meleeRange = 2f;

    public override void Initialize(ComposableModificationManager modManager, Func<AliveState> getAliveState, bool isPredictionEnabled = false)
    {
        base.Initialize(modManager, getAliveState, isPredictionEnabled);

        _characterAnimationController.Initialize(true);

        // Synchronize melee weapon range with enemy's melee range
        var meleeWeapon = GetComponentInChildren<MeleeWeapon>();
        if (meleeWeapon != null)
        {
            meleeWeapon.SetMeleeRange(meleeRange);

        }
        else
        {

        }
    }

    protected override void FixedUpdate()
    {
        LookAtTarget();
    }

    protected override async Awaitable MoveBurst()
    {
        if (_target == null)
        {
            // Fall back to base movement if no target
            await base.MoveBurst();
            return;
        }

        _isMoving = true;

        var distanceToTarget = Vector3.Distance(transform.position, _target.transform.position);

        // If close enough for melee, don't move - just attack
        // Use a slightly larger range for detection to ensure we can hit
        if (distanceToTarget <= meleeRange * 1.2f)
        {
            _isMoving = false;

            Shoot(); // This will use the melee weapon
            return;
        }

        // Rush directly toward the player
        await RushTowardsTarget();

        _isMoving = false;
    }

    private async Awaitable RushTowardsTarget()
    {
        var startPosition = transform.position;
        var targetPosition = _target.transform.position;
        var direction = (targetPosition - startPosition).normalized;

        // Calculate rush endpoint (limited by rushDistance)
        var distance = Vector3.Distance(startPosition, targetPosition);
        var actualRushDistance = Mathf.Min(distance, rushDistance);
        var rushEndpoint = startPosition + direction * actualRushDistance;

        if (_isPredictionEnabled)
        {
            _targetPosition = rushEndpoint;
        }

        // Ensure we stay within game field bounds
        var limits = GameField.current.GetLimits();
        rushEndpoint.x = Mathf.Clamp(rushEndpoint.x, limits.Left, limits.Right);
        rushEndpoint.z = Mathf.Clamp(rushEndpoint.z, limits.Lower, limits.Upper);

        var journeyLength = Vector3.Distance(startPosition, rushEndpoint);
        if (journeyLength < 0.01f)
        {
            return;
        }

        var startTime = Time.time;

        // Rush movement loop
        while (Vector3.Distance(transform.position, rushEndpoint) > 0.1f && _shouldMove)
        {
            var distanceCovered = (Time.time - startTime) * rushSpeed;
            var fractionOfJourney = distanceCovered / journeyLength;

            if (fractionOfJourney >= 1f)
            {
                _rb.MovePosition(rushEndpoint);
                break;
            }

            var currentPosition = Vector3.Lerp(startPosition, rushEndpoint, fractionOfJourney);
            _rb.MovePosition(currentPosition);

            // Update animation
            if (_characterAnimationController != null)
            {
                var movementDirection = transform.InverseTransformVector(direction);
                _characterAnimationController.Tick(movementDirection.normalized);
            }

            await Awaitable.NextFrameAsync();
        }

        if (_isPredictionEnabled)
        {
            _targetPosition = Vector3.zero;
        }


        if (!_target) return;

        var finalDistance = Vector3.Distance(transform.position, _target.transform.position);
        if (finalDistance <= meleeRange * 1.2f)
        {
            Shoot(); // Melee attack
        }
        else { }
    }
}