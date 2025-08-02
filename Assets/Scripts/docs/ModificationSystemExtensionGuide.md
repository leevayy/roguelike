# Modification System Extension Guide

This guide explains how to extend the modification system

## Table of Contents
1. [System Overview](#system-overview)
2. [Adding New Modifications](#adding-new-modifications)
3. [Available Triggers/Hooks](#available-triggershooks)
4. [Adding New Triggers](#adding-new-triggers)
5. [Material and Visual Setup](#material-and-visual-setup)
6. [Testing Your Modifications](#testing-your-modifications)
7. [Common Patterns](#common-patterns)

## System Overview

The modification system uses a strategy pattern with the following key components:

- **`IModification`**: Interface defining all available hooks and metadata
- **`BaseModification`**: Abstract base class with default implementations
- **`ModificationType`**: Enum listing all modification types
- **`Modification`**: Factory class that creates strategy instances
- **`ComposableModificationManager`**: Manages and applies modifications
- **Concrete Modification Classes**: Individual modification implementations

## Adding New Modifications

### Step 1: Add to ModificationType Enum

```csharp
// In Assets/Scripts/utility/Modification.cs
public enum ModificationType
{
    // ... existing types
    YourNewModification,  // Add your new type here
}
```

### Step 2: Create Modification Class

Create a new file `Assets/Scripts/modifications/YourNewModificationModification.cs`:

```csharp
using UnityEngine;

public class YourNewModificationModification : BaseModification
{
    public override string Name => "Название модификации";
    public override string Description => "Описание того, что делает модификация";
    public override Material Material => Resources.Load<Material>("Materials/YourMaterialLens");

    // Override the hooks you need (see Available Triggers section)
    public override void ApplyOnShoot(Weapon weapon, float damage)
    {
        // Your shoot logic here
    }
    
    public override float ModifyIncomingDamage(Player player, float damage)
    {
        // Your damage modification logic here
        return damage; // Remember to return the modified damage
    }
    
    // ... other hooks as needed
}
```

### Step 3: Register in Factory

Add your modification to the factory in `Modification.cs`:

```csharp
private static IModification CreateStrategy(ModificationType modType)
{
    return modType switch
    {
        // ... existing cases
        ModificationType.YourNewModification => new YourNewModificationModification(),
        _ => throw new System.ArgumentException($"Unknown modification type: {modType}"),
    };
}
```

### Step 4: Add Default Value (Optional)

If your modification uses the `Value` property, add it to the `GetValue` method:

```csharp
private static float GetValue(ModificationType modType)
{
    return modType switch
    {
        // ... existing cases
        ModificationType.YourNewModification => 25f, // Your default value
        _ => 1f,
    };
}
```

## Available Triggers/Hooks

### Core Hooks

```csharp
public interface IModification
{
    // Metadata
    string Name { get; }
    string Description { get; }
    Material Material { get; }

    // Event Hooks
    void ApplyOnShoot(Weapon weapon, float damage);
    float ModifyIncomingDamage(Player player, float damage);
    void ApplyOnUpdate(Player player);
    void ApplyOnKill(Player player);
    float GetModifiedValue(float baseValue);
    int GetProjectileCount(int baseCount);
    void ApplyOnTakeDamage(Player player, float damage);
}
```

### Hook Descriptions

| Hook | When Called | Use Cases |
|------|-------------|-----------|
| `ApplyOnShoot` | When player shoots | Apply effects to weapon/projectiles, consume ammo, trigger abilities |
| `ModifyIncomingDamage` | When player takes damage | Reduce/increase damage, apply shields, trigger counters |
| `ApplyOnUpdate` | Every frame | Continuous effects, regeneration, timed abilities |
| `ApplyOnKill` | When player kills an enemy | Healing, resource gain, explosive effects |
| `GetModifiedValue` | For damage calculations | Modify base damage, speed, or other numeric values |
| `GetProjectileCount` | When calculating projectiles | Multi-shot, spread shot effects |
| `ApplyOnTakeDamage` | After taking damage | Post-damage effects, invulnerability, retaliation |

### Hook Usage Examples

```csharp
// Modify projectile behavior
public override void ApplyOnShoot(Weapon weapon, float damage)
{
    var laser = weapon.GetComponent<Laser>();
    if (laser != null)
    {
        laser.isBurn = true; // Make projectiles burn
        laser.speed *= 2f;  // Double projectile speed
    }
}

// Damage reduction
public override float ModifyIncomingDamage(Player player, float damage)
{
    return damage * 0.5f; // Reduce damage by 50%
}

// Healing over time
public override void ApplyOnUpdate(Player player)
{
    if (Time.time % 1f < Time.deltaTime) // Once per second
    {
        player.Heal(0.02f); // Heal 2% max HP per second
    }
}

// Multi-shot
public override int GetProjectileCount(int baseCount)
{
    return baseCount * 2; // Double projectile count
}
```

## Adding New Triggers

If you need new triggers that don't exist, follow these steps:

### Step 1: Add to IModification Interface

```csharp
// In Assets/Scripts/utility/IModification.cs
public interface IModification
{
    // ... existing methods
    void YourNewHook(YourParameterType parameter);
}
```

### Step 2: Add Default Implementation to BaseModification

```csharp
// In Assets/Scripts/utility/BaseModification.cs
public abstract class BaseModification : IModification
{
    // ... existing methods
    public virtual void YourNewHook(YourParameterType parameter) { }
}
```

### Step 3: Add to ComposableModificationManager

```csharp
// In Assets/Scripts/ComposableModificationManager.cs
public void ApplyYourNewHook(YourParameterType parameter)
{
    foreach (var modification in _modifications)
    {
        modification.Strategy.YourNewHook(parameter);
    }
}
```

### Step 4: Call the Hook

Add calls to your new hook in the appropriate game systems:

```csharp
// Example: In a game system where the event occurs
player.modManager.ApplyYourNewHook(someParameter);
```

## Material and Visual Setup

### Creating Materials

1. Create a material in `Assets/Materials/` with the pattern `YourModNameLens.mat`
2. Copy the material to `Assets/Resources/Materials/` so it can be loaded by `Resources.Load`
3. Set up the material with appropriate textures and properties

### Material Loading

```csharp
public override Material Material => Resources.Load<Material>("Materials/YourModNameLens");
```

### Visual Representation

The visual ModificationObject will automatically use your material when the modification is equipped. The material determines how the lens looks on the player's weapon.

## Testing Your Modifications

### Using Debug Tools

1. **Debug Editor**: Use the `DebugToolsEditor` to spawn specific modifications
   ```csharp
   // In play mode, select an object with DebugTools component
   // Choose your modification type from the dropdown
   // Click "Buy Selected Modification"
   ```

2. **Console Testing**: Add debug logs to verify your hooks are called
   ```csharp
   public override void ApplyOnShoot(Weapon weapon, float damage)
   {
       Debug.Log($"YourModification: ApplyOnShoot called with damage {damage}");
       // Your logic here
   }
   ```

3. **Manual Testing**: Buy your modification from the shop and test its effects

## Common Patterns

### Stacking Modifications

```csharp
// For modifications that stack additively
public override float GetModifiedValue(float baseValue)
{
    return baseValue + 10f; // Each instance adds 10
}

// For modifications that stack multiplicatively
public override float GetModifiedValue(float baseValue)
{
    return baseValue * 1.5f; // Each instance multiplies by 1.5
}
```

### Cooldown-Based Abilities

```csharp
public class CooldownModification : BaseModification
{
    private float _lastActivation = 0f;
    private const float COOLDOWN = 5f;

    public override void ApplyOnShoot(Weapon weapon, float damage)
    {
        if (Time.time - _lastActivation >= COOLDOWN)
        {
            _lastActivation = Time.time;
            // Trigger ability
        }
    }
}
```

### Conditional Effects

```csharp
public override float ModifyIncomingDamage(Player player, float damage)
{
    // Only reduce damage if player health is low
    if (player.Healthpoints < player.MaxHealthpoints * 0.3f)
    {
        return damage * 0.5f; // 50% damage reduction when low health
    }
    return damage;
}
```

### Resource Management

```csharp
public override void ApplyOnKill(Player player)
{
    // Gain money on kill
    GameManager.instance.score += 5;
    
    // Heal on kill
    player.Heal(0.1f); // 10% max health
}
```

### Projectile Effects

```csharp
public override void ApplyOnShoot(Weapon weapon, float damage)
{
    var laser = weapon.GetComponent<Laser>();
    if (laser != null)
    {
        // Make projectiles pierce
        laser.isSolid = true;
        
        // Change projectile appearance
        laser.GetComponent<Renderer>().material.color = Color.red;
        
        // Add special effects
        laser.speed *= 1.5f;
    }
}
```

## Best Practices

1. **Always return modified values**: In hooks like `ModifyIncomingDamage`, always return the damage value
2. **Use descriptive names**: Make modification names clear in Russian for the UI
3. **Handle null checks**: Always check if components exist before modifying them
4. **Performance considerations**: Avoid expensive operations in `ApplyOnUpdate`
5. **Test edge cases**: Test with multiple instances of the same modification
6. **Material naming**: Follow the `XxxLens` naming convention for materials
7. **Documentation**: Add comments explaining complex modification logic

## Troubleshooting

### Common Issues

1. **Material not loading**: Ensure material is in `Assets/Resources/Materials/`
2. **Hook not called**: Check if the hook is properly implemented in ComposableModificationManager
3. **Modification not appearing in shop**: Verify it's added to the ModificationType enum and factory
4. **Visual issues**: Check if the material exists and follows naming conventions
5. **Compilation errors**: Ensure all required using statements are included

### Debug Tips

```csharp
// Add to your modification for debugging
public override void ApplyOnShoot(Weapon weapon, float damage)
{
    Debug.Log($"{Name}: Applying shoot effect");
    // Your logic
}
```

This guide should provide everything needed to extend the modification system with new abilities and triggers.
