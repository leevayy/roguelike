using System;
using UnityEngine;
using System.Threading.Tasks;
using utility;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveBurstRange = 6f;
    [SerializeField] private float minInterval = 0.2f;
    [SerializeField] private float maxInterval = 1.5f;
    [SerializeField] private Weapon weapon;
    [SerializeField] private Hitbox hitbox;
    
    private ComposableModificationManager _modManager;
    
    protected Rigidbody _rb;
    protected CharacterAnimationController _characterAnimationController;
    protected RagdollController _ragdollController;

    protected GameObject _target;
    private Awaitable _currentAction;
    protected bool _isMoving;
    protected bool _shouldMove;
    private Func<AliveState> _getAliveState;

    public virtual void Initialize(ComposableModificationManager modManager, Func<AliveState> getAliveState)
    {
        _rb = GetComponent<Rigidbody>();
        _characterAnimationController = GetComponent<CharacterAnimationController>();
        _characterAnimationController.Initialize(false);
        _ragdollController = GetComponent<RagdollController>();

        _modManager = modManager;
        _getAliveState = getAliveState;
        weapon.OnShoot = () => _characterAnimationController.FireAnimation();
    }

    private void Update()
    {
        _characterAnimationController.Tick(transform.InverseTransformVector(_rb.linearVelocity));
    }
    
    protected virtual void FixedUpdate()
    {
        var isGrounded = Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1.1f);

        if (!isGrounded && _shouldMove)
        {
            StopMoving();

            return;
        }

        LookAtTarget();

        if (!_shouldMove && _target)
        {
            StartMoving();
        }
    }

    protected virtual void LookAtTarget()
    {
        if (_shouldMove && _target)
        {
            transform.LookAt(_target.transform.position);
        }
    }


    public void SET_MAX_SPEED()
    {
        moveSpeed = 20f;
        moveBurstRange = 12f;
        minInterval = 0.0f;
        maxInterval = 0.1f;
    }

    public Hitbox GetHitbox()
    {
        return hitbox;
    }

    public void StartMoving()
    {
        _shouldMove = true;
        StartCoroutine(MoveRandomly());
    }

    public void PickTarget(GameObject newTarget)
    {
        _target = newTarget;
    }

    protected void StopMoving()
    {
        if (!_shouldMove) return;
        
        _shouldMove = false;
        if (_currentAction == null) return;
        
        if (!_currentAction.IsCompleted)
        {
            _currentAction.Cancel();
        }
    }

    private async Awaitable MoveRandomly()
    {
        while (_shouldMove)
        {
            if (_isMoving)
            {
                await Task.Yield(); // Allows the game loop to progress
                continue;
            }

            var randomInterval = Random.Range(minInterval, maxInterval);
            
            try
            {
                _currentAction = Awaitable.WaitForSecondsAsync(0.1f);
                await _currentAction;
                
                _currentAction = MoveBurst();
                await _currentAction;
                
                _currentAction = Awaitable.WaitForSecondsAsync(randomInterval);
                await _currentAction;
            }
            // can be cancelled so it's okay
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    protected virtual async Awaitable MoveBurst() // Changed to public for external calling if needed
    {
        _isMoving = true;
        if (_characterAnimationController != null)
        {
            // Set IsMoving to true at the start of movement
            _characterAnimationController.Tick(Vector3.zero); // Or an initial direction if you have one
        }

        var directionX = GetMoveDirection();
        var directionZ = GetMoveDirection();

        var moveDistanceX = Random.Range(3f, moveBurstRange);
        var moveDistanceZ = Random.Range(3f, moveBurstRange);

        var targetX = transform.position.x + directionX * moveDistanceX;
        var targetZ = transform.position.z + directionZ * moveDistanceZ;

        var limits = GameField.current.GetLimits();

        if (targetX > limits.Right)
        {
            targetX -= 2 * moveDistanceX;
        }
        if (targetX < limits.Left)
        {
            targetX += 2 * moveDistanceX;
        }
        targetX = Mathf.Clamp(targetX, limits.Left, limits.Right);

        if (targetZ > limits.Upper)
        {
            targetZ -= 2 * moveDistanceZ;
        }
        if (targetZ < limits.Lower)
        {
            targetZ += 2 * moveDistanceZ;
        }
        targetZ = Mathf.Clamp(targetZ, limits.Lower, limits.Upper); // Corrected from limits.Right to limits.Upper

        var startPosition = _rb.position;
        var targetPosition = new Vector3(targetX, _rb.position.y, targetZ);

        var journeyLength = Vector3.Distance(startPosition, targetPosition);
        if (journeyLength < 0.01f)
        {
            _isMoving = false;
            Shoot();
            if (_characterAnimationController != null)
            {
                // Reset IsMoving to false if no movement occurs
                _characterAnimationController.Tick(Vector3.zero);
            }
            return;
        }

        var startTime = Time.time;

        while (Vector3.Distance(_rb.position, targetPosition) > 0.01f)
        {
            if (!_shouldMove)
            {
                break;
            }

            var distanceCovered = (Time.time - startTime) * moveSpeed;
            var fractionOfJourney = distanceCovered / journeyLength;

            // Calculate the current movement direction
            var currentMovement = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            var movementDirection = transform.InverseTransformVector(currentMovement - _rb.position); // Calculate direction relative to character

            _rb.MovePosition(currentMovement);

            if (_characterAnimationController != null)
            {
                _characterAnimationController.Tick(movementDirection.normalized); // Pass normalized direction
            }

            await Task.Yield();
        }

        if (_shouldMove)
        {
            _rb.MovePosition(targetPosition);
            Shoot();
        }

        _isMoving = false;
        if (_characterAnimationController != null)
        {
            // Set IsMoving to false when movement stops
            _characterAnimationController.Tick(Vector3.zero);
        }
    }
    
    private float GetMoveDirection()
    {
        var direction = Random.Range(-1f, 1f);
        
        return direction / Math.Abs(direction);
    }

    protected void Shoot()
    {
        weapon.Shoot(_getAliveState(), transform.rotation, _modManager.GetModifications());
    }

    public void Die()
    {
        _characterAnimationController.Die();
        StopMoving();
        PickTarget(null);
        _ragdollController.Die();

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var enemyColliders = GetComponentsInChildren<Collider>();
            var playerColliders = player.GetComponentsInChildren<Collider>();
            
            foreach (var enemyCol in enemyColliders)
            {
                foreach (var playerCol in playerColliders)
                {
                    if (enemyCol != null && playerCol != null)
                    {
                        Physics.IgnoreCollision(enemyCol, playerCol, true);
                    }
                }
            }
        }
    }
}
