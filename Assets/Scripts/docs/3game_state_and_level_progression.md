# Aspect 1: Game State & Level Progression

## Purpose and Role

This aspect is the backbone of the game, controlling the overall flow, player objectives, and difficulty scaling. It is managed primarily by the `GameManager` singleton, which acts as the central authority for the game state. Its responsibilities include tracking player progress, managing level goals, triggering events, and handling the win/loss conditions.

## Relevant Files and Classes

-   **`GameManager.cs`**: The central singleton that orchestrates the entire game. It manages the main game loop, score, player health, goals, and interactions between other managers.
-   **`utility/Goal.cs`**: Defines the structure of a player's objective, such as killing a certain number of enemies or reaching a specific score.
-   **`RoomsManager.cs`**: Although simple in its current implementation, this class is intended to manage the different rooms or areas of a level.
-   **`Elevator.cs`**: A component that handles the physical transition between levels or floors.

## Interactions with Other Parts

-   **Player System**: `GameManager` holds a direct reference to the `Player` to monitor health, deduct score on taking damage, and grant healing upon goal completion.
-   **Enemy System**: It commands the `EnemySpawner` to start and stop spawning enemies based on the current game state (e.g., when a goal is active). It also processes `OnKill` events to update the score and goal progress.
-   **Shop & Economy System**: After a goal is completed, `GameManager` instructs the `ShopManager` to refresh its items for the player to purchase.
-   **UI**: `GameManager` is responsible for updating all major UI elements, including the timer, score, kill count, current goal, and the game over screen.

## Code Snippets and Logic

### The Goal System

The game progresses through a series of goals. When a goal is completed, the `GameManager` advances to the next one, increases the difficulty, and rewards the player.

The `goal` property in `GameManager` has a custom setter that triggers this entire sequence.

```csharp
// In GameManager.cs

private Goal _goal;
public Goal goal
{
    get => _goal;
    private set
    {
        winSound.Play(); // Reward the player

        if (_goalNumber == 9)
        {
            ShowGameOverScreen(true); // Win condition
        }
        
        _goal = value;
        _goalNumber++; // Increase difficulty counter
        killCount = 0;

        player.Heal(); // Heal player between rounds
        
        GameUI.instance.UpdateGoal(_goalNumber, value);

        if (_goalNumber > 1)
        {
            enemySpawner.StopSpawning();
            KillAll();
            RerollShop(); // Prepare for the shopping phase
            _ = DrawToExit(); // UI cue to find the elevator
        }
    }
}

// A new goal is assigned when the current one is met.
if (goal.Type == GoalType.KILL_N_ENEMIES && value >= goal.N)
{
    goal = GetNextGoal(_goalNumber);
}
```

### Difficulty Scaling

Difficulty is scaled implicitly through the `_goalNumber`. This integer is used to calculate enemy health, the number of modifications they spawn with, and the price of items in the shop.

```csharp
// In EnemyInstance.cs - Enemy health scales with the level (goal number)
private static int HealthScaleFunction(int lvl)
{
    return Mathf.RoundToInt((75 + 25 * (float)(lvl * lvl)) / 100) * 100;
}

// In GameManager.cs - Shop prices can be affected by the goal number
public void RerollShop()
{
    const int minPrice = 300;
    const int variety = 100;
    var discount = _goalNumber switch
    {
        2 => 90,
        3 => 60,
        _ => 0
    };
    // ...
}
```
