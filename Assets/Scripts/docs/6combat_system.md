# Aspect 4: Combat System

## Purpose and Role

The Combat System defines the core action mechanics of the game. It governs how both the player and enemies engage in combat, primarily through shooting projectiles. This system includes the logic for weapons, the projectiles themselves, hit detection, and damage calculation.

## Relevant Files and Classes

-   **`Weapon.cs`**: A component that can be attached to any character (player or enemy). It is responsible for calculating damage based on its modifications and for instantiating projectiles.
-   **`Laser.cs`**: The script attached to the projectile prefab. It controls the projectile's movement, lifetime, and carries damage information.
-   **`Hitbox.cs`**: A generic component placed on characters that can be damaged. It uses Unity's trigger system to detect collisions with projectiles and forwards the event to the appropriate character (`Player` or `EnemyInstance`).
-   **`ShotManager.cs`**: A singleton manager that ensures a single shotgun-like blast (composed of multiple projectiles) only registers as one "hit" for certain effects, like burning.
-   **`utility/GameHitEntity.cs`**: An enum (`Ally`, `Enemy`) used to identify the source and target of an attack, preventing friendly fire.

## Interactions with Other Parts

-   **Player and Enemy Systems**: Both `Player` and `Enemy` objects possess a `Weapon` component, which they use to `Shoot()`. They also have a `Hitbox` to receive damage.
-   **Modification & Item System**: The `Weapon`'s damage output is heavily influenced by `ModificationObject`s attached to it. The `GetDamage()` method in `Weapon.cs` dynamically calculates the final damage by iterating through its list of mods.
-   **Game State System**: The `GameManager` receives `HitInfo` from `Player` and `EnemyInstance` whenever a hit occurs. It uses this to play sounds, update the score, and display damage pop-ups.

## Code Snippets and Logic

### Shooting and Projectile Creation

The `Shoot` method in `Weapon.cs` is the entry point for any combat action. It handles cooldowns and instantiates the `Laser` prefab.

```csharp
// In Weapon.cs

public void Shoot(Quaternion rotation, float damage)
{
    if (_sinceLastShot < cooldown)
    {
        return; // Abort if on cooldown
    }
    _sinceLastShot = 0;
    
    // ... (sound effects) ...

    // Calculate how many projectiles to fire based on mods
    var damageMods = mods.Where((mod) => mod.mod.type == ModificationType.MultiplyMultiplyValue);
    var projectileCount = 1 * Mathf.Pow(2, damageMods.Count());

    // Generate a single ID for this entire shot/volley
    var shotId = ShotManager.Instance.GenerateNewShotId();

    for (var i = 0; i < projectileCount; i++)
    {
        var laser = Instantiate(laserPrefab, /* position */, rotation);
        var laserScript = laser.GetComponent<Laser>();

        // Assign damage and shot ID to the projectile
        laserScript.damage = damage;
        laserScript.shotId = shotId;

        // Apply special projectile properties from mods
        if (isOwnerPlayer && ModManager.instance.HasMod(ModificationType.BurnEffect))
        {
            laserScript.isBurn = true;
        }
    }
}
```

### Hit Detection and Damage Processing

Hit detection is decoupled from the characters. The `Hitbox` script on a character detects a trigger collision, and then the character's own script (`Player` or `EnemyInstance`) processes the result.

```csharp
// In Hitbox.cs - A simple event forwarder

private void OnTriggerEnter(Collider other)
{
    onTriggerEnterHandler?.Invoke(other, this);
}

// In EnemyInstance.cs - The handler is set up in Awake()

enemy.GetHitbox().SetOnTriggerEnterHandler((other, hitbox) =>
{
    if (!isAlive) return;

    // Check if the projectile is from the player
    var isHitByAlly = other.CompareTag("AllyProjectile");

    if (isHitByAlly)
    {
        var laser = other.GetComponent<Laser>();
        var laserDamage = laser.damage;
        
        // Notify the GameManager and apply damage to self
        GameManager.instance.OnHit(new HitInfo(GameHitEntity.Ally, GetDamage(laserDamage), laser.shotId), GameHitEntity.Enemy, collisionPoint);

        // Special effect processing using the ShotManager
        if (laser.isBurn)
        {
            // TryProcessHit ensures the burn effect is applied only once per shot volley
            if (ShotManager.Instance.TryProcessHit(laser.shotId))
            {
                _ = ApplyBurn((int)laserDamage / 2);
            }
        }
    }
});
```

### Dynamic Damage Calculation

The `Weapon` calculates its damage dynamically based on the modifications it holds. This allows for a flexible and expandable upgrade system.

```csharp
// In Weapon.cs

private float GetDamage()
{
    var flatValue = flatDamage;
    var multValue = 1f;

    // Sort mods to ensure consistent calculation order
    mods.Sort((mod1, mod2) => mod1.order - mod2.order);

    var damageMods = mods.Where(/* filters for damage-related mods */);

    foreach (var modObject in damageMods)
    {
        var mod = modObject.GetStats();
        switch (mod.type)
        {
            case ModificationType.AddFlatValue:
                flatValue += mod.value;
                break;
            case ModificationType.AddMultiplyValue:
                multValue += mod.value;
                break;
            case ModificationType.DoubleDamageAndTaken:
                multValue *= mod.value;
                break;
        }
    }
    
    return flatValue * multValue;
}
```
