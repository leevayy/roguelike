# Aspect 7: Core Engine & Utilities

## Purpose and Role

This aspect comprises a collection of foundational, often generic, classes and managers that provide essential services to the rest of the application. They are not tied to a single gameplay feature but are instead the underlying tools and data structures that enable other systems to function cleanly.

## Relevant Files and Classes

-   **`utility/` folder**: This directory contains most of the core data structures.
    -   **`GameHitEntity.cs`**: Enum to differentiate between `Ally` and `Enemy` entities.
    -   **`Limits.cs`**: Class to define the 2D boundaries of the play area. Also contains `HitInfo` and `RandomName`.
    -   **`Modification.cs`**, **`StoreItem.cs`**, **`Goal.cs`**: Core data definitions for the main gameplay systems.
    -   **`TimeFormatter.cs`**: A static helper class for formatting time values.
-   **`ShotManager.cs`**: A singleton service to prevent single multi-projectile attacks from triggering effects (like burning) multiple times. It works by assigning a unique ID to each "shot" event.
-   **`GameField.cs`**: A singleton that provides a global way to access the physical boundaries of the level.
-   **`MusicManager.cs` / `SettingsManager.cs`**: Simple singletons to manage global settings like audio volume and camera mode.
-   **`SafeFallingPlane.cs`**: A utility object that acts as a safety net, teleporting any player or enemy that falls off the map back into the play area.

## Interactions with Other Parts

These utilities are used broadly across the codebase.

-   `GameField` is used by the `Enemy` for movement constraints and by `SafeFallingPlane` to determine a respawn point.
-   `ShotManager` is used by `Weapon` to generate a shot ID and by `EnemyInstance` and `Player` to check if a hit from that shot has already been processed for a specific effect.
-   The data structures in `utility/` are created and passed between all major systems. For example, `GameManager` creates a `Goal`, `ShopManager` creates `StoreItem`s which contain `Modification`s, and `Player`/`EnemyInstance` create `HitInfo` objects.
-   `SettingsManager` is queried by `Player` to determine camera behavior.

## Code Snippets and Logic

### GameField: Providing World Boundaries

`GameField` uses a collider's bounds to define the playable area. It caches the result for efficiency.

```csharp
// In GameField.cs

public class GameField : MonoBehaviour
{
    public static GameField current { get; private set; }
    private Collider fieldCollider;
    [CanBeNull] private Limits cachedLimits;

    private void Awake()
    {
        // Singleton setup...
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
        
        var planeBounds = fieldCollider.bounds;
        var limits = new Limits(planeBounds.min.x, planeBounds.max.x, planeBounds.min.z, planeBounds.max.z);
        cachedLimits = limits;
        return limits;
    }
}
```

### ShotManager: Ensuring "One Hit" Per Volley

This manager solves the problem of a single shotgun blast triggering a "per hit" effect (like a burn) for every single pellet.

```csharp
// In ShotManager.cs

public class ShotManager : MonoBehaviour
{
    // Singleton setup...
    private Dictionary<int, float> _processedShotIds = new Dictionary<int, float>();
    private int _nextShotId = 0;

    // Called by Weapon.cs before firing a volley of projectiles
    public int GenerateNewShotId()
    {
        _nextShotId++;
        return _nextShotId;
    }

    // Called by EnemyInstance.cs when processing a hit with a special effect
    public bool TryProcessHit(int shotId)
    {
        // If the ID is new, it's the first projectile from this volley to hit.
        if (!_processedShotIds.ContainsKey(shotId))
        {
            _processedShotIds.Add(shotId, Time.time);
            return true; // Success, process the effect.
        }
        // This shot ID has been seen before.
        return false; // Failure, do not process the effect again.
    }

    // Coroutine to periodically clean up old IDs from the dictionary
    private IEnumerator CleanupRoutine()
    {
        // ...
    }
}
```
