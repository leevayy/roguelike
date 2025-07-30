# Roguelike MVP Codebase Overview

This document provides a high-level summary of the Roguelike MVP codebase, a Unity-based top-down shooter game. The architecture is centered around a main `GameManager` that orchestrates the various systems to create the core game loop.

The player's objective is to complete a series of goals, such as defeating a specific number of enemies or accumulating a certain amount of score. Between goals, the player can purchase upgrades from a shop. The game's difficulty scales with each completed goal.

## Key Architectural Ideas

- **Singleton Managers:** Core systems like `GameManager`, `ShopManager`, and `ModManager` are implemented as singletons for easy global access.
- **Component-Based Design:** The project follows Unity's standard component-based architecture. GameObjects (like the Player and Enemies) are composed of multiple scripts that handle specific functionalities (e.g., `MovementManager`, `Weapon`, `CharacterAnimationController`).
- **Event-Driven Logic:** The `GameManager` acts as a central hub, responding to events like `OnHit`, `OnKill`, and `OnBuy` to manage game state, score, and player progression.
- **ScriptableObject-like Data Structures:** Although not using `ScriptableObject` assets directly, the code uses plain C# classes like `Modification`, `StoreItem`, and `Goal` to define and manage game data in a structured way.

## Core Functional Aspects

Here are the primary functional aspects identified within the codebase:

1.  **Game State & Level Progression:** Manages the overall game loop, player goals, difficulty scaling, and transitions between game states (e.g., fighting, shopping).
2.  **Player System:** Encapsulates all logic related to the player character, including input, movement, health, and camera control.
3.  **Enemy System:** Handles enemy behavior, AI, spawning, and scaling of stats based on game progression.
4.  **Combat System:** Governs the mechanics of shooting, projectiles, damage calculation, and hit detection for both player and enemies.
5.  **Modification & Item System:** Manages the system of passive upgrades (modifications) that alter player and weapon statistics.
6.  **Shop & Economy System:** Controls the in-game shop, allowing the player to buy items and modifications using score as currency.
7.  **Core Engine & Utilities:** A collection of foundational classes and helper functions used across the entire project, including data structures and managers.
