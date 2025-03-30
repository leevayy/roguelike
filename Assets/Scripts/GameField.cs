using System;
using JetBrains.Annotations;
using UnityEngine;
using utility;
using Random = UnityEngine.Random;

public class GameField : MonoBehaviour
{
    public static GameField current { get; private set; }
    private Collider fieldCollider;
    [CanBeNull] private Limits cachedLimits;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        fieldCollider = GetComponent<Collider>();
    }

    public Vector3 GetRandomPointWithin()
    {
        var limits = GetLimits();
            
        var randomX = Random.Range(limits.Left, limits.Right);
        var randomZ = Random.Range(limits.Lower, limits.Upper);
        
        return new Vector3(randomX, transform.position.y + 0.1f, randomZ);
    }

    public Limits GetLimits()
    {
        if (cachedLimits != null)
        {
            return cachedLimits;
        }
        
        if (!fieldCollider)
        {
            throw new Exception("GameField's Collider is not assigned!");
        }

        var planeBounds = fieldCollider.bounds;
        
        var limits = new Limits(planeBounds.min.x, planeBounds.max.x, planeBounds.min.z, planeBounds.max.z);

        cachedLimits = limits;

        return limits;
    }
}