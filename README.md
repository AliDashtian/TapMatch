# Tap Match

A tap-match puzzle game built in Unity 6 as a programming assignment. Tap connected groups of same-color tiles to remove them — remaining tiles fall down and new ones fill from the top.

![Unity](https://img.shields.io/badge/Unity-6000.4-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)

## How to Play

1. Open the project in **Unity 6** (6000.4+)
2. Open `Assets/Scenes/SampleScene.unity`
3. Hit **Play**
4. Click/tap on a tile — all connected same-color tiles (horizontally/vertically adjacent) are removed
5. Tiles fall down to fill gaps, new random tiles spawn from the top
6. Minimum group size is 2 — single isolated tiles can't be removed

## Project Architecture

The project follows **MVP (Model-View-Presenter)** with strict layer separation:

```
Packages/com.rovio.tapmatch.gamelogic/    Pure C# game logic (no Unity dependency)
Assets/Scripts/Board/                      View layer (BoardView, BoardPresenter)
Assets/Scripts/DI/                         VContainer dependency injection
Assets/Scripts/Config/                     GameConfig ScriptableObject
Assets/Scripts/Input/                      Input handling
```

**Key design decisions:**
- Core game logic is an **embedded Unity package** with `noEngineReferences: true` — fully unit-testable without the Unity runtime
- **VContainer** for dependency injection
- **Addressables** for asset loading
- **Unity 6 Awaitable** for async animations
- **37 unit tests** covering all core logic with deterministic randomness

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed design decisions, trade-offs, and rationale.

## Configuration

All game parameters are configurable via the `GameConfig` ScriptableObject (`Assets/Config/GameConfig.asset`):

| Parameter | Default | Description |
|-----------|---------|-------------|
| Rows | 8 | Grid height (N) |
| Columns | 8 | Grid width (M) |
| Color Count | 5 | Number of distinct colors (P) |
| Cell Size | 1.1 | Tile spacing in world units |
| Fall Duration | 0.3s | Gravity animation speed |
| Remove Duration | 0.15s | Removal animation speed |

## Running Tests

**Window → General → Test Runner → EditMode → Run All**

Tests are pure NUnit (no Unity runtime required) and run in under a second.

## Requirements

- Unity 6 (6000.4.0f1 or later)
- Packages installed automatically via manifest: VContainer, Addressables, Input System
