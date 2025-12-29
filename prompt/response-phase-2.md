# Response Phase 2: 미로 기반 로그라이크 전환

## 0. 전체 그림 (아키텍처)

### Phase 1 → Phase 2 주요 변경사항

| 항목 | Phase 1 | Phase 2 |
|------|---------|---------|
| **맵 구조** | 방(Room) + 복도(Corridor) | 미로(Maze) - 코리더/교차로/막다른길 |
| **조우 방식** | 방 진입 트리거 (RoomTrigger) | 길목 실제 마주침 (FOV/순찰/소리) |
| **몬스터 정의** | ScriptableObject (EnemyDataSO) | JSON (monsters.json) |
| **생성 알고리즘** | 순차 연결 (Room placement) | DFS 백트래킹 미로 생성 |
| **조우 보장** | 방마다 1회 보장 | 조우 가능성 (폭주 방지 budget) |

### Unity 폴더 구조 및 Assembly Definition (Phase 2)

```
Assets/
├── _Project/
│   ├── Core/                          # Game.Core.asmdef
│   │   ├── Random/
│   │   │   ├── SeededRandom.cs
│   │   │   └── IRandomProvider.cs
│   │   ├── Events/
│   │   │   ├── EventBus.cs
│   │   │   └── GameEvents.cs (확장)
│   │   ├── Interfaces/
│   │   │   ├── IPoolable.cs
│   │   │   └── IEncounterResolver.cs
│   │   └── Utils/
│   │       ├── GridUtils.cs
│   │       └── MazeUtils.cs (NEW)
│   │
│   ├── DataJson/                      # Game.DataJson.asmdef (NEW)
│   │   ├── Schema/
│   │   │   ├── EnemyDefinition.cs (DTO)
│   │   │   ├── EncounterTableData.cs
│   │   │   └── MazeConfigData.cs
│   │   ├── Loader/
│   │   │   ├── JsonDataLoader.cs
│   │   │   └── DataValidator.cs
│   │   └── Registry/
│   │       ├── EnemyRegistry.cs
│   │       └── EncounterRegistry.cs
│   │
│   ├── Systems/                       # Game.Systems.asmdef
│   │   ├── Maze/                      # Dungeon → Maze 전환
│   │   │   ├── MazeGenerator.cs
│   │   │   ├── MazeResult.cs
│   │   │   ├── MazeConfig.cs
│   │   │   ├── MazeTilemapPainter.cs
│   │   │   └── MazeNode.cs (교차로 분석)
│   │   ├── Encounter/
│   │   │   ├── EncounterDirector.cs (NEW)
│   │   │   ├── SpawnPlanner.cs (NEW)
│   │   │   ├── PatrolPlanner.cs (NEW)
│   │   │   ├── EncounterBudget.cs (NEW)
│   │   │   ├── CorridorTrigger.cs (NEW)
│   │   │   └── EncounterResolver.cs (유지)
│   │   ├── Vision/
│   │   │   ├── FieldOfView2D.cs (유지, 미로 최적화)
│   │   │   ├── FacingProvider.cs
│   │   │   └── VisionCellFiller.cs (NEW - 셀 채우기)
│   │   ├── FogOfWar/
│   │   │   ├── FogOfWarSystem.cs
│   │   │   ├── FogOfWarGrid.cs
│   │   │   └── FogRenderer.cs
│   │   └── Metrics/
│   │       ├── RunMetrics.cs (NEW)
│   │       └── ReplayRecorder.cs (NEW - 확장용)
│   │
│   ├── Gameplay/                      # Game.Gameplay.asmdef
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerStats.cs
│   │   │   └── PlayerInventory.cs
│   │   ├── Enemy/
│   │   │   ├── EnemyController.cs (NEW)
│   │   │   ├── EnemyAI.cs (NEW)
│   │   │   ├── EnemyPatrol.cs (NEW)
│   │   │   └── EnemyFactory.cs (NEW)
│   │   └── Combat/
│   │       └── CombatSystem.cs
│   │
│   ├── UI/                            # Game.UI.asmdef
│   │   ├── HUD/
│   │   │   ├── PlayerHUD.cs
│   │   │   └── DebugPanel.cs (NEW)
│   │   └── Encounter/
│   │       └── EventChoicePanel.cs
│   │
│   └── Runtime/                       # Game.Runtime.asmdef
│       ├── GameRunManager.cs (NEW)
│       └── States/
│           ├── GameStateMachine.cs
│           ├── TitleState.cs
│           ├── RunState.cs
│           └── ResultState.cs
│
└── StreamingAssets/                   # JSON 데이터
    └── GameData/
        ├── monsters.json (NEW)
        ├── encounters.json (NEW)
        └── maze_configs.json (NEW, 옵션)
```

### Assembly Definition 의존성 (Phase 2)

```
Game.Runtime
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
```

**의존성 규칙:**
- `Game.Core`: 공용 유틸리티, 어디서든 참조 가능
- `Game.DataJson`: JSON DTO/로더/검증만, 런타임 로직 의존 금지
- `Game.Systems`: Core와 DataJson 참조
- `Game.Gameplay`: Systems, DataJson, Core 참조
- `Game.UI`: Gameplay, Systems, DataJson, Core 참조
- `Game.Runtime`: 모든 모듈 참조 가능 (최상위 조합 레이어)

### 런타임 흐름 (Phase 2)

```
[씬 구조]
Title (옵션) → Run → Result (옵션)

[Run 씬 흐름]
1. GameRunManager 초기화
    ↓
2. JSON 데이터 로드 (monsters.json, encounters.json)
   → EnemyRegistry, EncounterRegistry 생성
    ↓
3. Seed 설정 (입력 또는 랜덤)
    ↓
4. MazeGenerator.Generate(seed)
   → MazeResult 반환
    ↓
5. MazeTilemapPainter.Paint(MazeResult)
   → Tilemap 페인팅
    ↓
6. Player.Spawn(mazeResult.start)
    ↓
7. EncounterDirector.Initialize(mazeResult, seed)
   → SpawnPlanner: 교차로/코너에 스폰 포인트 배치
   → PatrolPlanner: 순찰 경로 생성
   → CorridorTrigger: Trap/Loot/Event 배치
    ↓
8. 게임 루프:
   - Player 이동/상호작용
   - FOV 업데이트 → VisionCellFiller로 셀 채우기
   - Fog-of-War 업데이트
   - Enemy 순찰/추적 (FOV 내 플레이어 감지 시)
   - CorridorTrigger 감지 (1회 이벤트)
   - EncounterBudget 관리 (폭주 방지)
    ↓
9. 탈출 도달 → Result 씬
   사망 → Result 씬
    ↓
10. RunMetrics 기록 (탐색률, 조우 수, 시간 등)
```

### EventBus 확장 (Phase 2)

```csharp
// Core/Events/GameEvents.cs (확장)
public static class GameEvents
{
    // Maze
    public static event Action<MazeResult> OnMazeGenerated;

    // Encounter (Phase 2)
    public static event Action<Vector2Int, EnemyDefinition> OnEnemySpawned;
    public static event Action<GameObject> OnEnemyDetectedPlayer;
    public static event Action<CorridorTrigger> OnCorridorTriggerActivated;
    public static event Action<EncounterBudget> OnBudgetChanged;

    // Vision (확장)
    public static event Action<HashSet<Vector2Int>> OnVisionCellsUpdated; // 셀 단위

    // Fog (확장)
    public static event Action<HashSet<Vector2Int>> OnFogCellsRevealed;

    // Metrics
    public static event Action<RunMetrics> OnRunComplete;

    // 기존 Phase 1 이벤트도 유지
    public static event Action<Vector2[]> OnVisionUpdated;
    public static event Action<EncounterResult> OnEncounterResolved;
    // ...
}
```

---

## 1. MazeGenerator 설계/구현

### A) 알고리즘 설명

**선택: DFS 백트래킹 미로 생성 (Recursive Backtracker)**

**이유:**
- 구현 간단, 디버그 용이
- 긴 복도와 적은 분기점 → 긴장감 있는 미로
- Seed 기반 재현성 완벽 보장
- deadEndRemoval로 루프량 조절 가능

**다른 알고리즘과 비교:**

| 알고리즘 | 장점 | 단점 | Init 적합성 |
|---------|------|------|------------|
| **DFS 백트래킹** | 구현 간단, 긴 복도, Seed 재현성 | 분기 적음, 예측 가능 | ★★★★★ |
| Prim's MST | 짧은 복도, 많은 분기 | 구현 복잡, 미로 밀도 높음 | ★★★☆☆ |
| Kruskal's MST | 루프 많음, 개방감 | 미로 느낌 약함 | ★★☆☆☆ |
| Binary Tree | 초고속, 초간단 | 패턴 명확, 편향 심함 | ★★★☆☆ |

**DFS 백트래킹 알고리즘 단계:**

1. 홀수 크기 그리드 생성 (예: 41x41)
2. 모든 셀을 벽으로 초기화
3. 시작점(start) 선택 → Stack에 push
4. Stack이 빌 때까지:
   - 현재 셀을 바닥으로 표시
   - 방문하지 않은 인접 셀 중 랜덤 선택
   - 선택된 셀과 현재 셀 사이의 벽 제거
   - 선택된 셀을 Stack에 push
   - 인접 셀이 없으면 Stack에서 pop (backtrack)
5. 출구(exit) 설정: 시작점과 가장 먼 바닥 셀
6. (옵션) deadEndRemoval: 막다른 길 X% 제거 → 루프 생성

**deadEndRemoval 파라미터:**
- 0%: 순수 미로 (분기 적음, 긴 복도)
- 50%: 균형 (일부 루프)
- 100%: 모든 막다른 길 제거 (순환 미로)

### B) MazeConfig 클래스 (ScriptableObject 대체)

```csharp
namespace Game.Systems.Maze
{
    using System;

    [Serializable]
    public class MazeConfig
    {
        [Header("Size")]
        public int width = 41;
        public int height = 41;

        [Header("Algorithm")]
        [Range(0f, 1f)]
        public float deadEndRemovalRate = 0.3f;

        [Header("Seed")]
        public bool useFixedSeed = false;
        public int fixedSeed = 12345;

        [Header("Spawn Points")]
        public int minEnemySpawnPoints = 5;
        public int maxEnemySpawnPoints = 15;
        public int minEventPoints = 3;
        public int maxEventPoints = 8;

        public void Validate()
        {
            if (width % 2 == 0) width++;
            if (height % 2 == 0) height++;
            if (width < 11) width = 11;
            if (height < 11) height = 11;
        }
    }
}
```

### C) MazeResult 구조

```csharp
namespace Game.Systems.Maze
{
    using UnityEngine;
    using System.Collections.Generic;

    public class MazeResult
    {
        public int Seed { get; }
        public int Width { get; }
        public int Height { get; }

        public HashSet<Vector2Int> FloorCells { get; }
        public HashSet<Vector2Int> WallCells { get; }

        public Vector2Int Start { get; set; }
        public Vector2Int Exit { get; set; }

        public List<MazeNode> Junctions { get; } // 교차로 (3방향 이상)
        public List<MazeNode> Corners { get; }   // 코너 (2방향, 직각)
        public List<MazeNode> DeadEnds { get; }  // 막다른 길

        public RectInt Bounds { get; }

        public MazeResult(int seed, int width, int height)
        {
            Seed = seed;
            Width = width;
            Height = height;

            FloorCells = new HashSet<Vector2Int>();
            WallCells = new HashSet<Vector2Int>();

            Junctions = new List<MazeNode>();
            Corners = new List<MazeNode>();
            DeadEnds = new List<MazeNode>();

            Bounds = new RectInt(0, 0, width, height);
        }

        public bool IsFloor(Vector2Int cell) => FloorCells.Contains(cell);
        public bool IsWall(Vector2Int cell) => WallCells.Contains(cell);
        public bool InBounds(Vector2Int cell) => Bounds.Contains(cell);

        public override string ToString()
        {
            return $"Maze (Seed: {Seed}, Size: {Width}x{Height}, " +
                   $"Floor: {FloorCells.Count}, Junctions: {Junctions.Count}, " +
                   $"Corners: {Corners.Count}, DeadEnds: {DeadEnds.Count})";
        }
    }
}
```

### D) C# 스켈레톤

#### MazeNode.cs

```csharp
namespace Game.Systems.Maze
{
    using UnityEngine;

    public enum MazeNodeType
    {
        Junction,   // 교차로 (3방향 이상)
        Corner,     // 코너 (2방향, 직각)
        DeadEnd,    // 막다른 길
        Corridor    // 일반 복도
    }

    public class MazeNode
    {
        public Vector2Int Position { get; }
        public MazeNodeType Type { get; }
        public int ConnectionCount { get; } // 연결된 방향 수

        public MazeNode(Vector2Int position, MazeNodeType type, int connectionCount)
        {
            Position = position;
            Type = type;
            ConnectionCount = connectionCount;
        }

        public override string ToString()
        {
            return $"{Type} at {Position} ({ConnectionCount} connections)";
        }
    }
}
```

#### MazeGenerator.cs

```csharp
namespace Game.Systems.Maze
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Core.Events;

    public class MazeGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MazeConfig config = new MazeConfig();

        [Header("References")]
        [SerializeField] private MazeTilemapPainter painter;

        [Header("Runtime")]
        [SerializeField] private bool autoGenerate = false;

        private MazeResult currentMaze;

        private void Start()
        {
            if (autoGenerate)
            {
                Generate(null);
            }
        }

        public MazeResult Generate(int? seedOverride = null)
        {
            config.Validate();

            int seed = seedOverride ?? (config.useFixedSeed ? config.fixedSeed : Random.Range(0, int.MaxValue));
            System.Random random = new System.Random(seed);

            Debug.Log($"[MazeGenerator] Generating maze with seed: {seed}");

            MazeResult result = new MazeResult(seed, config.width, config.height);

            // 1. DFS 백트래킹
            GenerateMazeDFS(result, random);

            // 2. 막다른 길 제거 (옵션)
            if (config.deadEndRemovalRate > 0)
            {
                RemoveDeadEnds(result, random);
            }

            // 3. 시작/출구 설정
            AssignStartExit(result);

            // 4. 노드 분석 (교차로/코너/막다른 길)
            AnalyzeNodes(result);

            // 5. Tilemap 페인팅
            if (painter != null)
            {
                painter.PaintMaze(result);
            }

            currentMaze = result;
            GameEvents.TriggerMazeGenerated(result);

            Debug.Log($"[MazeGenerator] {result}");
            return result;
        }

        private void GenerateMazeDFS(MazeResult result, System.Random random)
        {
            // 모든 셀을 벽으로 초기화
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    result.WallCells.Add(new Vector2Int(x, y));
                }
            }

            // DFS 시작점 (중앙 또는 랜덤)
            Vector2Int start = new Vector2Int(
                random.Next(result.Width / 2) * 2 + 1,
                random.Next(result.Height / 2) * 2 + 1
            );

            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(start);

            Vector2Int[] directions = {
                Vector2Int.up * 2,
                Vector2Int.down * 2,
                Vector2Int.left * 2,
                Vector2Int.right * 2
            };

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();

                // 현재 셀을 바닥으로
                MakeCellFloor(result, current);

                // 방문하지 않은 인접 셀 찾기
                List<Vector2Int> neighbors = new List<Vector2Int>();
                foreach (var dir in directions)
                {
                    Vector2Int next = current + dir;
                    if (result.InBounds(next) && result.IsWall(next))
                    {
                        neighbors.Add(next);
                    }
                }

                if (neighbors.Count > 0)
                {
                    // 랜덤 인접 셀 선택
                    Vector2Int chosen = neighbors[random.Next(neighbors.Count)];

                    // 중간 벽 제거
                    Vector2Int between = current + (chosen - current) / 2;
                    MakeCellFloor(result, between);
                    MakeCellFloor(result, chosen);

                    stack.Push(chosen);
                }
                else
                {
                    // 백트래킹
                    stack.Pop();
                }
            }
        }

        private void MakeCellFloor(MazeResult result, Vector2Int cell)
        {
            if (result.WallCells.Remove(cell))
            {
                result.FloorCells.Add(cell);
            }
        }

        private void RemoveDeadEnds(MazeResult result, System.Random random)
        {
            List<Vector2Int> deadEnds = FindDeadEnds(result);
            int removeCount = Mathf.FloorToInt(deadEnds.Count * config.deadEndRemovalRate);

            for (int i = 0; i < removeCount && deadEnds.Count > 0; i++)
            {
                int index = random.Next(deadEnds.Count);
                Vector2Int deadEnd = deadEnds[index];
                deadEnds.RemoveAt(index);

                // 막다른 길에서 랜덤 방향으로 벽 뚫기
                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = deadEnd + dir;
                    if (result.InBounds(neighbor) && result.IsWall(neighbor))
                    {
                        MakeCellFloor(result, neighbor);
                        break;
                    }
                }
            }
        }

        private List<Vector2Int> FindDeadEnds(MazeResult result)
        {
            List<Vector2Int> deadEnds = new List<Vector2Int>();

            foreach (var cell in result.FloorCells)
            {
                if (GetConnectionCount(result, cell) == 1)
                {
                    deadEnds.Add(cell);
                }
            }

            return deadEnds;
        }

        private int GetConnectionCount(MazeResult result, Vector2Int cell)
        {
            int count = 0;
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                Vector2Int neighbor = cell + dir;
                if (result.IsFloor(neighbor))
                {
                    count++;
                }
            }

            return count;
        }

        private void AssignStartExit(MazeResult result)
        {
            // 시작점: FloorCells 중 첫 번째 (또는 좌상단에서 가장 가까운 바닥)
            result.Start = FindNearestFloorTo(result, new Vector2Int(0, 0));

            // 출구: 시작점과 가장 먼 바닥 셀
            result.Exit = FindFarthestFloorFrom(result, result.Start);

            Debug.Log($"[MazeGenerator] Start: {result.Start}, Exit: {result.Exit}");
        }

        private Vector2Int FindNearestFloorTo(MazeResult result, Vector2Int target)
        {
            Vector2Int nearest = result.Start;
            float minDist = float.MaxValue;

            foreach (var cell in result.FloorCells)
            {
                float dist = Vector2Int.Distance(cell, target);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = cell;
                }
            }

            return nearest;
        }

        private Vector2Int FindFarthestFloorFrom(MazeResult result, Vector2Int start)
        {
            Vector2Int farthest = start;
            float maxDist = 0;

            foreach (var cell in result.FloorCells)
            {
                float dist = Vector2Int.Distance(cell, start);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthest = cell;
                }
            }

            return farthest;
        }

        private void AnalyzeNodes(MazeResult result)
        {
            foreach (var cell in result.FloorCells)
            {
                int connections = GetConnectionCount(result, cell);
                MazeNodeType type;

                if (connections >= 3)
                {
                    type = MazeNodeType.Junction;
                    result.Junctions.Add(new MazeNode(cell, type, connections));
                }
                else if (connections == 2 && IsCorner(result, cell))
                {
                    type = MazeNodeType.Corner;
                    result.Corners.Add(new MazeNode(cell, type, connections));
                }
                else if (connections == 1)
                {
                    type = MazeNodeType.DeadEnd;
                    result.DeadEnds.Add(new MazeNode(cell, type, connections));
                }
            }

            Debug.Log($"[MazeGenerator] Analyzed nodes: {result.Junctions.Count} junctions, " +
                      $"{result.Corners.Count} corners, {result.DeadEnds.Count} dead ends");
        }

        private bool IsCorner(MazeResult result, Vector2Int cell)
        {
            bool up = result.IsFloor(cell + Vector2Int.up);
            bool down = result.IsFloor(cell + Vector2Int.down);
            bool left = result.IsFloor(cell + Vector2Int.left);
            bool right = result.IsFloor(cell + Vector2Int.right);

            return (up && left) || (up && right) || (down && left) || (down && right);
        }

        public MazeResult GetCurrentMaze() => currentMaze;

        private void OnDrawGizmos()
        {
            if (currentMaze == null) return;

            // 시작점 (녹색)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(currentMaze.Start.x, currentMaze.Start.y, 0), 0.5f);

            // 출구 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(currentMaze.Exit.x, currentMaze.Exit.y, 0), 0.5f);

            // 교차로 (노란색)
            Gizmos.color = Color.yellow;
            foreach (var junction in currentMaze.Junctions)
            {
                Gizmos.DrawSphere(new Vector3(junction.Position.x, junction.Position.y, 0), 0.3f);
            }

            // 코너 (시안)
            Gizmos.color = Color.cyan;
            foreach (var corner in currentMaze.Corners)
            {
                Gizmos.DrawWireSphere(new Vector3(corner.Position.x, corner.Position.y, 0), 0.2f);
            }
        }
    }
}
```

#### MazeTilemapPainter.cs

```csharp
namespace Game.Systems.Maze
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class MazeTilemapPainter : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap floorTilemap;
        [SerializeField] private Tilemap wallTilemap;

        [Header("Tiles")]
        [SerializeField] private TileBase floorTile;
        [SerializeField] private TileBase wallTile;

        private MazeResult currentMaze;

        public void PaintMaze(MazeResult maze)
        {
            if (floorTilemap == null || wallTilemap == null)
            {
                Debug.LogError("[MazeTilemapPainter] Tilemaps not assigned!");
                return;
            }

            currentMaze = maze;

            // 초기화
            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();

            Debug.Log("[MazeTilemapPainter] Painting maze...");

            // 바닥 타일
            foreach (var cell in maze.FloorCells)
            {
                Vector3Int pos = new Vector3Int(cell.x, cell.y, 0);
                floorTilemap.SetTile(pos, floorTile);
            }

            // 벽 타일
            foreach (var cell in maze.WallCells)
            {
                Vector3Int pos = new Vector3Int(cell.x, cell.y, 0);

                // 바닥 인접한 벽만 표시 (최적화)
                if (HasAdjacentFloor(maze, cell))
                {
                    wallTilemap.SetTile(pos, wallTile);
                }
            }

            Debug.Log("[MazeTilemapPainter] Painting complete");
        }

        private bool HasAdjacentFloor(MazeResult maze, Vector2Int cell)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                if (maze.IsFloor(cell + dir))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
```

### E) 디버그

- **Gizmos**: OnDrawGizmos로 시작점(녹색), 출구(빨간색), 교차로(노란색), 코너(시안) 표시
- **Seed 고정**: MazeConfig.useFixedSeed 토글
- **콘솔 로그**: Seed, 바닥 셀 수, 노드 분석 결과

---

## 2. EncounterDirector 설계/구현 (길목 조우 시스템)

### A) EncounterDirector 구성요소

```
EncounterDirector
├── SpawnPlanner        # 스폰 포인트 계획 (교차로/코너/막다른 길)
├── PatrolPlanner       # 순찰 경로 생성 (2~4 노드 루프)
├── EncounterBudget     # 폭주 방지 (동시 적 수, 총량, 쿨다운)
└── CorridorTrigger[]   # 이벤트 트리거 (Trap/Loot/EventText)
```

**RoomTrigger 금지 이유:**
- 미로에는 "방"이 없음
- 조우는 실제로 보이거나 들리거나 순찰로 마주쳐야 발생
- 폭주 방지를 위한 Budget 시스템 필요

**SpawnPlanner 역할:**
- 미로 노드(교차로/코너) 분석
- 우선순위: 교차로 > 코너 > 막다른 길 (시작/출구 제외)
- Budget 범위 내에서 스폰 포인트 배치
- 최소 거리 보장 (스폰 포인트 간 5칸 이상)

**PatrolPlanner 역할:**
- 스폰 포인트마다 순찰 경로 생성
- 2~4개 노드 루프 (교차로/코너 연결)
- A* 경로 탐색으로 이동 가능 여부 확인
- 순찰 속도/대기 시간 파라미터

**EncounterBudget 역할:**
- 동시 활성 적 수 cap (예: 최대 5마리)
- 구간당 스폰 cap (플레이어 주변 10칸 내 최대 2마리)
- 조우 쿨다운 (마지막 조우 후 5초)
- 총 스폰 budget (전체 런에서 최대 30마리)

### B) 스폰 포인트 생성 규칙

1. **우선순위 점수 계산:**
   - 교차로 (3방향 이상): +10점
   - 코너 (2방향, 직각): +5점
   - 막다른 길: +3점
   - 시작/출구 근처 (-5칸 내): -100점 (배제)
   - 플레이어 시작 위치와 거리: +거리/10

2. **배치 알고리즘:**
   - 노드 리스트를 우선순위 점수로 정렬
   - 상위 N개 선택 (Budget 범위 내)
   - 최소 거리 필터링 (5칸 미만 제거)

3. **스폰 타이밍:**
   - 런 시작 시 모든 스폰 포인트 배치 (비활성)
   - 플레이어가 일정 거리 내 진입 시 활성화
   - 활성화 시 EncounterBudget 확인 → 조건 만족 시 스폰

### C) 순찰 경로 생성 규칙

1. **경로 노드 선택:**
   - 스폰 포인트 주변 10칸 내 노드(교차로/코너) 2~4개
   - A* 경로 탐색으로 연결 가능 여부 확인
   - 루프 형성 (마지막 노드 → 첫 노드)

2. **순찰 파라미터:**
   - 이동 속도: 0.5 ~ 1.5 (몬스터별 차이)
   - 대기 시간: 1~3초 (노드 도착 후)
   - 감지 범위: 5~10칸 (FOV 사용)

3. **순찰 중 행동:**
   - 플레이어 FOV 내 감지 → 추적 모드
   - 추적 실패 (5초 미발견) → 순찰 복귀
   - 순찰 경로 이탈 시 최단 경로로 복귀

### D) 디버그 HUD/토글

**DebugPanel UI:**
- 스폰 포인트 표시 (Gizmos)
- 순찰 경로 표시 (선)
- Budget 현황 (활성 적 수 / 총량)
- 강제 스폰 버튼 (테스트용)
- 강제 이벤트 발동 버튼
- Seed 표시 및 재생성 버튼

### E) C# 스켈레톤

#### EncounterBudget.cs

```csharp
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using System;

    [Serializable]
    public class EncounterBudget
    {
        [Header("Caps")]
        public int maxConcurrentEnemies = 5;
        public int maxEnemiesInRadius = 2;
        public float radiusCheck = 10f;
        public int totalSpawnBudget = 30;

        [Header("Cooldown")]
        public float encounterCooldown = 5f;

        [Header("Runtime")]
        public int currentActiveEnemies = 0;
        public int totalSpawned = 0;
        public float lastEncounterTime = 0f;

        public bool CanSpawn(Vector2 playerPos, Vector2 spawnPos)
        {
            // 동시 적 수 확인
            if (currentActiveEnemies >= maxConcurrentEnemies)
                return false;

            // 총 스폰 budget 확인
            if (totalSpawned >= totalSpawnBudget)
                return false;

            // 쿨다운 확인
            if (Time.time - lastEncounterTime < encounterCooldown)
                return false;

            // 반경 내 적 수 확인 (실제로는 Collider.OverlapCircle로 확인)
            float dist = Vector2.Distance(playerPos, spawnPos);
            if (dist < radiusCheck)
            {
                // TODO: 실제 카운트 확인
                return true;
            }

            return true;
        }

        public void OnEnemySpawned()
        {
            currentActiveEnemies++;
            totalSpawned++;
            lastEncounterTime = Time.time;
        }

        public void OnEnemyDespawned()
        {
            currentActiveEnemies = Mathf.Max(0, currentActiveEnemies - 1);
        }

        public void Reset()
        {
            currentActiveEnemies = 0;
            totalSpawned = 0;
            lastEncounterTime = 0f;
        }
    }
}
```

#### SpawnPlanner.cs

```csharp
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Systems.Maze;

    public class SpawnPlanner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int minSpawnPoints = 5;
        [SerializeField] private int maxSpawnPoints = 15;
        [SerializeField] private float minDistanceBetweenSpawns = 5f;
        [SerializeField] private float startExitSafeRadius = 5f;

        private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

        public List<SpawnPoint> Plan(MazeResult maze, System.Random random)
        {
            spawnPoints.Clear();

            // 1. 노드 점수 계산
            List<(MazeNode node, float score)> scored = new List<(MazeNode, float)>();

            foreach (var junction in maze.Junctions)
            {
                float score = CalculateScore(maze, junction);
                scored.Add((junction, score));
            }

            foreach (var corner in maze.Corners)
            {
                float score = CalculateScore(maze, corner);
                scored.Add((corner, score));
            }

            // 2. 점수순 정렬
            scored = scored.OrderByDescending(x => x.score).ToList();

            // 3. 스폰 포인트 선택
            int targetCount = random.Next(minSpawnPoints, maxSpawnPoints + 1);

            foreach (var (node, score) in scored)
            {
                if (spawnPoints.Count >= targetCount) break;
                if (score < 0) continue; // 제외 대상

                // 최소 거리 확인
                if (IsTooClose(node.Position))
                    continue;

                spawnPoints.Add(new SpawnPoint
                {
                    Position = node.Position,
                    NodeType = node.Type,
                    IsActive = false
                });
            }

            Debug.Log($"[SpawnPlanner] Planned {spawnPoints.Count} spawn points");
            return spawnPoints;
        }

        private float CalculateScore(MazeResult maze, MazeNode node)
        {
            float score = 0;

            // 노드 타입 점수
            switch (node.Type)
            {
                case MazeNodeType.Junction:
                    score += 10;
                    break;
                case MazeNodeType.Corner:
                    score += 5;
                    break;
                case MazeNodeType.DeadEnd:
                    score += 3;
                    break;
            }

            // 시작/출구 근처 제외
            float distToStart = Vector2Int.Distance(node.Position, maze.Start);
            float distToExit = Vector2Int.Distance(node.Position, maze.Exit);

            if (distToStart < startExitSafeRadius || distToExit < startExitSafeRadius)
            {
                score = -100;
            }
            else
            {
                // 시작점과 거리 보너스
                score += distToStart / 10f;
            }

            return score;
        }

        private bool IsTooClose(Vector2Int position)
        {
            foreach (var sp in spawnPoints)
            {
                if (Vector2Int.Distance(sp.Position, position) < minDistanceBetweenSpawns)
                {
                    return true;
                }
            }
            return false;
        }

        public List<SpawnPoint> GetSpawnPoints() => spawnPoints;
    }

    [System.Serializable]
    public class SpawnPoint
    {
        public Vector2Int Position;
        public MazeNodeType NodeType;
        public bool IsActive;
        public GameObject SpawnedEnemy;
    }
}
```

#### PatrolPlanner.cs

```csharp
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Systems.Maze;

    public class PatrolPlanner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int minPatrolNodes = 2;
        [SerializeField] private int maxPatrolNodes = 4;
        [SerializeField] private float patrolRadius = 10f;

        public List<PatrolPath> Plan(MazeResult maze, List<SpawnPoint> spawnPoints, System.Random random)
        {
            List<PatrolPath> paths = new List<PatrolPath>();

            foreach (var sp in spawnPoints)
            {
                PatrolPath path = CreatePatrolPath(maze, sp, random);
                if (path != null && path.Nodes.Count >= minPatrolNodes)
                {
                    paths.Add(path);
                }
            }

            Debug.Log($"[PatrolPlanner] Created {paths.Count} patrol paths");
            return paths;
        }

        private PatrolPath CreatePatrolPath(MazeResult maze, SpawnPoint spawnPoint, System.Random random)
        {
            PatrolPath path = new PatrolPath { SpawnPoint = spawnPoint };

            // 주변 노드 수집
            List<MazeNode> nearbyNodes = new List<MazeNode>();

            foreach (var junction in maze.Junctions)
            {
                if (Vector2Int.Distance(junction.Position, spawnPoint.Position) <= patrolRadius)
                {
                    nearbyNodes.Add(junction);
                }
            }

            foreach (var corner in maze.Corners)
            {
                if (Vector2Int.Distance(corner.Position, spawnPoint.Position) <= patrolRadius)
                {
                    nearbyNodes.Add(corner);
                }
            }

            // 랜덤 노드 선택 (2~4개)
            int nodeCount = random.Next(minPatrolNodes, Mathf.Min(maxPatrolNodes, nearbyNodes.Count) + 1);

            for (int i = 0; i < nodeCount && nearbyNodes.Count > 0; i++)
            {
                int index = random.Next(nearbyNodes.Count);
                path.Nodes.Add(nearbyNodes[index].Position);
                nearbyNodes.RemoveAt(index);
            }

            // 시작 위치 추가 (루프)
            if (path.Nodes.Count > 0)
            {
                path.Nodes.Add(spawnPoint.Position);
            }

            return path;
        }
    }

    [System.Serializable]
    public class PatrolPath
    {
        public SpawnPoint SpawnPoint;
        public List<Vector2Int> Nodes = new List<Vector2Int>();
    }
}
```

#### EncounterDirector.cs

```csharp
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Systems.Maze;
    using Game.Core.Events;

    public class EncounterDirector : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpawnPlanner spawnPlanner;
        [SerializeField] private PatrolPlanner patrolPlanner;
        [SerializeField] private EncounterBudget budget = new EncounterBudget();

        [Header("References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject enemyPrefab; // Init용

        [Header("Runtime")]
        private List<SpawnPoint> spawnPoints;
        private List<PatrolPath> patrolPaths;
        private MazeResult maze;
        private System.Random random;

        public void Initialize(MazeResult mazeResult, int seed)
        {
            maze = mazeResult;
            random = new System.Random(seed + 999);

            budget.Reset();

            // 스폰 포인트 계획
            spawnPoints = spawnPlanner.Plan(maze, random);

            // 순찰 경로 계획
            patrolPaths = patrolPlanner.Plan(maze, spawnPoints, random);

            Debug.Log($"[EncounterDirector] Initialized with {spawnPoints.Count} spawn points");
        }

        private void Update()
        {
            if (playerTransform == null || spawnPoints == null) return;

            Vector2 playerPos = playerTransform.position;

            // 스폰 포인트 활성화 체크
            foreach (var sp in spawnPoints)
            {
                if (sp.IsActive || sp.SpawnedEnemy != null) continue;

                float dist = Vector2.Distance(playerPos, sp.Position);
                if (dist < 15f) // 활성화 범위
                {
                    TrySpawnEnemy(sp, playerPos);
                }
            }
        }

        private void TrySpawnEnemy(SpawnPoint sp, Vector2 playerPos)
        {
            if (!budget.CanSpawn(playerPos, sp.Position))
            {
                return;
            }

            // 스폰
            Vector3 spawnPos = new Vector3(sp.Position.x, sp.Position.y, 0);
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            sp.IsActive = true;
            sp.SpawnedEnemy = enemy;

            budget.OnEnemySpawned();

            // TODO: 순찰 경로 할당

            Debug.Log($"[EncounterDirector] Spawned enemy at {sp.Position}");
            GameEvents.TriggerEnemySpawned(sp.Position, null); // EnemyDefinition은 JSON 연동 후
        }

        public void OnEnemyDied(GameObject enemy)
        {
            budget.OnEnemyDespawned();

            // SpawnPoint 정리
            foreach (var sp in spawnPoints)
            {
                if (sp.SpawnedEnemy == enemy)
                {
                    sp.SpawnedEnemy = null;
                    sp.IsActive = false;
                    break;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;

            // 스폰 포인트 (마젠타)
            Gizmos.color = Color.magenta;
            foreach (var sp in spawnPoints)
            {
                Vector3 pos = new Vector3(sp.Position.x, sp.Position.y, 0);
                Gizmos.DrawWireSphere(pos, 0.5f);
                if (sp.IsActive)
                {
                    Gizmos.DrawSphere(pos, 0.3f);
                }
            }

            // 순찰 경로 (흰색)
            if (patrolPaths != null)
            {
                Gizmos.color = Color.white;
                foreach (var path in patrolPaths)
                {
                    for (int i = 0; i < path.Nodes.Count - 1; i++)
                    {
                        Vector3 from = new Vector3(path.Nodes[i].x, path.Nodes[i].y, 0);
                        Vector3 to = new Vector3(path.Nodes[i + 1].x, path.Nodes[i + 1].y, 0);
                        Gizmos.DrawLine(from, to);
                    }
                }
            }
        }
    }
}
```

#### CorridorTrigger.cs

```csharp
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using Game.Core.Events;

    [RequireComponent(typeof(BoxCollider2D))]
    public class CorridorTrigger : MonoBehaviour
    {
        [Header("Event Type")]
        [SerializeField] private EncounterEventType eventType;

        [Header("Runtime")]
        [SerializeField] private bool hasTriggered = false;

        private void Awake()
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered) return;
            if (!other.CompareTag("Player")) return;

            hasTriggered = true;

            Debug.Log($"[CorridorTrigger] {eventType} triggered at {transform.position}");
            GameEvents.TriggerCorridorTriggerActivated(this);

            // TODO: EncounterResolver 호출
        }

        public void Reset()
        {
            hasTriggered = false;
        }
    }

    public enum EncounterEventType
    {
        Trap,
        Loot,
        EventText
    }
}
```

---

## 3. 몬스터 JSON 파이프라인

### A) monsters.json 스키마

```json
{
  "version": "1.0",
  "monsters": [
    {
      "id": "goblin_scout",
      "displayName": "Goblin Scout",
      "archetype": "Melee",
      "prefabPath": "Prefabs/Enemies/Goblin",
      "stats": {
        "maxHealth": 30,
        "moveSpeed": 2.5,
        "attackDamage": 5,
        "attackRange": 1.0,
        "detectionRange": 6.0,
        "attackCooldown": 1.5
      },
      "ai": {
        "behavior": "Patrol",
        "aggroRadius": 7.0,
        "chaseSpeed": 3.5,
        "giveUpTime": 5.0
      },
      "loot": {
        "goldMin": 5,
        "goldMax": 15,
        "dropChance": 0.3,
        "itemPool": ["health_potion_small"]
      }
    },
    {
      "id": "skeleton_warrior",
      "displayName": "Skeleton Warrior",
      "archetype": "Melee",
      "prefabPath": "Prefabs/Enemies/Skeleton",
      "stats": {
        "maxHealth": 50,
        "moveSpeed": 1.8,
        "attackDamage": 10,
        "attackRange": 1.5,
        "detectionRange": 8.0,
        "attackCooldown": 2.0
      },
      "ai": {
        "behavior": "Aggressive",
        "aggroRadius": 10.0,
        "chaseSpeed": 2.5,
        "giveUpTime": 10.0
      },
      "loot": {
        "goldMin": 10,
        "goldMax": 25,
        "dropChance": 0.5,
        "itemPool": ["health_potion_medium", "rusty_sword"]
      }
    }
  ]
}
```

### B) EnemyDefinition DTO

```csharp
namespace Game.DataJson.Schema
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class MonstersData
    {
        public string version;
        public List<EnemyDefinition> monsters;
    }

    [Serializable]
    public class EnemyDefinition
    {
        public string id;
        public string displayName;
        public string archetype; // "Melee", "Ranged", "Caster"
        public string prefabPath;
        public EnemyStats stats;
        public EnemyAI ai;
        public EnemyLoot loot;
    }

    [Serializable]
    public class EnemyStats
    {
        public int maxHealth;
        public float moveSpeed;
        public int attackDamage;
        public float attackRange;
        public float detectionRange;
        public float attackCooldown;
    }

    [Serializable]
    public class EnemyAI
    {
        public string behavior; // "Patrol", "Aggressive", "Defensive"
        public float aggroRadius;
        public float chaseSpeed;
        public float giveUpTime;
    }

    [Serializable]
    public class EnemyLoot
    {
        public int goldMin;
        public int goldMax;
        public float dropChance;
        public List<string> itemPool;
    }
}
```

### C) JsonDataLoader

```csharp
namespace Game.DataJson.Loader
{
    using UnityEngine;
    using System.IO;
    using Game.DataJson.Schema;

    public class JsonDataLoader : MonoBehaviour
    {
        [Header("Loading Path")]
        [SerializeField] private DataLoadingPath loadingPath = DataLoadingPath.StreamingAssets;

        [Header("File Names")]
        [SerializeField] private string monstersFileName = "monsters.json";

        public MonstersData LoadMonsters()
        {
            string path = GetFullPath(monstersFileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"[JsonDataLoader] File not found: {path}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                MonstersData data = JsonUtility.FromJson<MonstersData>(json);

                Debug.Log($"[JsonDataLoader] Loaded {data.monsters.Count} monsters from {path}");
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[JsonDataLoader] Failed to load monsters: {ex.Message}");
                return null;
            }
        }

        private string GetFullPath(string fileName)
        {
            switch (loadingPath)
            {
                case DataLoadingPath.StreamingAssets:
                    return Path.Combine(Application.streamingAssetsPath, "GameData", fileName);

                case DataLoadingPath.Resources:
                    // Resources는 .json 확장자 제외
                    string resourceName = Path.GetFileNameWithoutExtension(fileName);
                    TextAsset textAsset = Resources.Load<TextAsset>($"GameData/{resourceName}");
                    if (textAsset != null)
                    {
                        // 임시 파일로 저장 후 경로 반환 (또는 직접 JSON 파싱)
                        return textAsset.text; // 실제로는 JSON 문자열 직접 반환
                    }
                    return null;

                default:
                    return Path.Combine(Application.streamingAssetsPath, "GameData", fileName);
            }
        }
    }

    public enum DataLoadingPath
    {
        StreamingAssets,  // 추천: 빌드 후 수정 가능
        Resources         // 빌드에 포함, 수정 불가
    }
}
```

### D) DataValidator

```csharp
namespace Game.DataJson.Loader
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.DataJson.Schema;

    public class DataValidator
    {
        public static bool Validate(MonstersData data, out List<string> errors)
        {
            errors = new List<string>();

            if (data == null)
            {
                errors.Add("MonstersData is null");
                return false;
            }

            if (data.monsters == null || data.monsters.Count == 0)
            {
                errors.Add("No monsters defined");
                return false;
            }

            HashSet<string> ids = new HashSet<string>();

            foreach (var monster in data.monsters)
            {
                // ID 중복 검사
                if (string.IsNullOrEmpty(monster.id))
                {
                    errors.Add($"Monster has empty id");
                    continue;
                }

                if (ids.Contains(monster.id))
                {
                    errors.Add($"Duplicate monster id: {monster.id}");
                }
                else
                {
                    ids.Add(monster.id);
                }

                // 스탯 검증
                if (monster.stats == null)
                {
                    errors.Add($"Monster {monster.id} has no stats");
                }
                else
                {
                    if (monster.stats.maxHealth <= 0)
                        errors.Add($"Monster {monster.id} has invalid maxHealth");
                    if (monster.stats.moveSpeed < 0)
                        errors.Add($"Monster {monster.id} has invalid moveSpeed");
                }

                // AI 검증
                if (monster.ai == null)
                {
                    errors.Add($"Monster {monster.id} has no AI config");
                }

                // Prefab 경로 검증
                if (string.IsNullOrEmpty(monster.prefabPath))
                {
                    errors.Add($"Monster {monster.id} has no prefab path");
                }
            }

            bool isValid = errors.Count == 0;

            if (isValid)
            {
                Debug.Log($"[DataValidator] Validation passed for {data.monsters.Count} monsters");
            }
            else
            {
                Debug.LogWarning($"[DataValidator] Validation failed with {errors.Count} errors");
                foreach (var error in errors)
                {
                    Debug.LogError($"  - {error}");
                }
            }

            return isValid;
        }
    }
}
```

### E) EnemyRegistry

```csharp
namespace Game.DataJson.Registry
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.DataJson.Schema;

    public class EnemyRegistry : MonoBehaviour
    {
        private static EnemyRegistry instance;
        public static EnemyRegistry Instance => instance;

        private Dictionary<string, EnemyDefinition> registry = new Dictionary<string, EnemyDefinition>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Register(MonstersData data)
        {
            registry.Clear();

            foreach (var monster in data.monsters)
            {
                registry[monster.id] = monster;
            }

            Debug.Log($"[EnemyRegistry] Registered {registry.Count} enemies");
        }

        public EnemyDefinition Get(string id)
        {
            if (registry.TryGetValue(id, out EnemyDefinition def))
            {
                return def;
            }

            Debug.LogWarning($"[EnemyRegistry] Enemy not found: {id}");
            return null;
        }

        public bool Has(string id) => registry.ContainsKey(id);

        public List<EnemyDefinition> GetAll() => new List<EnemyDefinition>(registry.Values);

        public List<string> GetAllIds() => new List<string>(registry.Keys);
    }
}
```

### F) EnemyFactory

```csharp
namespace Game.Gameplay.Enemy
{
    using UnityEngine;
    using Game.DataJson.Schema;

    public class EnemyFactory : MonoBehaviour
    {
        public GameObject Create(EnemyDefinition definition, Vector3 position)
        {
            // 1. Prefab 로드
            GameObject prefab = Resources.Load<GameObject>(definition.prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[EnemyFactory] Prefab not found: {definition.prefabPath}");
                return null;
            }

            // 2. 인스턴스화
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            instance.name = definition.displayName;

            // 3. 스탯 바인딩
            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(definition);
            }
            else
            {
                Debug.LogWarning($"[EnemyFactory] No EnemyController on {definition.id}");
            }

            Debug.Log($"[EnemyFactory] Created {definition.displayName} at {position}");
            return instance;
        }
    }
}
```

### G) 로딩 경로 2안 비교

| 경로 | 장점 | 단점 | 추천 |
|------|------|------|------|
| **StreamingAssets** | 빌드 후 수정 가능, 외부 편집 용이 | 첫 로딩 시 파일 I/O | ★★★★★ |
| **Resources** | 빌드에 포함, 빠른 로딩 | 빌드 후 수정 불가, Resources 폴더 복잡도 증가 | ★★☆☆☆ |

**Init 추천: StreamingAssets**
- 몬스터 밸런싱 시 빌드 없이 JSON 수정 가능
- 외부 툴(Excel → JSON 변환 등) 연동 용이
- 모드 지원 확장 가능

---

## 4. FOV 구현 방식 2안 비교 (미로 최적화)

### 비교표

| 항목 | A) Raycast Fan (Phase 1 방식) | B) Shadowcasting (Roguelike 전통) |
|------|-------------------------------|-------------------------------------|
| **구현 난이도** | 중간 (레이캐스트 + 메쉬) | 높음 (그리드 기반 알고리즘) |
| **미로 적합성** | ★★★☆☆ (레이 간격 문제) | ★★★★★ (그리드 완벽 매칭) |
| **벽 Occlusion** | 높음 (Physics2D 활용) | 높음 (셀 단위 차단) |
| **Fog 연동** | 레이 끝점 → 셀 변환 필요 | 셀 결과 직접 사용 가능 |
| **성능** | 레이캐스트 비용 (rayCount 의존) | 그리드 순회 비용 (반경 의존) |
| **확장성** | 각도/거리 자유 조절 | 그리드 크기 의존 |
| **Init 적합성** | ★★★☆☆ | ★★★★☆ |

### Init 추천안: **B) Shadowcasting (개선: Symmetric Shadowcasting)**

**이유:**
1. **미로와 완벽 매칭**: 그리드 기반 미로 → 그리드 기반 FOV
2. **셀 결과 직접 활용**: Fog-of-War가 그대로 사용 가능
3. **로그라이크 전통**: 명확한 시야/비시야 구분
4. **디버그 용이**: 셀 단위로 명확히 표시 가능

**Phase 1 Raycast 방식의 미로에서의 문제:**
- 레이 간격으로 인해 좁은 복도에서 셀이 누락될 수 있음
- 레이 끝점을 셀로 변환하는 과정에서 부정확성
- 미로의 직각 구조와 부채꼴 시야의 불일치

### Shadowcasting 알고리즘 설명

**Symmetric Shadowcasting (Init 추천):**
- 중심에서 8방향으로 octant 스캔
- 각 octant마다 행(row) 단위로 셀 탐색
- 벽을 만나면 그림자(shadow) 생성
- 이전 행의 그림자를 현재 행에 적용
- 대칭성 보장 (reciprocal visibility)

**단계:**
1. 플레이어 위치 중심 (origin)
2. 8개 octant 각각 처리
3. 각 octant에서 행 단위 탐색 (distance 1 ~ viewRange)
4. 각 행의 셀이 그림자에 가려지는지 확인
5. 가려지지 않으면 visible 추가
6. 벽이면 그림자 추가

---

## 5. 선택한 FOV 방식 구현 (Shadowcasting)

### A) 오브젝트 구성

```
Player
├── Sprite
├── PlayerController
├── FacingProvider (유지, 방향 제한용)
└── FieldOfView2D (Shadowcasting으로 교체)
```

**렌더링:**
- Fog-of-War가 직접 처리 (FOV는 데이터만 제공)
- Stencil Shader는 Phase 2에서 제거 (불필요)

### B) Fog가 재사용할 데이터 형태

```csharp
// FOV → Fog로 전달되는 데이터
public class VisionData
{
    public Vector2Int OriginCell { get; set; }
    public HashSet<Vector2Int> VisibleCells { get; set; } // 셀 집합
    public int ViewRange { get; set; }
}

// 이벤트 발행
GameEvents.OnVisionCellsUpdated?.Invoke(visibleCells);
```

### C) C# 스켈레톤

#### FieldOfView2D.cs (Shadowcasting 버전)

```csharp
namespace Game.Systems.Vision
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Core.Events;
    using Game.Systems.Maze;

    public class FieldOfView2D : MonoBehaviour
    {
        [Header("FOV Parameters")]
        [SerializeField] private int viewRange = 8;
        [SerializeField] private float updateRate = 0.1f;

        [Header("Maze Reference")]
        [SerializeField] private MazeGenerator mazeGenerator;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private HashSet<Vector2Int> visibleCells = new HashSet<Vector2Int>();
        private MazeResult maze;
        private float updateTimer;

        private void Start()
        {
            if (mazeGenerator != null)
            {
                maze = mazeGenerator.GetCurrentMaze();
            }
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
            if (maze == null) return;

            visibleCells.Clear();

            Vector2Int origin = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );

            // 중심 셀은 항상 보임
            visibleCells.Add(origin);

            // 8개 octant 스캔
            for (int octant = 0; octant < 8; octant++)
            {
                ScanOctant(origin, octant);
            }

            // 이벤트 발행
            GameEvents.TriggerVisionCellsUpdated(visibleCells);
        }

        private void ScanOctant(Vector2Int origin, int octant)
        {
            List<(float start, float end)> shadows = new List<(float, float)>();

            for (int row = 1; row <= viewRange; row++)
            {
                for (int col = 0; col <= row; col++)
                {
                    Vector2Int cell = TransformOctant(origin, row, col, octant);

                    if (!maze.InBounds(cell)) continue;

                    // 그림자에 가려지는지 확인
                    float cellAngleStart = (float)col / (row + 0.5f);
                    float cellAngleEnd = (float)(col + 1) / (row - 0.5f);

                    if (IsInShadow(cellAngleStart, cellAngleEnd, shadows))
                        continue;

                    // 보임
                    visibleCells.Add(cell);

                    // 벽이면 그림자 추가
                    if (maze.IsWall(cell))
                    {
                        AddShadow(cellAngleStart, cellAngleEnd, shadows);
                    }
                }
            }
        }

        private Vector2Int TransformOctant(Vector2Int origin, int row, int col, int octant)
        {
            switch (octant)
            {
                case 0: return origin + new Vector2Int(col, row);
                case 1: return origin + new Vector2Int(row, col);
                case 2: return origin + new Vector2Int(row, -col);
                case 3: return origin + new Vector2Int(col, -row);
                case 4: return origin + new Vector2Int(-col, -row);
                case 5: return origin + new Vector2Int(-row, -col);
                case 6: return origin + new Vector2Int(-row, col);
                case 7: return origin + new Vector2Int(-col, row);
                default: return origin;
            }
        }

        private bool IsInShadow(float start, float end, List<(float start, float end)> shadows)
        {
            foreach (var shadow in shadows)
            {
                if (start >= shadow.start && end <= shadow.end)
                    return true;
            }
            return false;
        }

        private void AddShadow(float start, float end, List<(float start, float end)> shadows)
        {
            shadows.Add((start, end));
            // TODO: 그림자 병합 (최적화)
        }

        public bool IsVisible(Vector2Int cell) => visibleCells.Contains(cell);

        private void OnDrawGizmos()
        {
            if (!drawGizmos || visibleCells == null) return;

            Gizmos.color = new Color(1, 1, 0, 0.3f);
            foreach (var cell in visibleCells)
            {
                Gizmos.DrawCube(new Vector3(cell.x, cell.y, 0), Vector3.one * 0.9f);
            }
        }
    }
}
```

---

## 6. Fog-of-War 3단계 구현 (셀 채우기 방식)

### "레이 끝점만 찍기" 문제

Phase 1 방식의 문제:
- 레이캐스트 결과의 끝점만 Fog에 반영
- 중간 셀이 누락되어 "구멍 뚫린" Fog
- 미로의 긴 복도에서 특히 심각

### 셀 채우기 방식 (Phase 2)

**VisionCellFiller:**
- FOV 결과 (HashSet<Vector2Int>) 받기
- 각 visible 셀을 Fog 그리드에 직접 반영
- 중간 셀 누락 없음

**Fog 3단계:**
1. **미탐색 (Unexplored)**: 한 번도 본 적 없음 → 검은색 타일 (알파 0.9)
2. **탐색 완료 (Explored)**: 과거에 봤음 → 회색 타일 (알파 0.5)
3. **현재 시야 (Visible)**: 지금 보임 → Fog 타일 제거 (밝게)

### C# 스켈레톤

#### FogOfWarSystem.cs (Phase 2, 셀 기반)

```csharp
namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Core.Events;

    public class FogOfWarSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FogRenderer fogRenderer;
        [SerializeField] private MazeGenerator mazeGenerator;

        private FogOfWarGrid grid;
        private MazeResult maze;

        private void Start()
        {
            if (mazeGenerator != null)
            {
                maze = mazeGenerator.GetCurrentMaze();
                Initialize();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnVisionCellsUpdated += OnVisionCellsUpdated;
        }

        private void OnDisable()
        {
            GameEvents.OnVisionCellsUpdated -= OnVisionCellsUpdated;
        }

        private void Initialize()
        {
            if (maze == null) return;

            grid = new FogOfWarGrid(maze.Width, maze.Height, Vector2Int.zero);

            if (fogRenderer != null)
            {
                fogRenderer.Initialize(grid);
            }

            Debug.Log($"[FogOfWarSystem] Initialized: {grid}");
        }

        private void OnVisionCellsUpdated(HashSet<Vector2Int> visibleCells)
        {
            if (grid == null) return;

            // 셀 단위로 직접 업데이트
            grid.UpdateVisibility(visibleCells);

            if (fogRenderer != null)
            {
                fogRenderer.Render(grid);
            }
        }
    }
}
```

#### FogOfWarGrid.cs (셀 기반)

```csharp
namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using System.Collections.Generic;

    public class FogOfWarGrid
    {
        private HashSet<Vector2Int> visitedCells;
        private HashSet<Vector2Int> visibleCells;

        public int Width { get; }
        public int Height { get; }

        public FogOfWarGrid(int width, int height, Vector2Int offset)
        {
            Width = width;
            Height = height;

            visitedCells = new HashSet<Vector2Int>();
            visibleCells = new HashSet<Vector2Int>();
        }

        public void UpdateVisibility(HashSet<Vector2Int> newVisibleCells)
        {
            visibleCells.Clear();

            foreach (var cell in newVisibleCells)
            {
                visibleCells.Add(cell);
                visitedCells.Add(cell); // 탐색 기록
            }
        }

        public bool IsVisible(Vector2Int cell) => visibleCells.Contains(cell);
        public bool IsVisited(Vector2Int cell) => visitedCells.Contains(cell);
        public bool IsUnexplored(Vector2Int cell) => !visitedCells.Contains(cell);

        public float GetExplorationProgress(int totalFloorCells)
        {
            return totalFloorCells > 0 ? (float)visitedCells.Count / totalFloorCells : 0f;
        }
    }
}
```

---

## 마무리 산출물

### Phase 2 Done 기준 (DoD) 10개

1. **미로 생성**: DFS 백트래킹으로 41x41 미로 생성, start-exit 연결 보장
2. **재현성**: 동일 Seed 입력 시 동일한 미로 생성 확인
3. **노드 분석**: 교차로/코너/막다른 길 자동 분석 및 Gizmos 표시
4. **스폰 배치**: 교차로 우선 스폰 포인트 5~15개 배치, 최소 거리 보장
5. **순찰 경로**: 각 스폰 포인트마다 2~4 노드 순찰 루프 생성
6. **JSON 로드**: monsters.json 로드 → 검증 → EnemyRegistry 등록
7. **Budget 제한**: 동시 적 수 cap, 총량 제한, 쿨다운 정상 작동
8. **Shadowcasting FOV**: 그리드 기반 시야, 벽 Occlusion 정확
9. **Fog 3단계**: 미탐색/탐색/현재시야 명확히 구분, 셀 채우기 방식
10. **디버그 패널**: 스폰 포인트/순찰 경로/Budget 현황 표시

### 성능/리스크 체크리스트 10개

| 항목 | 리스크 | 대응 방안 |
|------|--------|-----------|
| **Shadowcasting 비용** | viewRange가 크면 셀 순회 과다 | viewRange 8~12 제한, updateRate 0.1초 |
| **Fog Tilemap 업데이트** | 매 프레임 전체 셀 업데이트 시 GC | 변경된 셀만 업데이트 (HashSet diff) |
| **JSON 파싱** | 큰 JSON 파일 로드 시 프리징 | 비동기 로드 (Coroutine) 또는 분할 로드 |
| **스폰 포인트 과다** | 너무 많은 적 동시 활성화 | maxConcurrentEnemies cap (5마리) |
| **순찰 경로 A*  비용** | 모든 적이 매 프레임 경로 계산 | 경로 캐싱, updateRate 조절 |
| **Tilemap Collider** | CompositeCollider 없으면 콜라이더 과다 | TilemapCollider + CompositeCollider 조합 |
| **미로 크기** | 너무 큰 미로 (100x100+) 생성 시 지연 | 41x41 기본, 최대 61x61 제한 |
| **JSON 검증 누락** | 잘못된 JSON으로 런타임 오류 | DataValidator로 로드 후 즉시 검증 |
| **EnemyFactory Prefab 로드** | Resources.Load 동기 호출 지연 | 사전 로드 (Awake/Start)또는 비동기 |
| **Fog 렌더링** | 대량 Tilemap.SetTile 호출 | SetTilesBlock 사용 (배치 업데이트) |

### 다음 확장 로드맵 6개

| 순서 | 확장 항목 | 설명 |
|------|-----------|------|
| 1 | **다층 던전** | 미로 층별 생성, 층마다 난이도/테이블 변화, 계단 연결 |
| 2 | **바이옴 시스템** | TilePalette 바이옴별 분리, 시각적 다양성, 특수 규칙 |
| 3 | **Enemy AI 다양화** | BehaviorTree 도입, 패턴 추가 (원거리/마법/소환), 협동 AI |
| 4 | **JSON 확장** | items.json, encounters.json, biomes.json 추가 |
| 5 | **RunMetrics & Replay** | 탐색률/조우 수/시간 기록, Replay 시스템 (입력 녹화/재생) |
| 6 | **메타 진행** | 런 종료 후 언락 요소, 영구 업그레이드, 도전 과제 |

---

## 다음 단계 (Phase 3 예정)

### 처리 완료 항목 (Phase 2)
- ✅ 미로 기반 생성 (DFS 백트래킹)
- ✅ 길목 조우 시스템 (EncounterDirector)
- ✅ JSON 기반 몬스터 파이프라인
- ✅ Shadowcasting FOV
- ✅ Fog-of-War 셀 채우기

### Phase 3에서 필요한 작업
1. **실제 Unity 프로젝트 통합** (Phase 1 → Phase 2 마이그레이션)
2. **JSON 파일 생성** (monsters.json, StreamingAssets 배치)
3. **Enemy AI 구현** (순찰/추적/공격)
4. **전투 시스템** (CombatSystem, 피격/사망 처리)
5. **DebugPanel UI** (스폰 포인트 토글, Budget 표시)
6. **성능 최적화** (Tilemap 배치 업데이트, A* 캐싱)
7. **테스트 및 밸런싱**

### 고려사항
- **Phase 1 코드 재사용**: FOV/Fog 기반 코드 유지, 인터페이스만 변경
- **점진적 마이그레이션**: DungeonGenerator → MazeGenerator 병행 가능
- **JSON 에디터 툴**: monsters.json 편집용 Unity 에디터 확장 (옵션)
- **성능 프로파일링**: Shadowcasting vs Raycast 실측 비교

---

이상으로 **Phase 2 Response**를 완료합니다. Phase 3에서 실제 Unity 프로젝트에 통합하고 AI/전투 시스템을 구현하겠습니다.
