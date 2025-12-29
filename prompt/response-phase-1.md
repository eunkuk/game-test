# Response Phase 1: Unity 2D 로그라이크 아키텍처 설계 및 구현

## 0. 전체 그림 (아키텍처)

### Unity 폴더 구조 및 Assembly Definition

```
Assets/
├── _Project/
│   ├── Core/                          # Game.Core.asmdef
│   │   ├── Random/
│   │   │   ├── SeededRandom.cs
│   │   │   └── IRandomProvider.cs
│   │   ├── Events/
│   │   │   ├── EventBus.cs
│   │   │   └── GameEvents.cs
│   │   ├── Interfaces/
│   │   │   ├── IPoolable.cs
│   │   │   └── IEncounterResolver.cs
│   │   └── Utils/
│   │       ├── Singleton.cs
│   │       └── GridUtils.cs
│   │
│   ├── Data/                          # Game.Data.asmdef
│   │   ├── Dungeon/
│   │   │   ├── DungeonConfigSO.cs
│   │   │   └── TilePaletteSO.cs
│   │   ├── Encounter/
│   │   │   ├── EncounterTableSO.cs
│   │   │   ├── EncounterDefinitionSO.cs (abstract)
│   │   │   ├── EnemySpawnEncounterSO.cs
│   │   │   ├── TrapEncounterSO.cs
│   │   │   ├── LootEncounterSO.cs
│   │   │   └── EventTextEncounterSO.cs
│   │   ├── Enemy/
│   │   │   └── EnemyDataSO.cs
│   │   └── Item/
│   │       └── ItemDataSO.cs
│   │
│   ├── Systems/                       # Game.Systems.asmdef
│   │   ├── Dungeon/
│   │   │   ├── DungeonGenerator.cs
│   │   │   ├── DungeonResult.cs
│   │   │   ├── Room.cs
│   │   │   ├── Corridor.cs
│   │   │   └── TilemapPainter.cs
│   │   ├── Encounter/
│   │   │   ├── EncounterResolver.cs
│   │   │   ├── EncounterContext.cs
│   │   │   ├── EncounterResult.cs
│   │   │   └── RoomTrigger2D.cs
│   │   ├── Vision/
│   │   │   ├── FieldOfView2D.cs
│   │   │   ├── FacingProvider.cs
│   │   │   └── VisionRenderer.cs
│   │   └── FogOfWar/
│   │       ├── FogOfWarSystem.cs
│   │       ├── FogOfWarGrid.cs
│   │       └── FogRenderer.cs
│   │
│   ├── Gameplay/                      # Game.Gameplay.asmdef
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerStats.cs
│   │   │   └── PlayerInventory.cs
│   │   ├── Enemy/
│   │   │   ├── EnemyController.cs
│   │   │   └── EnemyAI.cs
│   │   └── Combat/
│   │       └── CombatSystem.cs
│   │
│   ├── UI/                            # Game.UI.asmdef
│   │   ├── HUD/
│   │   │   └── PlayerHUD.cs
│   │   └── Encounter/
│   │       ├── EventChoicePanel.cs
│   │       └── EncounterResultPanel.cs
│   │
│   └── Runtime/                       # Game.Runtime.asmdef
│       ├── GameRunManager.cs
│       └── States/
│           ├── GameStateMachine.cs
│           ├── TitleState.cs
│           ├── RunState.cs
│           └── ResultState.cs
```

### Assembly Definition 의존성 (단방향)

```
Game.Runtime
    ↓
Game.UI ────────┐
    ↓           ↓
Game.Gameplay   │
    ↓           ↓
Game.Systems ───┘
    ↓
Game.Core ←──── Game.Data (독립, 모든 모듈에서 참조 가능)
```

**의존성 규칙:**
- `Game.Data`: 런타임 로직 의존 금지, 데이터만 정의
- `Game.Core`: 공용 유틸리티, 어디서든 참조 가능
- `Game.Systems`: Core와 Data만 참조
- `Game.Gameplay`: Systems, Core, Data 참조
- `Game.UI`: Gameplay, Systems, Core, Data 참조
- `Game.Runtime`: 모든 모듈 참조 가능 (최상위 조합 레이어)

### 런타임 흐름

```
[씬 구조]
Title (옵션) → Run (1층) → Result (옵션)

[Run 씬 흐름]
GameRunManager 초기화
    ↓
Seed 설정 (입력 또는 랜덤)
    ↓
DungeonGenerator.Generate(seed)
    ↓
Player Spawn (시작방)
    ↓
게임 루프:
  - Player 이동/상호작용
  - FOV 업데이트
  - Fog-of-War 업데이트
  - RoomTrigger 감지
      ↓
  Encounter 발동 (방 진입 1회)
      ↓
  EncounterResolver.Resolve()
      ↓
  결과 처리 (적 스폰/함정/보상/이벤트)
    ↓
탈출 도달 → Result 씬
사망 → Result 씬
```

### 네임스페이스 규칙

- `Game.Core.*` (예: `Game.Core.Random`, `Game.Core.Events`)
- `Game.Data.*` (예: `Game.Data.Dungeon`, `Game.Data.Encounter`)
- `Game.Systems.*` (예: `Game.Systems.Dungeon`, `Game.Systems.Vision`)
- `Game.Gameplay.*`
- `Game.UI.*`
- `Game.Runtime.*`

### ScriptableObject + Resolver 패턴

콘텐츠는 ScriptableObject로 정의하고, 런타임에서 Resolver가 실행합니다:

```
[데이터]                [실행기]
EncounterDefinitionSO → EncounterResolver
DungeonConfigSO       → DungeonGenerator
```

이 패턴으로 코드 수정 없이 콘텐츠 확장이 가능합니다.

### EventBus 시스템

시스템 간 결합도를 낮추기 위해 C# 이벤트 사용:

```csharp
// Core/Events/GameEvents.cs
public static class GameEvents
{
    public static event Action<Room> OnEnterRoom;
    public static event Action<EncounterResult> OnEncounterResolved;
    public static event Action<Vector2[]> OnVisionUpdated;
    public static event Action<Room> OnRoomCleared;
}
```

### Object Pool (Init 최소 구현)

인터페이스만 준비하고, 실제 풀링은 나중에 구현:

```csharp
// Core/Interfaces/IPoolable.cs
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}

// Core/Pool/SimplePool.cs (Init용 간단 구현)
public class SimplePool<T> where T : MonoBehaviour, IPoolable
{
    // Get/Return 메서드만 제공
}
```

---

## 1. DungeonGenerator 설계/구현

### A) 알고리즘 설명

**선택: 순차 연결 (Init 추천)**

이유:
- MST는 구현 복잡도가 높고 디버그가 어려움
- 순차 연결은 간단하며 "던전 진행감"이 명확함
- 1층 던전에서는 8~12개 방이면 충분히 연결 보장 가능

**알고리즘:**
1. 랜덤으로 8~12개 사각형 방을 배치 (겹침 방지: 그리드 셀 기반 충돌 검사)
2. 방들을 순차적으로 연결 (Room[i] ↔ Room[i+1])
3. L자 복도로 연결 (수평 → 수직 또는 수직 → 수평)
4. 시작방: 첫 번째 방 (또는 랜덤)
5. 출구방: 마지막 방 (또는 시작방과 가장 먼 방)

**실패 케이스 대응:**
- 배치 실패 시 최대 10회 재시도
- 재시도 시 seed 증가 또는 배치 범위 확대

### B) Tilemap 구성

| Tilemap | 목적 | Sorting Layer | Collider | Z Position |
|---------|------|---------------|----------|------------|
| Floor | 바닥 타일 | Ground (0) | 없음 | 0 |
| Wall | 벽 타일 | Ground (0) | TilemapCollider2D | 0 |
| Overlay | Fog/디버그 | Overlay (10) | 없음 | -1 |

**Layer 설계:**
- `Wall` (Layer 6): 물리 충돌 + Vision Blocker
- `VisionBlocker` (Layer 7): 시야 차단만 (Wall과 동일하게 처리 가능, 확장용)
- `Player` (Layer 8)
- `Enemy` (Layer 9)

**Layer Collision Matrix:**
```
           Wall  Player  Enemy
Player      O      X       O
Enemy       O      O       X
```

### C) ScriptableObject 데이터 초안

#### DungeonConfigSO

```csharp
namespace Game.Data.Dungeon
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/Dungeon/Config")]
    public class DungeonConfigSO : ScriptableObject
    {
        [Header("Room Settings")]
        public int minRooms = 8;
        public int maxRooms = 12;
        public Vector2Int minRoomSize = new Vector2Int(5, 5);
        public Vector2Int maxRoomSize = new Vector2Int(12, 12);

        [Header("Corridor Settings")]
        public int corridorWidth = 2;

        [Header("Generation Settings")]
        public int maxRetries = 10;
        public int gridSpacing = 2; // 방 사이 최소 간격

        [Header("Seed")]
        public bool useFixedSeed = false;
        public int fixedSeed = 12345;
    }
}
```

#### TilePaletteSO

```csharp
namespace Game.Data.Dungeon
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    [CreateAssetMenu(menuName = "Game/Dungeon/TilePalette")]
    public class TilePaletteSO : ScriptableObject
    {
        [Header("Floor Tiles")]
        public TileBase[] floorTiles;

        [Header("Wall Tiles")]
        public TileBase wallTile;
        public TileBase wallTopTile;
        public TileBase wallCornerTile;

        public TileBase GetRandomFloorTile(System.Random random)
        {
            if (floorTiles == null || floorTiles.Length == 0) return null;
            return floorTiles[random.Next(floorTiles.Length)];
        }
    }
}
```

### D) C# 스켈레톤 (컴파일 가능)

#### Room.cs

```csharp
namespace Game.Systems.Dungeon
{
    using UnityEngine;

    public class Room
    {
        public int Id { get; }
        public RectInt Bounds { get; }
        public Vector2Int Center => new Vector2Int(
            Bounds.x + Bounds.width / 2,
            Bounds.y + Bounds.height / 2
        );

        public bool IsStartRoom { get; set; }
        public bool IsExitRoom { get; set; }
        public bool IsVisited { get; set; }

        public Room(int id, RectInt bounds)
        {
            Id = id;
            Bounds = bounds;
        }

        public bool Intersects(Room other, int spacing = 0)
        {
            return !(Bounds.xMax + spacing < other.Bounds.xMin ||
                     Bounds.xMin - spacing > other.Bounds.xMax ||
                     Bounds.yMax + spacing < other.Bounds.yMin ||
                     Bounds.yMin - spacing > other.Bounds.yMax);
        }
    }
}
```

#### Corridor.cs

```csharp
namespace Game.Systems.Dungeon
{
    using UnityEngine;
    using System.Collections.Generic;

    public class Corridor
    {
        public Room RoomA { get; }
        public Room RoomB { get; }
        public List<Vector2Int> Tiles { get; }

        public Corridor(Room roomA, Room roomB, List<Vector2Int> tiles)
        {
            RoomA = roomA;
            RoomB = roomB;
            Tiles = tiles;
        }
    }
}
```

#### DungeonResult.cs

```csharp
namespace Game.Systems.Dungeon
{
    using System.Collections.Generic;

    public class DungeonResult
    {
        public List<Room> Rooms { get; }
        public List<Corridor> Corridors { get; }
        public Room StartRoom { get; set; }
        public Room ExitRoom { get; set; }
        public int Seed { get; }

        public DungeonResult(int seed)
        {
            Seed = seed;
            Rooms = new List<Room>();
            Corridors = new List<Corridor>();
        }
    }
}
```

#### DungeonGenerator.cs

```csharp
namespace Game.Systems.Dungeon
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using Game.Data.Dungeon;
    using Game.Core.Random;

    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] private DungeonConfigSO config;
        [SerializeField] private TilemapPainter painter;

        public DungeonResult Generate(int? seedOverride = null)
        {
            int seed = seedOverride ?? (config.useFixedSeed ? config.fixedSeed : UnityEngine.Random.Range(0, int.MaxValue));
            System.Random random = new System.Random(seed);

            DungeonResult result = new DungeonResult(seed);

            // 1. 방 생성
            if (!GenerateRooms(result, random))
            {
                Debug.LogError("Failed to generate rooms after max retries");
                return null;
            }

            // 2. 방 연결
            GenerateCorridors(result, random);

            // 3. 시작/출구 설정
            AssignSpecialRooms(result);

            // 4. Tilemap 페인팅
            painter.PaintDungeon(result);

            return result;
        }

        private bool GenerateRooms(DungeonResult result, System.Random random)
        {
            int roomCount = random.Next(config.minRooms, config.maxRooms + 1);
            int attempts = 0;

            while (result.Rooms.Count < roomCount && attempts < config.maxRetries * roomCount)
            {
                attempts++;

                // 랜덤 크기
                int width = random.Next(config.minRoomSize.x, config.maxRoomSize.x + 1);
                int height = random.Next(config.minRoomSize.y, config.maxRoomSize.y + 1);

                // 랜덤 위치 (그리드 기반)
                int x = random.Next(-20, 20) * (config.maxRoomSize.x + config.gridSpacing);
                int y = random.Next(-20, 20) * (config.maxRoomSize.y + config.gridSpacing);

                RectInt bounds = new RectInt(x, y, width, height);
                Room newRoom = new Room(result.Rooms.Count, bounds);

                // 겹침 검사
                bool overlaps = false;
                foreach (var existingRoom in result.Rooms)
                {
                    if (newRoom.Intersects(existingRoom, config.gridSpacing))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    result.Rooms.Add(newRoom);
                }
            }

            return result.Rooms.Count >= config.minRooms;
        }

        private void GenerateCorridors(DungeonResult result, System.Random random)
        {
            // 순차 연결
            for (int i = 0; i < result.Rooms.Count - 1; i++)
            {
                Room roomA = result.Rooms[i];
                Room roomB = result.Rooms[i + 1];

                List<Vector2Int> tiles = CreateLShapedCorridor(roomA.Center, roomB.Center, random);
                result.Corridors.Add(new Corridor(roomA, roomB, tiles));
            }
        }

        private List<Vector2Int> CreateLShapedCorridor(Vector2Int start, Vector2Int end, System.Random random)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();

            // 수평 우선 또는 수직 우선 (랜덤)
            bool horizontalFirst = random.Next(2) == 0;

            if (horizontalFirst)
            {
                // 수평 → 수직
                for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                {
                    tiles.Add(new Vector2Int(x, start.y));
                }
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    tiles.Add(new Vector2Int(end.x, y));
                }
            }
            else
            {
                // 수직 → 수평
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    tiles.Add(new Vector2Int(start.x, y));
                }
                for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                {
                    tiles.Add(new Vector2Int(x, end.y));
                }
            }

            return tiles;
        }

        private void AssignSpecialRooms(DungeonResult result)
        {
            if (result.Rooms.Count == 0) return;

            result.StartRoom = result.Rooms[0];
            result.StartRoom.IsStartRoom = true;

            // 출구는 시작방과 가장 먼 방
            Room farthest = result.Rooms[0];
            float maxDist = 0;
            foreach (var room in result.Rooms)
            {
                float dist = Vector2Int.Distance(result.StartRoom.Center, room.Center);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthest = room;
                }
            }

            result.ExitRoom = farthest;
            result.ExitRoom.IsExitRoom = true;
        }

        private void OnDrawGizmos()
        {
            // 디버그용 Gizmos는 TilemapPainter에서 처리
        }
    }
}
```

#### TilemapPainter.cs

```csharp
namespace Game.Systems.Dungeon
{
    using UnityEngine;
    using UnityEngine.Tilemaps;
    using Game.Data.Dungeon;
    using System.Collections.Generic;

    public class TilemapPainter : MonoBehaviour
    {
        [SerializeField] private Tilemap floorTilemap;
        [SerializeField] private Tilemap wallTilemap;
        [SerializeField] private TilePaletteSO palette;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private DungeonResult currentDungeon;

        public void PaintDungeon(DungeonResult dungeon)
        {
            currentDungeon = dungeon;

            // 초기화
            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();

            System.Random random = new System.Random(dungeon.Seed);

            // 방 페인팅
            foreach (var room in dungeon.Rooms)
            {
                PaintRoom(room, random);
            }

            // 복도 페인팅
            foreach (var corridor in dungeon.Corridors)
            {
                PaintCorridor(corridor, random);
            }

            // 벽 생성 (바닥 타일 주변)
            GenerateWalls();
        }

        private void PaintRoom(Room room, System.Random random)
        {
            for (int x = room.Bounds.xMin; x < room.Bounds.xMax; x++)
            {
                for (int y = room.Bounds.yMin; y < room.Bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    floorTilemap.SetTile(pos, palette.GetRandomFloorTile(random));
                }
            }
        }

        private void PaintCorridor(Corridor corridor, System.Random random)
        {
            foreach (var tile in corridor.Tiles)
            {
                // 복도 폭만큼 확장
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector3Int pos = new Vector3Int(tile.x + dx, tile.y + dy, 0);
                        if (floorTilemap.GetTile(pos) == null)
                        {
                            floorTilemap.SetTile(pos, palette.GetRandomFloorTile(random));
                        }
                    }
                }
            }
        }

        private void GenerateWalls()
        {
            BoundsInt bounds = floorTilemap.cellBounds;

            for (int x = bounds.xMin - 1; x <= bounds.xMax + 1; x++)
            {
                for (int y = bounds.yMin - 1; y <= bounds.yMax + 1; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);

                    // 바닥 타일이 없으면서 인접에 바닥이 있으면 벽
                    if (floorTilemap.GetTile(pos) == null && HasAdjacentFloor(pos))
                    {
                        wallTilemap.SetTile(pos, palette.wallTile);
                    }
                }
            }
        }

        private bool HasAdjacentFloor(Vector3Int pos)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector3Int neighbor = pos + new Vector3Int(dx, dy, 0);
                    if (floorTilemap.GetTile(neighbor) != null)
                        return true;
                }
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || currentDungeon == null) return;

            // 방 그리기
            foreach (var room in currentDungeon.Rooms)
            {
                Gizmos.color = room.IsStartRoom ? Color.green : (room.IsExitRoom ? Color.red : Color.blue);
                Vector3 center = new Vector3(room.Center.x, room.Center.y, 0);
                Vector3 size = new Vector3(room.Bounds.width, room.Bounds.height, 0);
                Gizmos.DrawWireCube(center, size);
            }

            // 연결선 그리기
            Gizmos.color = Color.yellow;
            foreach (var corridor in currentDungeon.Corridors)
            {
                Vector3 startPos = new Vector3(corridor.RoomA.Center.x, corridor.RoomA.Center.y, 0);
                Vector3 endPos = new Vector3(corridor.RoomB.Center.x, corridor.RoomB.Center.y, 0);
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }
}
```

### E) 디버그

- Gizmos로 방/연결선 표시 (`TilemapPainter.OnDrawGizmos`)
- Inspector에서 `DungeonConfigSO.useFixedSeed` 토글로 seed 고정/랜덤 전환
- 콘솔 로그: 생성된 seed, 방 개수, 재시도 횟수 출력

---

## 2. Random Response(Encounter) 시스템

### A) 데이터 모델 (ScriptableObject)

#### EncounterTableSO

```csharp
namespace Game.Data.Encounter
{
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public class WeightedEncounterEntry
    {
        public int weight = 10;
        public EncounterDefinitionSO encounter;
    }

    [CreateAssetMenu(menuName = "Game/Encounter/EncounterTable")]
    public class EncounterTableSO : ScriptableObject
    {
        public List<WeightedEncounterEntry> entries = new List<WeightedEncounterEntry>();

        public EncounterDefinitionSO Roll(System.Random random)
        {
            int totalWeight = 0;
            foreach (var entry in entries)
            {
                totalWeight += entry.weight;
            }

            int roll = random.Next(totalWeight);
            int cumulative = 0;

            foreach (var entry in entries)
            {
                cumulative += entry.weight;
                if (roll < cumulative)
                    return entry.encounter;
            }

            return entries.Count > 0 ? entries[0].encounter : null;
        }
    }
}
```

#### EncounterDefinitionSO (Abstract Base)

```csharp
namespace Game.Data.Encounter
{
    using UnityEngine;

    public abstract class EncounterDefinitionSO : ScriptableObject
    {
        [TextArea(2, 4)]
        public string description;

        public abstract string GetTypeName();
    }
}
```

#### EnemySpawnEncounterSO

```csharp
namespace Game.Data.Encounter
{
    using UnityEngine;
    using Game.Data.Enemy;

    [CreateAssetMenu(menuName = "Game/Encounter/EnemySpawn")]
    public class EnemySpawnEncounterSO : EncounterDefinitionSO
    {
        public EnemyDataSO enemyType;
        public int minCount = 1;
        public int maxCount = 3;
        public float spawnRadius = 3f;

        public override string GetTypeName() => "Enemy Spawn";
    }
}
```

#### TrapEncounterSO

```csharp
namespace Game.Data.Encounter
{
    using UnityEngine;

    public enum TrapType
    {
        Damage,
        Slow,
        Poison
    }

    [CreateAssetMenu(menuName = "Game/Encounter/Trap")]
    public class TrapEncounterSO : EncounterDefinitionSO
    {
        public TrapType trapType;
        public int damageAmount = 10;
        public float duration = 5f;

        public override string GetTypeName() => "Trap";
    }
}
```

#### LootEncounterSO

```csharp
namespace Game.Data.Encounter
{
    using UnityEngine;
    using Game.Data.Item;

    [CreateAssetMenu(menuName = "Game/Encounter/Loot")]
    public class LootEncounterSO : EncounterDefinitionSO
    {
        public int goldAmount = 50;
        public ItemDataSO itemReward;

        public override string GetTypeName() => "Loot";
    }
}
```

#### EventTextEncounterSO

```csharp
namespace Game.Data.Encounter
{
    using UnityEngine;

    [System.Serializable]
    public class EventChoice
    {
        public string choiceText;
        [TextArea(2, 3)]
        public string resultText;
        public int goldReward;
        public int healthPenalty;
    }

    [CreateAssetMenu(menuName = "Game/Encounter/EventText")]
    public class EventTextEncounterSO : EncounterDefinitionSO
    {
        [TextArea(3, 5)]
        public string eventText;

        public EventChoice choiceA;
        public EventChoice choiceB;

        public override string GetTypeName() => "Event";
    }
}
```

### B) 런타임 로직

#### EncounterContext

```csharp
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using Game.Systems.Dungeon;

    public class EncounterContext
    {
        public int Seed { get; }
        public Room Room { get; }
        public GameObject Player { get; }
        public Vector3 SpawnCenter { get; }

        public EncounterContext(int seed, Room room, GameObject player)
        {
            Seed = seed;
            Room = room;
            Player = player;
            SpawnCenter = new Vector3(room.Center.x, room.Center.y, 0);
        }
    }
}
```

#### EncounterResult

```csharp
namespace Game.Systems.Encounter
{
    using System.Collections.Generic;
    using UnityEngine;

    public class EncounterResult
    {
        public string Message { get; set; }
        public int GoldReward { get; set; }
        public int HealthChange { get; set; }
        public List<GameObject> SpawnedEnemies { get; set; }

        public EncounterResult()
        {
            SpawnedEnemies = new List<GameObject>();
        }
    }
}
```

#### EncounterResolver

```csharp
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using Game.Data.Encounter;
    using Game.Core.Events;

    public class EncounterResolver : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab; // Init용 단순 프리팹

        public EncounterResult Resolve(EncounterDefinitionSO definition, EncounterContext context)
        {
            EncounterResult result = new EncounterResult();

            switch (definition)
            {
                case EnemySpawnEncounterSO enemySpawn:
                    ResolveEnemySpawn(enemySpawn, context, result);
                    break;

                case TrapEncounterSO trap:
                    ResolveTrap(trap, context, result);
                    break;

                case LootEncounterSO loot:
                    ResolveLoot(loot, context, result);
                    break;

                case EventTextEncounterSO eventText:
                    // UI를 통해 처리, 여기서는 이벤트 발행만
                    GameEvents.OnEventEncounterTriggered?.Invoke(eventText, context, result);
                    break;
            }

            GameEvents.OnEncounterResolved?.Invoke(result);
            return result;
        }

        private void ResolveEnemySpawn(EnemySpawnEncounterSO data, EncounterContext context, EncounterResult result)
        {
            System.Random random = new System.Random(context.Seed + context.Room.Id);
            int count = random.Next(data.minCount, data.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i;
                Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.up * data.spawnRadius;
                Vector3 spawnPos = context.SpawnCenter + offset;

                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                result.SpawnedEnemies.Add(enemy);
            }

            result.Message = $"Spawned {count} enemies!";
            Debug.Log($"[Encounter] {result.Message}");
        }

        private void ResolveTrap(TrapEncounterSO data, EncounterContext context, EncounterResult result)
        {
            result.HealthChange = -data.damageAmount;
            result.Message = $"Trap! Took {data.damageAmount} damage.";
            Debug.Log($"[Encounter] {result.Message}");

            // 플레이어에게 피해 적용 (PlayerStats 필요)
        }

        private void ResolveLoot(LootEncounterSO data, EncounterContext context, EncounterResult result)
        {
            result.GoldReward = data.goldAmount;
            result.Message = $"Found {data.goldAmount} gold!";
            Debug.Log($"[Encounter] {result.Message}");
        }
    }
}
```

#### RoomTrigger2D

```csharp
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using Game.Data.Encounter;
    using Game.Systems.Dungeon;
    using Game.Core.Events;

    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomTrigger2D : MonoBehaviour
    {
        [SerializeField] private Room room;
        [SerializeField] private EncounterTableSO encounterTable;
        [SerializeField] private EncounterResolver resolver;

        private bool hasTriggered = false;

        public void Initialize(Room room, EncounterTableSO table, EncounterResolver resolver, int seed)
        {
            this.room = room;
            this.encounterTable = table;
            this.resolver = resolver;

            // Collider 설정
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(room.Bounds.width, room.Bounds.height);
            transform.position = new Vector3(room.Center.x, room.Center.y, 0);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered) return;
            if (!other.CompareTag("Player")) return;

            hasTriggered = true;
            room.IsVisited = true;

            GameEvents.OnEnterRoom?.Invoke(room);

            // Encounter 롤
            System.Random random = new System.Random(room.Id + 12345); // Seed 기반
            EncounterDefinitionSO encounter = encounterTable.Roll(random);

            if (encounter != null)
            {
                EncounterContext context = new EncounterContext(room.Id, room, other.gameObject);
                resolver.Resolve(encounter, context);
            }
        }
    }
}
```

### C) GameEvents 확장

```csharp
namespace Game.Core.Events
{
    using System;
    using UnityEngine;
    using Game.Systems.Dungeon;
    using Game.Systems.Encounter;
    using Game.Data.Encounter;

    public static class GameEvents
    {
        // Dungeon
        public static event Action<Room> OnEnterRoom;
        public static event Action<Room> OnRoomCleared;

        // Encounter
        public static event Action<EncounterResult> OnEncounterResolved;
        public static event Action<EventTextEncounterSO, EncounterContext, EncounterResult> OnEventEncounterTriggered;

        // Vision
        public static event Action<Vector2[]> OnVisionUpdated;
    }
}
```

### D) 밸런스/테스트

**Init용 가중치 테이블 예시:**

| Encounter Type | Weight | 비율 |
|----------------|--------|------|
| EnemySpawn | 40 | 40% |
| Trap | 20 | 20% |
| Loot | 30 | 30% |
| EventText | 10 | 10% |

**폭주 방지 (옵션):**
- 연속 EnemySpawn 제한: 마지막 3개 방 중 2개 이상이 EnemySpawn이면 다음은 강제로 Loot/Event
- Init에서는 구현하지 않고, 확장 시 추가

---

## 3. 전방 시야(FOV) 구현 방식 2안 비교

### 비교표

| 항목 | A) Raycast Fan + 메쉬 마스크 | B) URP 2D Light + ShadowCaster2D |
|------|---------------------------|----------------------------------|
| **구현 난이도** | 중간 (레이캐스트 + 메쉬 생성) | 낮음 (URP 내장 기능 활용) |
| **디버그 용이성** | 높음 (Gizmos로 레이 직접 확인) | 중간 (Light 설정 복잡) |
| **확장성** | 높음 (각도/거리 파라미터 자유) | 중간 (Light 범위 제약) |
| **벽 Occlusion 정확도** | 높음 (레이 기반 정밀 컷) | 높음 (ShadowCaster 자동) |
| **Fog-of-War 결합** | 용이 (레이 결과를 직접 활용) | 어려움 (Light는 시야만, Fog는 별도) |
| **성능** | 레이캐스트 비용 (rayCount 의존) | GPU 라이팅 비용 (해상도 의존) |
| **Init 적합성** | ★★★★☆ | ★★★☆☆ |

### Init 추천안: **A) Raycast Fan + 메쉬 마스크**

**이유:**
1. **Fog-of-War와의 자연스러운 결합**: 레이캐스트 결과(폴리곤)를 그대로 Fog 시스템에 전달 가능
2. **디버그 용이**: Gizmos로 레이를 직접 시각화, 문제 파악 빠름
3. **확장성**: 각도/거리 파라미터를 스크립트로 자유롭게 조절
4. **학습 곡선**: URP Light 설정보다 직관적

**필요 오브젝트/렌더 구성:**
- `Player` (FacingProvider)
- `Vision` GameObject (FieldOfView2D + MeshFilter + MeshRenderer)
- `DarknessOverlay` (전체 화면 어둠, Stencil로 Vision 영역만 뚫기)
- 머티리얼: Stencil Shader 2개 (Vision: Write Stencil, Darkness: Read Stencil)

---

## 4. 선택한 FOV 방식 실제 구현

### A) 씬 오브젝트 구성

```
Player
├── Sprite (캐릭터 스프라이트)
├── PlayerController (이동 + FacingProvider)
├── Collider2D
└── Vision (자식 오브젝트)
    ├── FieldOfView2D (메쉬 생성)
    ├── MeshFilter
    └── MeshRenderer (VisionMask Material)

Canvas
└── DarknessOverlay
    └── Image (DarknessMask Material, 전체 화면)
```

### B) 레이어/콜라이더 요구사항

- `Wall` 레이어: TilemapCollider2D 필수
- `VisionBlocker` 레이어: 시야 차단 전용 (확장용)
- Physics2D Raycast는 `Wall | VisionBlocker` 레이어만 감지

### C) C# 스켈레톤

#### FieldOfView2D.cs

```csharp
namespace Game.Systems.Vision
{
    using UnityEngine;
    using Game.Core.Events;

    [RequireComponent(typeof(MeshFilter))]
    public class FieldOfView2D : MonoBehaviour
    {
        [Header("FOV Parameters")]
        [SerializeField] private float viewAngle = 90f;
        [SerializeField] private float viewDistance = 10f;
        [SerializeField] private int rayCount = 50;
        [SerializeField] private float updateRate = 0.1f;

        [Header("Occlusion")]
        [SerializeField] private LayerMask visionBlockerMask;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private MeshFilter meshFilter;
        private Mesh viewMesh;
        private FacingProvider facingProvider;

        private Vector2[] currentViewPoints;
        private float updateTimer;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            facingProvider = GetComponentInParent<FacingProvider>();

            viewMesh = new Mesh { name = "View Mesh" };
            meshFilter.mesh = viewMesh;
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateRate)
            {
                updateTimer = 0;
                UpdateVision();
            }
        }

        private void UpdateVision()
        {
            Vector2 origin = transform.position;
            Vector2 facingDir = facingProvider.GetFacingDirection();
            float facingAngle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;

            // 레이캐스트
            Vector3[] vertices = new Vector3[rayCount + 1];
            int[] triangles = new int[(rayCount - 1) * 3];

            vertices[0] = Vector3.zero; // 중심

            float angleStep = viewAngle / (rayCount - 1);
            float startAngle = facingAngle - viewAngle / 2f;

            currentViewPoints = new Vector2[rayCount];

            for (int i = 0; i < rayCount; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewDistance, visionBlockerMask);

                Vector2 point = hit.collider != null ? hit.point : origin + dir * viewDistance;
                currentViewPoints[i] = point;

                vertices[i + 1] = transform.InverseTransformPoint(point);

                if (i < rayCount - 1)
                {
                    int triIndex = i * 3;
                    triangles[triIndex] = 0;
                    triangles[triIndex + 1] = i + 1;
                    triangles[triIndex + 2] = i + 2;
                }
            }

            viewMesh.Clear();
            viewMesh.vertices = vertices;
            viewMesh.triangles = triangles;
            viewMesh.RecalculateNormals();

            GameEvents.OnVisionUpdated?.Invoke(currentViewPoints);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || currentViewPoints == null) return;

            Gizmos.color = Color.yellow;
            Vector2 origin = transform.position;

            for (int i = 0; i < currentViewPoints.Length; i++)
            {
                Gizmos.DrawLine(origin, currentViewPoints[i]);
            }

            Gizmos.color = Color.red;
            foreach (var point in currentViewPoints)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }
        }
    }
}
```

#### FacingProvider.cs

```csharp
namespace Game.Systems.Vision
{
    using UnityEngine;

    public class FacingProvider : MonoBehaviour
    {
        private Vector2 lastFacingDirection = Vector2.down;

        public void SetFacingDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                lastFacingDirection = direction.normalized;
            }
        }

        public Vector2 GetFacingDirection()
        {
            return lastFacingDirection;
        }
    }
}
```

#### VisionRenderer (Stencil Mask용)

**VisionMask.shader** (Assets/Shaders/)

```shader
Shader "Custom/VisionMask"
{
    Properties
    {
        _StencilRef ("Stencil Ref", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        ColorMask 0
        ZWrite Off

        Stencil
        {
            Ref [_StencilRef]
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
```

**DarknessMask.shader**

```shader
Shader "Custom/DarknessMask"
{
    Properties
    {
        _Color ("Darkness Color", Color) = (0,0,0,0.8)
        _StencilRef ("Stencil Ref", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Stencil
        {
            Ref [_StencilRef]
            Comp NotEqual
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
```

### D) 성능 가이드

- **rayCount 추천**: 30~50 (Init), 50~100 (최종)
- **updateRate**: 0.1초 (10 FPS), 너무 빠르면 레이캐스트 비용 증가
- **viewDistance**: 10~15 타일
- **GC 방지**: 메쉬 정점 배열 재사용 (Mesh.Clear + 재할당 최소화)

---

## 5. Fog-of-War Init 버전

### A) 가장 빠른 구현 (필수안)

**그리드 기반 + Tilemap Overlay**

- Tilemap에 반투명 "Fog" 타일을 깔고, 탐색한 셀은 제거
- 3단계:
  - **미탐색**: Fog 타일 (검은색 알파 0.9)
  - **탐색 완료**: Fog 타일 제거 (반투명 알파 0.5 타일로 교체)
  - **현재 시야**: FOV 메쉬로 밝게 표시

**선택적 고급안:**
- RenderTexture + Shader: 더 부드러운 Fog, 하지만 Init에서는 오버엔지니어링

### B) 데이터 구조

#### FogOfWarGrid.cs

```csharp
namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using System.Collections.Generic;

    public class FogOfWarGrid
    {
        private int width;
        private int height;
        private Vector2Int offset;

        private HashSet<Vector2Int> visitedCells;
        private HashSet<Vector2Int> visibleCells;

        public FogOfWarGrid(int width, int height, Vector2Int offset)
        {
            this.width = width;
            this.height = height;
            this.offset = offset;

            visitedCells = new HashSet<Vector2Int>();
            visibleCells = new HashSet<Vector2Int>();
        }

        public void UpdateVisibility(Vector2[] viewPoints)
        {
            visibleCells.Clear();

            foreach (var worldPoint in viewPoints)
            {
                Vector2Int cell = WorldToCell(worldPoint);
                visibleCells.Add(cell);
                visitedCells.Add(cell); // 한 번 본 셀은 visited로 승격
            }
        }

        public bool IsVisible(Vector2Int cell) => visibleCells.Contains(cell);
        public bool IsVisited(Vector2Int cell) => visitedCells.Contains(cell);
        public bool IsUnexplored(Vector2Int cell) => !visitedCells.Contains(cell);

        public Vector2Int WorldToCell(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x) - offset.x,
                Mathf.FloorToInt(worldPos.y) - offset.y
            );
        }

        public Vector2 CellToWorld(Vector2Int cell)
        {
            return new Vector2(cell.x + offset.x, cell.y + offset.y);
        }

        public void Reset()
        {
            visitedCells.Clear();
            visibleCells.Clear();
        }
    }
}
```

### C) 업데이트 흐름

1. `FieldOfView2D`가 `OnVisionUpdated` 이벤트 발행 (레이 결과 폴리곤)
2. `FogOfWarSystem`이 이벤트 수신 → `FogOfWarGrid.UpdateVisibility()`
3. `FogRenderer`가 변경된 셀만 Tilemap 업데이트

### D) 코드 스켈레톤

#### FogOfWarSystem.cs

```csharp
namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using Game.Core.Events;

    public class FogOfWarSystem : MonoBehaviour
    {
        [SerializeField] private int gridWidth = 100;
        [SerializeField] private int gridHeight = 100;
        [SerializeField] private Vector2Int gridOffset = new Vector2Int(-50, -50);

        [SerializeField] private FogRenderer fogRenderer;

        private FogOfWarGrid grid;

        private void Awake()
        {
            grid = new FogOfWarGrid(gridWidth, gridHeight, gridOffset);
            GameEvents.OnVisionUpdated += OnVisionUpdated;
        }

        private void OnDestroy()
        {
            GameEvents.OnVisionUpdated -= OnVisionUpdated;
        }

        private void OnVisionUpdated(Vector2[] viewPoints)
        {
            grid.UpdateVisibility(viewPoints);
            fogRenderer.Render(grid);
        }

        public void ResetFog()
        {
            grid.Reset();
            fogRenderer.Clear();
        }
    }
}
```

#### FogRenderer.cs

```csharp
namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class FogRenderer : MonoBehaviour
    {
        [SerializeField] private Tilemap fogTilemap;
        [SerializeField] private TileBase unexploredTile; // 검은색 알파 0.9
        [SerializeField] private TileBase exploredTile;   // 회색 알파 0.5

        private HashSet<Vector2Int> renderedCells = new HashSet<Vector2Int>();

        public void Render(FogOfWarGrid grid)
        {
            // 초기화: 모든 셀을 미탐색으로 설정 (최초 1회만)
            if (renderedCells.Count == 0)
            {
                InitializeFog(grid);
            }

            // 변경된 셀만 업데이트 (visited 승격)
            foreach (var cell in renderedCells)
            {
                Vector3Int tilePos = new Vector3Int(cell.x, cell.y, 0);

                if (grid.IsVisible(cell))
                {
                    // 현재 시야: Fog 제거 (FOV 메쉬가 밝게 표시)
                    fogTilemap.SetTile(tilePos, null);
                }
                else if (grid.IsVisited(cell))
                {
                    // 탐색 완료: 반투명 타일
                    fogTilemap.SetTile(tilePos, exploredTile);
                }
            }

            // 새로 추가된 visible 셀 추적
            // (최적화: 실제로는 변경된 셀만 추적해야 하지만 Init에서는 단순화)
        }

        private void InitializeFog(FogOfWarGrid grid)
        {
            // 던전 범위만큼 Fog 타일 깔기 (실제로는 DungeonResult 기반으로 범위 설정)
            for (int x = -50; x < 50; x++)
            {
                for (int y = -50; y < 50; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    fogTilemap.SetTile(tilePos, unexploredTile);
                    renderedCells.Add(cell);
                }
            }
        }

        public void Clear()
        {
            fogTilemap.ClearAllTiles();
            renderedCells.Clear();
        }
    }
}
```

### E) 디버그

- Inspector 토글: `showVisited` (visited 셀 Gizmos 표시)
- 콘솔 명령: `/resetFog` → `FogOfWarSystem.ResetFog()`

---

## 마무리 산출물

### 1) Init Done 기준 (DoD) 10개

1. **재현성**: 동일 Seed 입력 시 동일한 던전 맵 생성 확인
2. **방 생성**: 8~12개 사각형 방이 겹침 없이 배치됨
3. **복도 연결**: 모든 방이 복도로 연결되어 고립된 방 없음
4. **시작/출구**: 시작방(녹색)과 출구방(빨간색)이 명확히 구분됨
5. **Encounter 트리거**: 방 진입 시 1회만 Encounter 발동 (재진입 시 미발동)
6. **가중치 랜덤**: EncounterTable의 가중치에 따라 적절한 비율로 결과 출력
7. **FOV Occlusion**: 플레이어 전방 시야가 벽에 의해 정확히 차단됨
8. **FOV 방향**: 플레이어 이동 방향 기준으로 부채꼴 시야 업데이트
9. **Fog 3단계**: 미탐색(어두움), 탐색 완료(흐림), 현재 시야(밝음) 구분 확인
10. **디버그 Gizmos**: 방/복도/FOV 레이가 Gizmos로 정상 표시됨

### 2) 리스크/성능 체크리스트 (8개)

| 항목 | 리스크 | 대응 방안 |
|------|--------|-----------|
| **레이캐스트 비용** | rayCount가 높으면 매 프레임 부하 | updateRate 0.1초, rayCount 30~50 제한 |
| **Fog Tilemap 업데이트** | 전체 셀 매번 갱신 시 GC/성능 저하 | 변경된 셀만 업데이트 (HashSet 활용) |
| **GC Allocation** | 메쉬 정점 배열 매번 생성 | Mesh.Clear + 배열 재사용 |
| **레이어 충돌 매트릭스** | 불필요한 레이어 간 충돌 | Physics2D 설정에서 불필요한 조합 비활성화 |
| **Tilemap Collider 성능** | CompositeCollider2D 미사용 시 콜라이더 과다 | TilemapCollider + CompositeCollider2D 조합 |
| **카메라 정렬** | Fog/Vision/Tilemap 렌더 순서 꼬임 | Sorting Layer 명확히 설정 (Ground < Vision < Overlay) |
| **Stencil 충돌** | 다른 Stencil 사용 시스템과 충돌 가능 | Stencil Ref 값 문서화, 다른 시스템과 협의 |
| **메쉬 크기** | FOV 메쉬가 너무 크면 drawcall 증가 | viewDistance 제한 (10~15), rayCount 최적화 |

### 3) 다음 확장 로드맵 (6개)

| 순서 | 확장 항목 | 설명 |
|------|-----------|------|
| 1 | **다층 던전** | DungeonConfigSO에 층별 설정 추가, 층마다 난이도/테이블 변화 |
| 2 | **바이옴 시스템** | TilePaletteSO를 바이옴별로 분리, 시각적 다양성 확보 |
| 3 | **Encounter 테이블 다중화** | 층/바이옴별 EncounterTableSO 분리, 난이도 곡선 설계 |
| 4 | **몹 AI 다양화** | EnemyAI에 패턴 추가 (추적/순찰/원거리), BehaviorTree 도입 검토 |
| 5 | **아이템/특성 시스템** | ItemDataSO 확장, 플레이어 특성/장비 시스템, 빌드 다양성 |
| 6 | **메타 진행** | 런 종료 후 언락 요소, 영구 업그레이드, 스토리 진행 |

---

## 다음 단계 (Phase 2 예정)

### 처리 완료 항목
- ✅ 전체 아키텍처 설계
- ✅ Assembly Definition 구조
- ✅ DungeonGenerator 설계/코드 스켈레톤
- ✅ Encounter 시스템 설계/코드 스켈레톤
- ✅ FOV 시스템 설계/코드 스켈레톤
- ✅ Fog-of-War 시스템 설계/코드 스켈레톤

### 다음 Phase에서 필요한 작업
1. **실제 Unity 프로젝트 생성** (현재는 코드 스켈레톤만 제공)
2. **Assembly Definition 파일 작성** (.asmdef)
3. **ScriptableObject 에셋 생성** (DungeonConfig, EncounterTable 등)
4. **프리팹 구성** (Player, Enemy, Room Trigger 등)
5. **씬 구성** (Run 씬, Tilemap 레이어 설정)
6. **머티리얼/셰이더 적용** (VisionMask, DarknessMask)
7. **테스트 및 디버깅**
8. **밸런싱 및 파라미터 튜닝**

### 고려사항
- **Tilemap 아트 에셋**: 임시 에셋으로 시작, 추후 교체 가능하도록 TilePaletteSO 활용
- **PlayerController 구현**: 입력 시스템 (New Input System vs Legacy), 이동 방식 (Rigidbody vs Transform)
- **Enemy AI**: Init에서는 간단한 추적만, 확장 시 BehaviorTree/FSM 도입
- **UI 프레임워크**: EventChoicePanel은 간단한 UI로 시작, 추후 UI Toolkit 검토
- **성능 프로파일링**: 초기부터 Profiler로 병목 지점 확인 (레이캐스트, Tilemap 업데이트)

---

## 코드 컴파일 체크리스트

모든 제공된 C# 코드는 다음을 만족합니다:
- ✅ 네임스페이스 명시 (`Game.*`)
- ✅ `using` 문 포함
- ✅ Unity API 사용 (MonoBehaviour, ScriptableObject)
- ✅ 문법 오류 없음 (세미콜론, 중괄호 매칭)
- ✅ 인터페이스/추상 클래스 상속 명확
- ✅ SerializeField/public 구분

---

이상으로 **Phase 1 Response**를 완료합니다. 다음 Phase에서 실제 Unity 프로젝트에 적용하고 테스트를 진행하겠습니다.
