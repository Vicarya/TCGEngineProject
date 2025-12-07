# GEMINI.md: TCG Engine Project Analysis

## 1. Project Overview

This is a general-purpose engine for creating Trading Card Games (TCGs) in Unity. The project is architecturally designed with a clear separation between a reusable core engine and specific game implementations. The primary example provided is for the game *Weiss Schwarz*.

The project's official documentation is located in the `docs` directory and serves as the primary source of truth for design and architecture.

## 2. Core Architecture: A Two-Layer System

The engine's architecture is built on a strict principle of **separation of concerns**, dividing the system into two main layers.

### Layer 1: The Core Engine (`Assets/Scripts/GameCore`)

-   **Namespace**: `TCG.Core`
-   **Responsibility**: To provide a set of abstract and universal concepts applicable to any TCG. This layer is completely agnostic of any specific game's rules, terminology, or mechanics.
-   **Key Components**:
    -   `GameBase`, `GameState`: Manages the fundamental game flow (turns, phases).
    -   `Card`, `Player`, `IZone`, `ZoneBase`: Represents the universal physical entities in a card game.
    -   `EventBus`: A system for event-driven communication, allowing components to interact without direct dependencies (loose coupling).
    -   `AbilityBase`, `IEffect`, `ICost`: Abstract representations of card abilities, effects, and their costs.

### Layer 2: Game-Specific Implementations (`Assets/Scripts/WeissSchwarz`)

-   **Namespace**: `TCG.Weiss`
-   **Responsibility**: To implement the concrete rules, logic, and components for a specific TCG by inheriting from the `GameCore`'s abstract classes and interfaces.
-   **Key Components**:
    -   `WeissGame`: Inherits from `GameBase` to manage the specific turn structure and rules of Weiss Schwarz.
    -   `WeissCard`, `WeissCardData`: Extends `CardBase<TData>` to include game-specific data like Power, Soul, and Traits.
    -   **Specific Zone Interfaces**: Defines roles for zones that are unique to the game (e.g., `IStageZone`, `IClockZone`).
    -   **Concrete Costs**: Implements the `ICost` interface for game-specific costs (e.g., `StockCost`, `DiscardHandCost`).

## 3. Key Design Patterns

The engine heavily relies on two powerful design patterns:

### Data-Driven Abilities

This is a cornerstone of the engine's design. Instead of hard-coding card abilities, they are defined as data.

1.  **`AbilityDefinition`**: A data class that describes an ability's type, trigger conditions, and effects (e.g., `OnPlay`, `LookTopAndPlace`).
2.  **`AbilityFactory`**: This factory reads an `AbilityDefinition` and dynamically constructs an `AbilityBase` object, which contains the actual game logic.

This pattern allows developers and designers to add or modify card abilities by simply editing data files, without needing to write new C# code, significantly improving extensibility.

### Event-Driven Architecture

The `EventBus` allows different parts of the game to communicate by raising and listening for events (e.g., `TurnStarted`, `CardPlayed`). This decouples components, making the system more modular and easier to maintain.

## 4. How to Run the Project

1.  Open the project in **Unity Editor `2022.3.12f1`** or a later version.
2.  Open the scene file located at `Assets/Scenes/SampleScene.unity`.
3.  Click the **Play** button at the top of the editor.

## 5. Development Guide & Current Status

### Documentation
The `docs` folder is the definitive source for understanding the project's architecture. Key documents include:
- `ProjectArchitecture.md`: Explains the technical stack and directory structure.
- `ProjectOverview.md`: Details the design philosophy and components.
- `TODO_ActionPlan.md`: Outlines the development roadmap and current tasks.

### Contribution Guidelines
When contributing, strictly adhere to the architectural principle of separating the `GameCore` from game-specific logic. Any logic, interface, or concept that is not universally applicable to *all* TCGs belongs in the game-specific layer, not the core.

### Current Status
According to `docs/TODO_ActionPlan.md`, the project is currently in **Milestone 3: Player Input Implementation**.
-   Core logic (M1, M1.5) and UI-logic connections (M2) are complete.
-   Current work focuses on implementing the `UIGamePlayerController` to allow players to interact with the game through the UI.
-   The next major phase (M4) will be to expand the `AbilityFactory` to support a wider range of card effects automatically.