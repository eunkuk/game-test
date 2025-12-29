# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Labyrinth** is a Unity 2D top-down roguelike game implemented in two phases:
- **Phase 1**: Room-based dungeon with encounter triggers
- **Phase 2**: Maze-based generation with dynamic enemy patrols

The project uses a modular architecture with Assembly Definitions to maintain clear dependency boundaries.

## Unity Environment

- **Unity Version**: 2022.3 LTS or higher
- **Render Pipeline**: URP 2D Renderer
- **Required Packages**: TextMeshPro

## Development Commands

### Running the Game
1. Open the project in Unity Editor
2. Open the `Run` scene (Assets/_Project/Scenes/Run.unity)
3. Press Play button

### Testing Dungeon/Maze Generation
- **DungeonGenerator** (Phase 1): Enable `autoGenerate` in Inspector
- **MazeGenerator** (Phase 2): Enable `autoGenerate` in Inspector
- Use `useFixedSeed` to test reproducibility with the same seed

### Regenerating Systems
- To regenerate dungeon/maze at runtime: Call `Generate(seed)` method
- To reset Fog-of-War: Call `FogOfWarSystem.ResetFog()`

## Architecture

### Assembly Definition Hierarchy

The codebase is organized into distinct modules with **unidirectional dependencies**:

```
Game.Runtime (top-level orchestrator)
    ↓
Game.UI ────────┐
    ↓           ↓
Game.Gameplay   │
    ↓           ↓
Game.Systems ───┘
    ↓
Game.DataJson ──┐
    ↓           ↓
Game.Core ←─────┘
Game.Data (Phase 1 - ScriptableObject-based, independent)
```

**Critical Dependency Rules:**
- `Game.Core`: Common utilities, can be referenced by all modules
- `Game.Data`: Phase 1 ScriptableObject definitions, no runtime logic dependencies
- `Game.DataJson`: Phase 2 JSON DTOs/loaders/validators, no runtime logic dependencies
- `Game.Systems`: References Core and DataJson only
- `Game.Gameplay`: References Systems, DataJson, Core
- `Game.UI`: References Gameplay, Systems, DataJson, Core
- `Game.Runtime`: References all modules (composition layer)

### Key System Interactions

**Phase 1 Flow (Room-based):**
```
DungeonGenerator → DungeonResult → TilemapPainter
                                 ↓
                              RoomTrigger → EncounterResolver
                                 ↓
                           GameEvents.OnEncounterResolved
```

**Phase 2 Flow (Maze-based):**
```
MazeGenerator → MazeResult → MazeTilemapPainter
                          ↓
              EncounterDirector (SpawnPlanner + PatrolPlanner + Budget)
                          ↓
              EnemyFactory (JSON-based) → EnemyController → Patrol/Chase AI
```

**Vision & Fog-of-War:**
```
Phase 1: FieldOfView2D (Raycast) → OnVisionUpdated → FogOfWarSystem
Phase 2: FieldOfView2D (Shadowcasting) → OnVisionCellsUpdated → FogOfWarSystem
```

## Critical Implementation Details

### Seed-Based Reproducibility

All procedural generation uses `System.Random` with explicit seeds:
```csharp
int seed = seedOverride ?? (config.useFixedSeed ? config.fixedSeed : Random.Range(0, int.MaxValue));
System.Random random = new System.Random(seed);
```

**Always use the seeded `System.Random` instance**, not `UnityEngine.Random`, to ensure reproducibility.

### Maze Generation (Phase 2)

**Algorithm**: DFS Backtracking (Recursive Backtracker)
- Grid size must be **odd** (e.g., 41x41) - validated in `MazeConfig.Validate()`
- Uses 2-cell steps to ensure proper wall placement
- `deadEndRemovalRate` (0-1) controls loop generation

**Node Analysis:**
- **Junction**: 3+ connections (prime spawn locations)
- **Corner**: 2 connections at 90° angle
- **DeadEnd**: 1 connection only

### JSON Data Pipeline (Phase 2)

**Loading Path**: Use `StreamingAssets/GameData/` (NOT Resources folder)
- Allows post-build modification without recompilation
- Files: `monsters.json`, `encounters.json`, etc.

**Pipeline Flow:**
```
JsonDataLoader → DataValidator → EnemyRegistry → EnemyFactory
```

**Adding New Monsters:**
1. Edit `StreamingAssets/GameData/monsters.json`
2. Add enemy prefab to `Resources/Prefabs/Enemies/`
3. No code changes required - registry auto-updates

### Encounter Budget System (Phase 2)

**Purpose**: Prevent enemy spawn overwhelming (폭주 방지)

**Constraints:**
- `maxConcurrentEnemies`: Max active enemies at once (default: 5)
- `totalSpawnBudget`: Max total spawns per run (default: 30)
- `encounterCooldown`: Min time between spawns (default: 5s)
- `maxEnemiesInRadius`: Max enemies within radius of player (default: 2)

**Always check** `EncounterBudget.CanSpawn()` before spawning enemies.

### FOV Implementation Differences

**Phase 1 (Raycast Fan):**
- Uses `Physics2D.Raycast` in fan pattern
- Returns `Vector2[]` ray endpoints
- Event: `GameEvents.OnVisionUpdated(Vector2[])`

**Phase 2 (Shadowcasting):**
- Grid-based octant scanning
- Returns `HashSet<Vector2Int>` visible cells
- Event: `GameEvents.OnVisionCellsUpdated(HashSet<Vector2Int>)`
- Better for maze alignment, no missing cells

### Event System

**GameEvents** is a static event bus for decoupling:

```csharp
// Subscribe
void OnEnable() => GameEvents.OnMazeGenerated += HandleMazeGenerated;
void OnDisable() => GameEvents.OnMazeGenerated -= HandleMazeGenerated;

// Publish
GameEvents.TriggerMazeGenerated(mazeResult);
```

**Important**: Always unsubscribe in `OnDisable`/`OnDestroy` to prevent memory leaks.

## Common Pitfalls

### 1. Assembly Definition Violations
**Problem**: Circular dependencies between modules
**Solution**: Check the dependency hierarchy above - dependencies must flow downward only

### 2. Random Seed Contamination
**Problem**: Using `UnityEngine.Random` breaks reproducibility
**Solution**: Always use the `System.Random` instance passed to generation methods

### 3. Maze Grid Size
**Problem**: Even-sized grids (e.g., 40x40) break DFS algorithm
**Solution**: `MazeConfig.Validate()` auto-corrects to odd size

### 4. JSON Path Issues
**Problem**: JSON files not found at runtime
**Solution**: Files must be in `StreamingAssets/GameData/`, NOT `Resources/`

### 5. Fog-of-War Cell Gaps (Phase 2)
**Problem**: Using ray endpoints causes missing cells in corridors
**Solution**: Use `OnVisionCellsUpdated` with HashSet - fills all cells automatically

### 6. Spawn Point Clustering
**Problem**: Enemies spawn too close together
**Solution**: `SpawnPlanner.minDistanceBetweenSpawns` enforces minimum distance (default: 5 cells)

## Debug Tools

### Gizmos Visualization
- **DungeonGenerator**: Rooms (blue), Start (green), Exit (red), Corridors (yellow lines)
- **MazeGenerator**: Start (green), Exit (red), Junctions (yellow), Corners (cyan)
- **EncounterDirector**: Spawn points (magenta), Active spawns (filled), Patrol paths (white lines)
- **FieldOfView2D**: Visible cells (yellow transparent cubes)

### Debug Panel (Phase 2)
Enable via `DebugPanel` component:
- Spawn point visualization toggle
- Patrol path visualization toggle
- Current budget display (active enemies / total spawned)
- Force spawn button (testing)
- Seed display and regeneration

### Console Logging
Key systems log generation details:
- `[MazeGenerator]`: Seed, floor cells, node analysis
- `[EncounterDirector]`: Spawn points, patrol paths, budget usage
- `[JsonDataLoader]`: File paths, loaded counts
- `[DataValidator]`: Validation errors with details

## Performance Considerations

### Shadowcasting FOV
- Keep `viewRange` between 8-12 (larger = more cell iterations)
- Set `updateRate` to 0.1s (10 FPS update, not every frame)

### Fog-of-War Tilemap
- Use `Tilemap.SetTilesBlock()` for batch updates instead of individual `SetTile()`
- Only update changed cells - track with `HashSet` diff

### Maze Generation
- Limit grid size to 41x41 (default) or max 61x61
- `deadEndRemovalRate` affects generation time (higher = more iterations)

### JSON Loading
- Load during initialization, not during gameplay
- Use `DataValidator.Validate()` immediately after loading

### Enemy Patrol Pathfinding
- Cache patrol paths - don't recalculate every frame
- Use `updateRate` on patrol movement to reduce A* calls

## File Structure Reference

```
Assets/_Project/
├── Core/               # Game.Core - Utilities, Events, Interfaces
├── Data/               # Game.Data - Phase 1 ScriptableObjects
├── DataJson/           # Game.DataJson - Phase 2 JSON DTOs/Loaders/Registry
├── Systems/
│   ├── Dungeon/        # Phase 1 room-based generation
│   ├── Maze/           # Phase 2 maze generation (DFS)
│   ├── Encounter/      # RoomTrigger (P1) / EncounterDirector (P2)
│   ├── Vision/         # FOV (Raycast/Shadowcasting)
│   └── FogOfWar/       # 3-tier fog system
├── Gameplay/           # Player, Enemy, Combat
├── UI/                 # HUD, Panels
└── Runtime/            # GameRunManager, State Machine

StreamingAssets/GameData/
├── monsters.json       # Enemy definitions
├── encounters.json     # Encounter tables
└── maze_configs.json   # (optional) Maze configurations
```

## Phase Migration Notes

When working across Phase 1/Phase 2 systems:

**Coexistence**: Both phases can run simultaneously - systems are isolated by namespace and events

**Phase 1 → Phase 2 Migration:**
1. DungeonGenerator → MazeGenerator
2. RoomTrigger → EncounterDirector + CorridorTrigger
3. ScriptableObject enemies → JSON pipeline
4. Raycast FOV → Shadowcasting FOV (event signature changes)

**Event Compatibility:**
- Phase 1 events (`OnEnterRoom`, `OnRoomCleared`) still exist
- Phase 2 adds new events (`OnMazeGenerated`, `OnVisionCellsUpdated`, `OnEnemySpawned`)
- Both can be subscribed to simultaneously for hybrid systems

## Key Design Patterns

**ScriptableObject + Resolver (Phase 1):**
- Data in ScriptableObject (DungeonConfigSO, EncounterTableSO)
- Execution in MonoBehaviour (DungeonGenerator, EncounterResolver)

**JSON + Registry + Factory (Phase 2):**
- Data in JSON files (monsters.json)
- Runtime registry (EnemyRegistry singleton)
- Factory pattern (EnemyFactory creates instances)

**Event-Driven Architecture:**
- Static GameEvents class for system decoupling
- Trigger methods ensure null-safe invocation

**Budget Pattern (Encounter Control):**
- Centralized constraint checking
- Prevents system overwhelming
- Clear counter management (OnSpawn/OnDespawn)
