# Tap Match — Architecture & Design Decisions

## Overview

A simple "tap match" puzzle game built in Unity 6 with clean architecture, dependency injection, and comprehensive unit tests. The player taps connected groups of same-color matchables to remove them; remaining tiles fall down and new ones fill from the top.

---

## Project Structure

```
TapMatch/
├── Assets/
│   ├── Scripts/                          # Unity runtime code (TapMatch.Runtime assembly)
│   │   ├── Board/                        # View layer (BoardView, BoardPresenter, MatchableView)
│   │   ├── Config/                       # GameConfig ScriptableObject code
│   │   ├── DI/                           # VContainer composition root
│   │   └── Input/                        # InputHandler
│   ├── Config/                           # GameConfig asset (.asset file)
│   ├── EditModeTests/                    # Integration tests with Unity context
│   ├── Prefabs/                          # Matchable prefab (Addressable)
│   └── Scenes/                           # Game scene
├── Packages/
│   └── com.rovio.tapmatch.gamelogic/     # Embedded package (pure C#)
│       ├── Runtime/                      # Core game logic
│       └── Tests/Editor/                 # Unit tests (NUnit, no Unity runtime)
```

## Assembly Architecture

```
TapMatch.GameLogic          (embedded package, noEngineReferences=true)
       ↑
TapMatch.GameLogic.Tests    (NUnit, Editor-only)

TapMatch.Runtime            (UnityEngine, VContainer, InputSystem, Addressables)
       ↑
TapMatch.Tests.EditMode     (Unity Test Runner, Editor-only)
```

**Why this separation?**
- `TapMatch.GameLogic` has `noEngineReferences: true`, meaning it compiles without any Unity dependency. This makes the core game rules **pure C#** — fast to compile, trivial to unit test, and portable.
- The Unity layer (`TapMatch.Runtime`) handles visuals, input, and DI wiring. It depends on GameLogic but not vice versa.
- **Trade-off**: This means core logic can't use Unity types like `Vector2Int` for grid positions — we use plain tuples instead. The benefit in testability outweighs this minor inconvenience.

---

## Design Patterns & Principles

### MVP (Model-View-Presenter)

The game follows a Model-View-Presenter pattern:

| Role | Class | Responsibility |
|------|-------|----------------|
| **Model** | `GridModel`, `GameController` | Game state and rules (pure C#) |
| **View** | `BoardView`, `MatchableView` | Visual representation, animations |
| **Presenter** | `BoardPresenter` | Orchestrates Model ↔ View communication |

**Why MVP over MVC?** The Presenter explicitly controls the View through an interface (`IBoardView`), making it possible to test the presentation logic without Unity. The View is passive — it only does what the Presenter tells it to do.

### Input Blocking — Single Source of Truth

Input blocking during animations is owned exclusively by `BoardPresenter` via `InputHandler.SetInputEnabled()`. The `GameController` is intentionally **stateless** — it's a pure logic orchestrator that validates input, computes results, and returns data. It does not track whether the game is "processing."

**Why?** Having two systems block input (a state machine in GameController AND a flag in InputHandler) creates redundancy and confusion about which is authoritative. By keeping GameController stateless, it stays purely testable and the UI concern (blocking input during animations) lives where it belongs — in the presenter layer.

### Dependency Injection (VContainer)

`GameLifetimeScope` is the **composition root** — the single place where the object graph is assembled:

```
GameConfig (ScriptableObject, serialized reference)
  → GridModel, MatchFinder, GridCollapser, GridFiller, SystemRandomProvider
    → GameController
  → BoardView, InputHandler, BoardPresenter (MonoBehaviour, scene references)
    → GameStartup (IStartable entry point)
```

**Why VContainer?** It's lightweight, Unity-native, and supports both constructor injection (for pure C# classes) and method injection via `[Inject]` (for MonoBehaviours). It also provides `IStartable` for clean async initialization without relying on Unity's `Start()` ordering.

**Key lesson learned**: VContainer always picks the constructor with the most parameters. If a class has `MyClass(int seed = null)`, VContainer will try to resolve `int` from the container and fail. Keep constructors simple or use explicit `WithParameter()` registration.

### Strategy Pattern — IRandomProvider

`IRandomProvider` abstracts random number generation:
- `SystemRandomProvider` — production implementation using `System.Random`
- `FakeRandomProvider` — test double returning deterministic sequences

**Why?** This enables fully deterministic unit tests. Without this, testing match logic would require either mocking frameworks or non-deterministic assertions. With injectable randomness, every test produces the exact same result every time.

### Embedded Package

Core game logic lives in `Packages/com.rovio.tapmatch.gamelogic/` as a Unity embedded package.

**Benefits:**
- Enforces the dependency boundary at the assembly level (Unity won't let Runtime code leak into GameLogic)
- Clear ownership and discoverability
- Can be extracted to a standalone package if needed
- Demonstrates familiarity with Unity's package system

**Trade-off:** Slightly more setup than a simple folder under Assets, but the architectural benefits are significant.

---

## Core Algorithm

### Match Finding (BFS Flood-Fill)

`MatchFinder.FindConnected()` uses breadth-first search:
1. Start from the tapped cell
2. Enqueue all unvisited neighbors (up, down, left, right) with the same color
3. Continue until the queue is empty
4. Return all visited cells

**Why BFS over DFS?** Both have the same O(N×M) complexity. BFS is iterative (no stack overflow risk on large grids) and produces results in a natural order (outward from tap point).

### Gravity Collapse

`GridCollapser.Collapse()` processes each column independently:
1. Scan from bottom to top
2. Compact non-empty cells downward
3. Empty cells bubble to the top
4. Return `FallMove` structs describing each movement

**Why return FallMove data?** The view layer needs to know what moved where to animate the falls. By returning structured data instead of callbacks, the logic remains pure and testable.

### Game Flow

```
Player taps tile
    → InputHandler converts screen pos to grid coordinates
    → BoardPresenter receives tap event, checks input is enabled
    → GameController.TryTap():
        1. Validate bounds and cell is not empty
        2. MatchFinder finds connected group (must be ≥ 2)
        3. Remove matched cells (set to EmptyCell)
        4. GridCollapser applies gravity
        5. GridFiller fills empty cells with random colors
        6. Return TapResult (removed, falls, spawned)
    → BoardPresenter disables input, drives animations sequentially:
        1. AnimateRemoval (scale down matched tiles)
        2. AnimateFalls (move existing tiles to new positions)
        3. AnimateSpawns (create new tiles above board, animate down)
    → BoardPresenter re-enables input
```

---

## Configuration

`GameConfig` (ScriptableObject) makes all game parameters inspector-tweakable:
- **Grid dimensions** (N rows × M columns)
- **Color count** (P distinct colors)
- **Visual colors** (Color array mapped by ID)
- **Cell size** (world units per tile)
- **Animation durations** (fall speed, removal speed)

Includes `OnValidate()` to warn if `matchableColors` array length doesn't match `colorCount`, catching misconfigurations early in the editor.

**Why ScriptableObject?** It's Unity's standard pattern for shared, editable configuration. It avoids hardcoded values, supports multiple configs (e.g., easy/hard), and is serialized in version control.

---

## Defensive Programming

Several defensive measures were added during a self-review pass:

- **Animation cancellation safety**: All animation loops (`AnimateScaleDownAndRelease`, `AnimateMoveTo`) use `try/finally` to guarantee cleanup. If a `CancellationToken` fires mid-animation (e.g., scene unload), tiles are still returned to the pool and positions still snap to their target. Without this, cancelled animations would leak GameObjects or leave tiles frozen mid-fall.

- **Null assertion in SpawnMatchable**: `Debug.Assert` checks that the Addressable prefab was loaded before spawning. Catches initialization ordering bugs with a clear message instead of a cryptic NullReferenceException.

- **Exception logging in async void**: `BoardPresenter.HandleTileTapped` is `async void` (required for event handlers). A general `catch (Exception)` with `Debug.LogException()` ensures unexpected errors surface in the Console instead of being silently swallowed. `OperationCanceledException` is caught separately as expected behavior.

---

## Testing Strategy

### Pure Logic Tests (37 tests, no Unity runtime)

All core classes are tested with plain NUnit in the embedded package's `Tests/Editor/` folder:

| Test Suite | Coverage |
|------------|----------|
| `GridModelTests` | Construction, bounds checking, indexer, clear |
| `MatchFinderTests` | BFS correctness, isolation, diagonals, L-shapes, assignment example |
| `GridCollapserTests` | Gravity, gaps, multi-column, move tracking |
| `GridFillerTests` | Fill empty cells, entire grid, color distribution |
| `GameControllerTests` | Initialization, tap lifecycle, validation, edge cases |

**Key testing decisions:**
- `FakeRandomProvider` enables deterministic tests without mocking frameworks
- `noEngineReferences: true` means tests run without the Unity runtime — they're fast and reliable
- The assignment's PDF example (4×5 grid with X/x pattern) is verified as an explicit test case

---

## Object Pooling

`BoardView` uses Unity's built-in `ObjectPool<MatchableView>` (from `UnityEngine.Pool`) to recycle matchable GameObjects instead of instantiating and destroying them on every tap.

**How it works:**
- Pool is created after the Addressable prefab is loaded, pre-sized to `Rows × Columns`
- `SpawnMatchable()` and `AnimateSpawnsAsync()` call `_pool.Get()` instead of `Instantiate()`
- `AnimateScaleDownAndRelease()` calls `_pool.Release()` instead of `Destroy()`
- `MatchableView.ResetForPool()` resets scale to `Vector3.one` and deactivates the GameObject
- `MatchableView.ActivateFromPool()` re-activates it when taken from the pool

**Why?** In a match game, tiles are constantly created and destroyed. Each `Instantiate()`/`Destroy()` cycle allocates and deallocates memory, which triggers garbage collection spikes — especially noticeable on mobile. Object pooling reuses existing GameObjects, reducing GC pressure and improving frame consistency.

**Trade-off:** Slightly more complex code (pool callbacks, reset logic), but the performance benefit is significant for a game that may run indefinitely.

---

## Technology Choices

| Technology | Purpose | Rationale |
|-----------|---------|-----------|
| **Unity 6** (6000.4) | Runtime | Assignment requirement, latest LTS |
| **VContainer** | DI Framework | Lightweight, Unity-native, supports MonoBehaviour injection |
| **Addressables** | Asset loading | Matchable prefab loaded via AssetReference — decouples prefab from scene |
| **Input System** | Input handling | Modern Unity input, supports both mouse and touch |
| **Unity Awaitable** | Async animations | Built-in Unity 6 feature, no third-party dependency (replaces UniTask) |
| **ObjectPool** | Object recycling | Unity's built-in pool reduces GC pressure from frequent spawn/destroy cycles |
| **Embedded Package** | Code organization | Enforces architecture boundaries at assembly level |

---

## AI Usage Disclosure

Claude Code and Gemini were used as a coding assistant throughout this project for:
- **Architecture planning**: Discussed layer separation, assembly structure, and design patterns
- **Unit test creation**: Generated comprehensive test suites with deterministic test doubles
- **Bug fixing**: Helped diagnose VContainer resolution issues (constructor parameter selection)

All code was reviewed, understood, and validated by me. The architectural decisions and design trade-offs reflect my engineering judgment. I can explain and defend every choice in detail during the interview.
