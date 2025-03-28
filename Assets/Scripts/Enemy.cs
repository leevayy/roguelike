using System;
using UnityEngine;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;  // Speed of the movement (how fast the object moves)
    [SerializeField] private float moveBurstRange = 6f;  // Maximum distance for each burst of movement
    [SerializeField] private float minInterval = 2f;  // Minimum time in seconds between movements
    [SerializeField] private float maxInterval = 3f;  // Maximum time in seconds between movements
    [SerializeField] private Weapon weapon;
    [SerializeField] private Hitbox hitbox;
    
    private GameObject _target;
    private Awaitable _currentAction;
    private bool _isMoving;
    private bool _shouldMove;

    public void SET_MAX_SPEED()
    {
        moveSpeed = 20f;
        moveBurstRange = 12f;
        minInterval = 0.0f;
        maxInterval = 0.5f;
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

    private void FixedUpdate()
    {
        var isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

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

    public void PickTarget(GameObject newTarget)
    {
        _target = newTarget;
    }

    public void StopMoving()
    {
        _shouldMove = false;
        _currentAction.Cancel();
    }

    private async Awaitable MoveRandomly()
    {
        while (_shouldMove)
        {
            var randomInterval = Random.Range(minInterval, maxInterval);

            try
            {
                _currentAction = Awaitable.WaitForSecondsAsync(randomInterval);
            
                await _currentAction;
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                {
                    return;
                }
            }

            if (_isMoving)
                continue;

            try
            {
                _currentAction = MoveBurst();  
            
                await _currentAction;
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                {
                    return;
                }
            }
        }
    }

    private async Awaitable MoveBurst()
    {
        _isMoving = true;
        
        var directionX = GetMoveDirection(); 
        var directionZ = GetMoveDirection(); 

        // Randomly choose move distances for both X and Z directions
        var moveDistanceX = Random.Range(3f, moveBurstRange);
        var moveDistanceZ = Random.Range(3f, moveBurstRange);

        // Calculate the final target position in both X and Z
        var targetX = transform.position.x + directionX * moveDistanceX;
        var targetZ = transform.position.z + directionZ * moveDistanceZ;

        // Get movement limits for X and Z
        var limits = GameField.instance.GetLimits();

        // Ensure the target positions don't exceed the limits
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
        targetZ = Mathf.Clamp(targetZ, limits.Lower, limits.Upper);

        var startPosition = transform.position;
        var targetPosition = new Vector3(targetX, transform.position.y, targetZ);
        
        var journeyLength = Vector3.Distance(startPosition, targetPosition);
        var startTime = Time.time;
        
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            if (!_shouldMove)
                break;
            
            var distanceCovered = (Time.time - startTime) * moveSpeed;
            var fractionOfJourney = distanceCovered / journeyLength;

            // Interpolate position in a straight line
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            await Awaitable.NextFrameAsync();
        }

        if (_shouldMove)
        {
            // Ensure the object reaches the target position exactly
            transform.position = targetPosition;

            Shoot();
        }

        _isMoving = false;
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
}
