using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;  // Speed of the movement (how fast the object moves)
    [SerializeField] private float moveBurstRange = 6f;  // Maximum distance for each burst of movement
    [SerializeField] private float minInterval = 2f;  // Minimum time in seconds between movements
    [SerializeField] private float maxInterval = 3f;  // Maximum time in seconds between movements
    [SerializeField] private Weapon weapon;
    [SerializeField] private Collider planeCollider;
    
    private float leftLimit, rightLimit;
    private bool isMoving = false;

    private bool isActive = false;

    private void Start()
    {
        if (planeCollider != null)
        {
            var planeBounds = planeCollider.bounds;

            leftLimit = planeBounds.min.x;
            rightLimit = planeBounds.max.x;
        }
        else
        {
            Debug.LogError("Plane Collider not assigned.");
        }
    }

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

        var direction = GetMoveDirection();

        var moveDistance = direction * Random.Range(3f, moveBurstRange) + 1f;

        var targetX = transform.position.x + moveDistance;
        if (targetX > rightLimit)
        {
            targetX -= 2 * moveDistance;
        }
        if (targetX < leftLimit)
        {
            targetX += 2 * moveDistance;
        }

        targetX = Mathf.Clamp(targetX, leftLimit, rightLimit);

        var startPositionX = transform.position.x;
        var journeyLength = Mathf.Abs(targetX - startPositionX);
        var startTime = Time.time;

        while (Mathf.Abs(transform.position.x - targetX) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            transform.position = new Vector3(Mathf.Lerp(startPositionX, targetX, fractionOfJourney), transform.position.y, transform.position.z);

            yield return null;
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        
        Shoot();

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
