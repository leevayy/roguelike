using System;
using UnityEngine;
using System.Threading.Tasks;
using utility;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;  // Speed of the movement (how fast the object moves)
    [SerializeField] private float moveBurstRange = 6f;  // Maximum distance for each burst of movement
    [SerializeField] private float minInterval = 0.2f;  // Minimum time in seconds between movements
    [SerializeField] private float maxInterval = 1.5f;  // Maximum time in seconds between movements
    [SerializeField] private Weapon weapon;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private GameObject modificationPrefab;
    
    private Rigidbody _rb;
    private CharacterAnimationController _characterAnimationController;
    private RagdollController _ragdollController;

    private GameObject _target;
    private Awaitable _currentAction;
    private bool _isMoving;
    private bool _shouldMove;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _characterAnimationController = GetComponent<CharacterAnimationController>();
        _ragdollController = GetComponent<RagdollController>();
    }

    private void Update()
    {
        _characterAnimationController.Tick(transform.InverseTransformVector(_rb.linearVelocity));
    }
    
    private void FixedUpdate()
    {
        var isGrounded = Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1.1f);

        if (!isGrounded && _shouldMove)
        {
            StopMoving();

            return;
        }

        if (_shouldMove && _target)
        {
            transform.LookAt(_target.transform.position);
        }

        if (!_shouldMove && _target)
        {
            StartMoving();
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

    private void StopMoving()
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

    private async Awaitable MoveBurst() // Changed to public for external calling if needed
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

    private void Shoot()
    {
        weapon.Shoot(transform.rotation);
    }
    
    public void AddModification(Modification modification)
    {
        var mod = Instantiate(modificationPrefab, weapon.transform);
        
        var modObject = mod.GetComponent<ModificationObject>();
        
        weapon.AddModification(modObject, modification);
    }

    public void Die()
    {
        _characterAnimationController.Die();
        StopMoving();
        PickTarget(null);
        _ragdollController.Die();
    }
}
