using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using utility;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;  // Speed of the movement (how fast the object moves)
    [SerializeField] private float moveBurstRange = 6f;  // Maximum distance for each burst of movement
    [SerializeField] private float minInterval = 2f;  // Minimum time in seconds between movements
    [SerializeField] private float maxInterval = 3f;  // Maximum time in seconds between movements
    [SerializeField] private Weapon weapon;
    [SerializeField] private Collider planeCollider;
    
    private bool isMoving = false;
    private bool isActive = false;

    public void StartMoving()
    {
        isActive = true;
        StartCoroutine(MoveRandomly());
    }

    public void StopMoving()
    {
        isActive = false;
    }

    private IEnumerator MoveRandomly()
    {
        while (isActive)
        {
            var randomInterval = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(randomInterval);

            if (isMoving)
                continue;

            StartCoroutine(MoveBurst());  
        }
    }

    private IEnumerator MoveBurst()
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
        var limits = GetLimits();

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
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            // Interpolate position in a straight line
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            yield return null;
        }

        // Ensure the object reaches the target position exactly
        transform.position = targetPosition;

        Shoot();

        isMoving = false;
    }


    private Limits GetLimits()
    {  
        if (planeCollider)
        {
            var planeBounds = planeCollider.bounds;

            return new Limits(planeBounds.min.x, planeBounds.max.x, planeBounds.max.z, planeBounds.min.z);
        }
        else
        {
            throw new Exception("Plane Collider not assigned.");
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
}
