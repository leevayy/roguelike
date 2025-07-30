# Codebase Dependency Tree

This diagram illustrates the primary dependencies and relationships between the major classes in the Roguelike MVP codebase.

- `-->`: Represents a one-way reference, composition, or method call.
- `<-->`: Represents a two-way (cyclical) dependency, often due to the singleton pattern.
- `--|>`: Represents inheritance (e.g., `Enemy` inherits from `BaseEnemy`).

```mermaid
classDiagram
    direction LR

    subgraph Core Systems
        GameManager
        RoomsManager
        ShopManager
        ModManager
        ShotManager
    end

    subgraph Player
        Player
        MovementManager
        CharacterAnimationController
        RagdollController
    end

    subgraph Enemy
        EnemySpawner
        EnemyInstance
        Enemy
        BaseEnemy
    end

    subgraph Combat
        Weapon
        Laser
        Hitbox
    end

    subgraph Items
        ModificationObject
        BuyBox
    end
    
    subgraph Data
        utility.Modification
        utility.StoreItem
        utility.GameHitEntity
        utility.Goal
    end

    %% --- Core Dependencies & Cycles ---
    GameManager <--> Player : instance
    GameManager <--> EnemySpawner : instance
    GameManager <--> ShopManager : instance
    GameManager <--> ModManager : instance
    GameManager --> utility.Goal
    GameManager ..> DamagePopup_UI
    GameManager ..> HealthBarManager_UI
    GameManager ..> GameUI_UI

    %% --- Player Dependencies ---
    Player --> Weapon
    Player --> MovementManager
    Player --> CharacterAnimationController
    Player --> RagdollController
    Player --> Hitbox
    Player --> utility.Modification
    Player ..> SettingsManager

    %% --- Enemy Dependencies ---
    EnemySpawner --> EnemyInstance
    EnemyInstance --> Enemy
    EnemyInstance --> utility.GameHitEntity
    EnemyInstance --> GameManager : instance
    Enemy --> BaseEnemy
    Enemy --> Weapon
    Enemy --> CharacterAnimationController
    Enemy --> RagdollController
    Enemy --> Hitbox
    Enemy ..> GameField

    %% --- Combat Dependencies ---
    Weapon --> Laser
    Weapon --> ModificationObject
    Weapon --> ShotManager
    Weapon --> ModManager : instance
    Laser --> utility.GameHitEntity
    Hitbox --> utility.GameHitEntity

    %% --- Shop & Item Dependencies ---
    ShopManager --> BuyBox
    ModManager --> BuyBox
    BuyBox --> utility.StoreItem
    BuyBox --> ModificationObject
    BuyBox --> Player
    utility.StoreItem --> utility.Modification
    ModificationObject --> utility.Modification

    %% --- Link to Player ---
    EnemyInstance --> Player : FocusTarget
    Enemy --> Player : Target

    class DamagePopup_UI {
      <<UI Component>>
      ShowDamagePopup()
    }
    class HealthBarManager_UI {
      <<UI Component>>
      AddHealthBar()
    }
    class GameUI_UI {
      <<UI Component>>
      UpdateKillCount()
      UpdateScore()
    }

```

### Explanatory Notes

- **UI Components (Dotted Lines):** Classes like `DamagePopup`, `HealthBarManager`, and `GameUI` are referenced by `GameManager` but are not part of the core script files provided. They are included to show the connection between the game logic and the user interface.
- **`utility` Namespace:** Classes within the `utility` namespace are foundational data structures used by many other classes but generally do not have dependencies of their own.
- **`GameField`:** This singleton provides level boundary information to any class that needs it, such as `Enemy` for movement constraints.
- **Singletons & Cycles:** `GameManager`, `ShopManager`, `ModManager`, `ShotManager`, and `GameField` are all singletons. The extensive use of the singleton pattern (`.instance`) creates cyclical dependencies, as many classes call back into the managers that control them. These are marked with a `<-->` arrow. For example, `GameManager` holds a reference to `Player`, while `Player` can call `GameManager.instance` at any time.