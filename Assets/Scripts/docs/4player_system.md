# Aspect 2: Player System

## Purpose and Role

The Player System encapsulates all the components and logic that define the player character. This includes handling user input for movement and actions, managing the player's state (health, money), controlling the camera, and coordinating the player's physical interactions with the game world.

## Relevant Files and Classes

-   **`Player.cs`**: The main class for the player character. It acts as a hub, coordinating input, combat, and interactions with other systems.
-   **`MovementManager.cs`**: A dedicated component that handles the physics of player movement, including running and dashing.
-   **`CharacterAnimationController.cs`**: Manages the player's animations by translating movement and action states into animator parameters.
-   **`RagdollController.cs`**: Controls the transition between an animated character and a physics-driven ragdoll, typically upon death.
-   **`Weapon.cs`**: While part of the Combat System, the `Player`'s `Weapon` component is integral to this system as it's the primary tool for player action.

## Interactions with Other Parts

-   **Game State System**: The `Player`'s health and score are critical inputs to the `GameManager`. The `Player` receives commands like `Heal()` from the `GameManager` upon completing a goal.
-   **Input System**: The `Player` class directly processes keyboard and mouse input to drive movement (`GetAxisRaw`) and actions (`GetButtonDown("Fire1")`, `OnDash`).
-   **Combat System**: The `Player` initiates combat by calling the `Shoot()` method on its `Weapon` component. It also receives damage through its `Hitbox`, which processes collisions with enemy projectiles.
-   **Shop & Economy System**: The `Player`'s `BuyItem` method is called when interacting with a `BuyBox`, which checks if the player has enough score (money) to complete the purchase.
-   **Camera**: The `Player` script controls the main camera's position, creating a dynamic follow-camera that responds to player movement and mouse position.

## Code Snippets and Logic

### Input and Movement Handling

Input is gathered in `Player.Update()` and passed to `MovementManager` for processing. The physics-based movement is then applied in `FixedUpdate`. This separation is a standard Unity practice.

```csharp
// In Player.cs

private void Update()
{
    // Get raw input and pass it to the movement manager
    var moveInput = GetRawMoveInput();
    _movementManager.Tick(moveInput);

    // Update animations based on movement
    var movementDirection = transform.InverseTransformVector(_movementManager.MoveVector);
    _characterAnimationController.Tick(movementDirection);
    
    // Handle camera movement
    MoveCamera(moveInput);
}

private void FixedUpdate()
{
    // Apply physics-based movement
    _movementManager.FixedTick(_rb);

    // Make the player look at the mouse cursor
    var mouseWorldPosition = GetMouseWorldPosition();
    transform.LookAt(new Vector3(mouseWorldPosition.x, transform.position.y, mouseWorldPosition.z));
}
```

### Health and Damage

The `Player` class manages its own health. The `GetDamage` method encapsulates the logic for taking damage, which can be modified by items.

```csharp
// In Player.cs

public float Healthpoints { 
    get => _healthpoints;
    private set
    {
        _healthpoints = value;
        // Update UI and other listeners
        GameUI.instance.UpdateHp((int)value, (int)_maxHealthpoints);
        OnHealthPointsChanged(value, _maxHealthpoints);
    } 
}

private float GetDamage(float damageIn)
{
    // Apply damage modifiers before reducing health
    for (var i = 0; i < ModManager.instance.CountMod(ModificationType.DoubleDamageAndTaken); i++)
    {
        damageIn *= 2f;
    }
    
    Healthpoints -= damageIn;
    return damageIn;
}

// The hitbox receives the trigger event and initiates the damage sequence
hitbox.SetOnTriggerEnterHandler((other, _) =>
{
    var isHitByEnemy = other.CompareTag("EnemyProjectile");
    if (!isHitByEnemy) return;
    
    // ... (check for invulnerability)

    var laser = other.gameObject.GetComponent<Laser>();
    GameManager.instance.OnHit(
        // ... creates HitInfo with damage from GetDamage()
    );
});
```
