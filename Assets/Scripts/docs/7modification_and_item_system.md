# Aspect 5: Modification & Item System

## Purpose and Role

This system provides the core roguelike element of the game: persistent and stackable upgrades. Modifications (or "mods") are items that, once acquired, grant passive bonuses to the player. These can range from simple stat increases (like more damage) to unique gameplay effects (like healing on kill). This system is central to player progression and build variety.

## Relevant Files and Classes

-   **`utility/Modification.cs`**: Defines the data structure for a modification, including its `ModificationType` (an enum) and its `value`. It handles the logic for creating a random modification.
-   **`ModificationObject.cs`**: The `MonoBehaviour` component that is physically attached to a character's `Weapon`. It holds a `Modification` data object and applies a specific material based on the mod's type for visual identification.
-   **`ModManager.cs`**: A singleton that provides helper functions to check for the presence or count of specific modifications on the player (`HasMod`, `CountMod`). It also handles the logic for dropping mods on the ground.
-   **`Player.cs` / `Weapon.cs`**: The `Player` and its `Weapon` are the primary consumers of modifications. The `Weapon` class holds the list of `ModificationObject`s and uses them to calculate its stats.

## Interactions with Other Parts

-   **Combat System**: The `Weapon.GetDamage()` method directly queries its list of `ModificationObject`s to calculate the final damage, projectile count, and other combat effects like `BurnEffect` or `GhostLaser`.
-   **Player System**: The `Player` script itself can be affected by mods. For example, `HealOnKill` is checked in `GameManager.OnKill()` but the healing is applied to the `Player`. `MoveSpeedIncrease` is checked by the `MovementManager`.
-   **Shop & Economy System**: Modifications are the primary items sold in the shop. A `StoreItem` of type `Modification` is created, which the player can then buy.
-   **Enemy System**: Enemies can also have modifications. The `EnemyInstance` class has logic to add a number of random mods based on the current game level (`ModCountScaleFunction`).

## Code Snippets and Logic

### Modification Data Structure

The `Modification` class is a simple data container. The `ModificationType` enum is the key to its behavior.

```csharp
// In utility/Modification.cs

public enum ModificationType
{
    AddFlatValue,
    AddMultiplyValue,
    MultiplyMultiplyValue,
    RubberDuck,
    MoneyEqualsLife,
    // ... and so on
}

public class Modification
{
    public ModificationType type { get; private set; }
    public float value { get; private set; }

    // Constructor for a completely random modification
    public Modification() : this(GetRandomModificationType())
    {
    }

    // Constructor for a specific type, assigning a default value
    public Modification(ModificationType modType)
    {
        type = modType;
        value = type switch
        {
            ModificationType.AddFlatValue => 15f,
            ModificationType.AddMultiplyValue => 2f,
            // ... other default values
            _ => 1f,
        }; 
    }
}
```

### Applying Modifications

When a player buys a modification, a `ModificationObject` prefab is instantiated and attached to the player's weapon.

```csharp
// In Player.cs

private void AddModification(Modification modification, string name)
{
    // 1. Instantiate the visual representation of the mod
    var mod = Instantiate(modificationPrefab, weapon.transform);
    _modObjects.Add(mod);

    var modObject = mod.GetComponent<ModificationObject>();

    // 2. Tell the weapon to add the mod to its logic
    weapon.AddModification(modObject, modification);
}

// In Weapon.cs
public void AddModification(ModificationObject mod, Modification modification)
{
    // 3. Initialize the mod object with its data and order
    mod.Init(modification, mods.Count);
    mods.Add(mod);
}
```

### Querying for Modifications

Other systems use `ModManager` as a convenient, centralized place to ask if the player has a certain upgrade. This decouples the systems from needing to know about the `Player`'s `Weapon` internals.

```csharp
// In ModManager.cs - A simple helper

public bool HasMod(ModificationType modType)
{
    return GameManager.instance.modifications.ToList().Exists(mod => mod.type == modType);
}

// In GameManager.cs - Example usage

public void OnKill()
{
    score += GetInterest() + Random.Range(7, 9);

    // Check for the HealOnKill mod before healing the player
    if (ModManager.instance.HasMod(ModificationType.HealOnKill))
    {
        player.Heal(0.2f);
    }
    
    killCount++;
}
```
