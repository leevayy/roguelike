# Aspect 3: Enemy System

## Purpose and Role

The Enemy System is responsible for everything related to non-player characters that oppose the player. This includes their creation (spawning), their behavior (AI), their stats, and how they are managed and cleaned up. The system is designed to provide a scalable challenge that increases as the player progresses through the game.

## Relevant Files and Classes

-   **`EnemySpawner.cs`**: A manager responsible for instantiating enemy prefabs into the game world at specific intervals and locations.
-   **`EnemyInstance.cs`**: A wrapper class for an enemy, managing its instance-specific data like health, name, and alive status. It acts as the bridge between the high-level `GameManager` and the low-level `Enemy` behavior.
-   **`Enemy.cs`**: The core "brain" of an enemy, containing the AI logic for movement and combat (shooting).
-   **`BaseEnemy.cs`**: An abstract base class, currently empty, intended for shared enemy logic in the future.
-   **`GameField.cs`**: A singleton that defines the boundaries of the playable area, which enemies use to constrain their movement.

## Interactions with Other Parts

-   **Game State System**: `GameManager` controls the `EnemySpawner`, telling it when to `SpawnEnemies` and when to `StopSpawning`. The spawner's rate and the enemies' stats are determined by the `_goalNumber` from `GameManager`.
-   **Player System**: Enemies are given a reference to the `Player`'s `GameObject` as a target. The `Enemy` AI uses this target to `LookAt` and move towards the player's general area.
-   **Combat System**: Each `Enemy` has a `Weapon` component that it uses to shoot at the player. Their `Hitbox` component detects collisions with player projectiles, leading to damage calculation within `EnemyInstance`.
-   **UI**: `EnemyInstance` health changes are displayed via the `HealthBarManager`.

## Code Snippets and Logic

### Enemy Spawning

The `EnemySpawner` is activated by the `GameManager`. It uses a coroutine-like pattern with `Awaitable` to spawn enemies at random intervals. Spawning only occurs when the player is within the spawner's trigger volume.

```csharp
// In EnemySpawner.cs

// Called by GameManager to arm the spawner
public void SpawnEnemies(Action<EnemyInstance> onSpawn)
{
    _shouldSpawn = true;
    _cachedOnSpawn = onSpawn;
}

// Spawning loop starts when the player enters the trigger
private async Awaitable StartSpawningLoop()
{
    _isActive = true;
    while (_isActive && _playerInside)
    {
        // Interval is based on the current goal number (difficulty)
        var goalNumber = GameManager.instance.GetGoalNumber();
        var randomInterval = Random.Range(5f, 7f) / (0.5f * goalNumber);

        await Awaitable.WaitForSecondsAsync(randomInterval);

        if (_playerInside)
        {
            SpawnEnemy(_cachedOnSpawn);
        }
    }
    _isActive = false;
}

private void OnTriggerEnter(Collider other)
{
    if (!_shouldSpawn || !other.CompareTag("Player")) return;
    _playerInside = true;
    if (!_isActive)
    {
        _ = StartSpawningLoop(); // Start the loop
    }
}
```

### Enemy AI and Movement

The `Enemy.cs` script implements a simple "burst" movement AI. The enemy moves for a short duration towards a random point within the `GameField` boundaries, stops, shoots, and then waits for a random interval before repeating.

```csharp
// In Enemy.cs

private async Awaitable MoveRandomly()
{
    while (_shouldMove)
    {
        if (_isMoving)
        {
            await Task.Yield();
            continue;
        }

        var randomInterval = Random.Range(minInterval, maxInterval);
        
        try
        {
            // 1. Wait briefly
            await Awaitable.WaitForSecondsAsync(0.1f);
            // 2. Move in a burst
            await MoveBurst();
            // 3. Wait for the next cycle
            await Awaitable.WaitForSecondsAsync(randomInterval);
        }
        catch (OperationCanceledException) { break; }
    }
}

private async Awaitable MoveBurst()
{
    _isMoving = true;
    // ... calculate a random targetPosition within GameField limits ...

    var startTime = Time.time;
    while (Vector3.Distance(_rb.position, targetPosition) > 0.01f)
    {
        // ... Lerp position towards target ...
        _rb.MovePosition(currentMovement);
        await Task.Yield();
    }

    if (_shouldMove)
    {
        _rb.MovePosition(targetPosition);
        Shoot(); // Shoot after moving
    }
    _isMoving = false;
}
```

### Enemy Death

When an `EnemyInstance`'s health reaches zero, it triggers a death sequence. This involves disabling its AI, enabling ragdoll physics, and eventually "flattening" the model to visually clean it up.

```csharp
// In EnemyInstance.cs

private void Die(Hitbox hitbox)
{
    if (!isAlive) return;
    isAlive = false;

    enemy.Die(); // Calls Die() on the Enemy component
    hitbox.enabled = false;
    StartCoroutine(Flatten());
}

// In Enemy.cs
public void Die()
{
    _characterAnimationController.Die();
    StopMoving(); // Stop the AI coroutine
    PickTarget(null);
    _ragdollController.Die(); // Enable ragdoll physics
}
```
