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
    
    private GameObject target;
    private Awaitable currentAction;
    private bool isMoving;
    private bool shouldMove;

    public Hitbox GetHitbox()
    {
        return hitbox;
    }

    public void StartMoving()
    {
        shouldMove = true;
        StartCoroutine(MoveRandomly());
    }

    private void FixedUpdate()
    {
        if (target)
        {
            transform.LookAt(target.transform.position);
        }
    }

    public void PickTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    public void StopMoving()
    {
        shouldMove = false;
        currentAction.Cancel();
    }

    private async Awaitable MoveRandomly()
    {
        while (shouldMove)
        {
            var randomInterval = Random.Range(minInterval, maxInterval);

            try
            {
                currentAction = Awaitable.WaitForSecondsAsync(randomInterval);
            
                await currentAction;
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                {
                    return;
                }
            }

            if (isMoving)
                continue;

            try
            {
                currentAction = MoveBurst();  
            
                await currentAction;
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
        isMoving = true;
        
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
            if (!shouldMove)
                break;
            
            var distanceCovered = (Time.time - startTime) * moveSpeed;
            var fractionOfJourney = distanceCovered / journeyLength;

            // Interpolate position in a straight line
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            await Task.Yield();
        }

        if (shouldMove)
        {
            // Ensure the object reaches the target position exactly
            transform.position = targetPosition;

            Shoot();
        }

        isMoving = false;
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
