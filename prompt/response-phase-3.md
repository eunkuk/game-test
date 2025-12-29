# Response Phase 3: Unity 프로젝트 통합 및 플레이 가능한 런 완성

## 0. 전체 그림 (아키텍처)

### Phase 2 → Phase 3 전환 개요

**Phase 2**: 설계 및 코드 스켈레톤 제공
**Phase 3**: 실제 Unity 프로젝트에 통합하여 플레이 가능한 1층 런 완성

**핵심 목표**:
- 시작 → 미로 탐색 → 적 조우 → 전투 → 탈출/사망 → 결과 표시
- Seed 기반 재현성 보장
- JSON 기반 몬스터 정의로 코드 수정 없이 밸런싱 가능
- 디버그 도구로 개발 효율 극대화

### 모듈 구성 (최종)

```
Assets/_Project/
├── Core/                          # Game.Core.asmdef
│   ├── Random/
│   │   └── SeededRandom.cs
│   ├── Events/
│   │   └── GameEvents.cs
│   ├── Interfaces/
│   │   ├── IPoolable.cs
│   │   ├── IDamageable.cs (NEW)
│   │   └── IEnemyAI.cs (NEW)
│   └── Utils/
│       ├── GridUtils.cs
│       └── PathfindingHelper.cs (NEW)
│
├── DataJson/                      # Game.DataJson.asmdef
│   ├── Schema/
│   │   ├── EnemyDefinition.cs
│   │   ├── EncounterTableData.cs
│   │   └── MonstersData.cs
│   ├── Loader/
│   │   ├── JsonDataLoader.cs
│   │   └── DataValidator.cs
│   └── Registry/
│       ├── EnemyRegistry.cs
│       └── EncounterRegistry.cs (NEW)
│
├── Systems/                       # Game.Systems.asmdef
│   ├── Maze/
│   │   ├── MazeGenerator.cs
│   │   ├── MazeResult.cs
│   │   ├── MazeConfig.cs
│   │   ├── MazeTilemapPainter.cs
│   │   └── MazeNode.cs
│   ├── Encounter/
│   │   ├── EncounterDirector.cs
│   │   ├── SpawnPlanner.cs
│   │   ├── PatrolPlanner.cs
│   │   ├── EncounterBudget.cs
│   │   ├── CorridorTrigger.cs
│   │   └── EncounterResolver.cs
│   ├── Vision/
│   │   ├── FieldOfView2D.cs (Shadowcasting)
│   │   └── FacingProvider.cs
│   ├── FogOfWar/
│   │   ├── FogOfWarSystem.cs
│   │   ├── FogOfWarGrid.cs
│   │   └── FogRenderer.cs
│   └── Pathfinding/               # NEW
│       ├── GridPathfinder.cs (A* 간단 구현)
│       └── PathCache.cs
│
├── Gameplay/                      # Game.Gameplay.asmdef
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerStats.cs
│   │   ├── PlayerAttack.cs (NEW)
│   │   └── PlayerInventory.cs
│   ├── Enemy/
│   │   ├── EnemyController.cs (NEW)
│   │   ├── EnemyAI.cs (NEW - FSM)
│   │   ├── EnemySensing.cs (NEW)
│   │   ├── EnemyPatrol.cs (NEW)
│   │   ├── EnemyChase.cs (NEW)
│   │   ├── EnemyAttack.cs (NEW)
│   │   └── EnemyFactory.cs
│   ├── Combat/
│   │   ├── Health.cs (NEW)
│   │   ├── CombatSystem.cs (NEW)
│   │   └── IDamageable.cs → Core로 이동
│   └── Pooling/                   # NEW
│       ├── IPoolable.cs → Core로 이동
│       └── SimplePoolManager.cs
│
├── UI/                            # Game.UI.asmdef
│   ├── HUD/
│   │   ├── PlayerHUD.cs
│   │   └── DebugPanel.cs (NEW)
│   └── Encounter/
│       └── EventChoicePanel.cs
│
└── Runtime/                       # Game.Runtime.asmdef
    ├── GameRunManager.cs (NEW)
    └── States/
        ├── GameStateMachine.cs
        ├── TitleState.cs
        ├── RunState.cs
        └── ResultState.cs

StreamingAssets/
└── GameData/
    ├── monsters.json (NEW)
    └── encounters.json (NEW)
```

### 의존성 다이어그램 (최종)

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

**핵심 규칙**:
- Core: 공용 인터페이스/유틸만, 구체 구현 금지
- DataJson: DTO/로더/검증만, 런타임 로직 금지
- Systems: 게임 로직 독립 (플레이어/적 의존 최소)
- Gameplay: Systems 활용, 구체적 게임플레이 구현
- UI: Gameplay/Systems 표시 및 입력
- Runtime: 최상위 조합 레이어

### Run 씬 하이어라키 구성

```
Run (Scene)
├── ═══ Managers ═══
│   ├── GameRunManager          # 런 전체 관리, Seed/로딩/생성/초기화
│   ├── JsonDataLoader          # JSON 로드
│   └── EnemyRegistry           # 몬스터 레지스트리 싱글톤
│
├── ═══ Grid ═══
│   ├── Grid
│   │   ├── FloorTilemap        # Sorting Layer: Ground (0)
│   │   ├── WallTilemap         # Sorting Layer: Ground (0), TilemapCollider2D + CompositeCollider2D
│   │   └── FogTilemap          # Sorting Layer: Overlay (10)
│   │
│   └── MazeGenerator           # MazeTilemapPainter 참조
│       └── MazeTilemapPainter
│
├── ═══ Systems ═══
│   ├── EncounterDirector
│   │   ├── SpawnPlanner
│   │   └── PatrolPlanner
│   ├── FogOfWarSystem
│   │   └── FogRenderer
│   └── CombatSystem
│
├── ═══ Player ═══
│   └── Player                  # Tag: Player, Layer: Player (8)
│       ├── Sprite
│       ├── CircleCollider2D
│       ├── Rigidbody2D (Kinematic)
│       ├── PlayerController
│       ├── PlayerStats (Health)
│       ├── PlayerAttack
│       ├── FacingProvider
│       └── FieldOfView2D       # Shadowcasting
│
├── ═══ Enemies ═══              # 런타임 스폰
│   └── (EnemyFactory가 생성)
│
├── ═══ UI ═══
│   └── Canvas
│       ├── PlayerHUD
│       │   ├── HealthBar
│       │   └── GoldText
│       └── DebugPanel
│           ├── TogglesPanel
│           ├── MetricsPanel
│           └── ActionsPanel
│
└── ═══ Camera ═══
    └── Main Camera             # Follow Player
```

### 런타임 흐름 (시작 → 종료)

```
[게임 시작]
    ↓
1. GameRunManager.Start()
   - JsonDataLoader.LoadMonsters() → EnemyRegistry.Register()
   - Seed 설정 (입력 or 랜덤)
    ↓
2. MazeGenerator.Generate(seed)
   - DFS 백트래킹 미로 생성
   - MazeTilemapPainter.PaintMaze()
   - GameEvents.TriggerMazeGenerated(result)
    ↓
3. Player.Spawn(maze.Start)
   - PlayerController 초기화
   - FieldOfView2D 활성화
   - FogOfWarSystem 활성화
    ↓
4. EncounterDirector.Initialize(maze, seed)
   - SpawnPlanner.Plan() → 스폰 포인트 배치
   - PatrolPlanner.Plan() → 순찰 경로 생성
   - EncounterBudget.Reset()
    ↓
5. [게임 루프]
   ├─ Player 이동 (WASD)
   │   ↓
   ├─ FieldOfView2D 업데이트 (0.1s마다)
   │   → OnVisionCellsUpdated → FogOfWarSystem
   │   ↓
   ├─ EncounterDirector.Update()
   │   - 플레이어 근처 스폰 포인트 활성화 체크
   │   - Budget 조건 만족 시 EnemyFactory.Create()
   │   - EnemyController 초기화 (stats, patrol path)
   │   ↓
   ├─ EnemyAI.Update() (각 적마다)
   │   - Patrol: 순찰 경로 따라 이동
   │   - Sensing: FOV/거리 체크 → 플레이어 감지
   │   - Chase: 추적 (A* 경로)
   │   - Attack: 사거리 내 공격
   │   - Return: 추적 실패 시 순찰 복귀
   │   ↓
   ├─ CombatSystem
   │   - PlayerAttack ↔ EnemyHealth (피격/사망)
   │   - EnemyAttack ↔ PlayerHealth (피격/사망)
   │   - GameEvents.OnEnemyDied → Budget.OnEnemyDespawned()
   │   ↓
   └─ 승리/사망 조건 체크
       - Player가 maze.Exit 도달 → Win
       - Player.Health <= 0 → Lose
    ↓
6. [런 종료]
   - RunMetrics 기록 (탐색률, 조우 수, 시간)
   - Result 씬 전환 (옵션)
```

### 점진적 마이그레이션 전략

**Phase 1 코드 재사용:**
- ✅ GameEvents (확장만)
- ✅ FacingProvider (유지)
- ✅ FogOfWarGrid/Renderer (셀 기반으로 수정)
- ✅ TilemapPainter 패턴 (Dungeon → Maze)

**Phase 1 → Phase 2 전환 옵션 (개발 편의):**

```csharp
// GameRunManager.cs
public enum GenerationType { Dungeon, Maze }
[SerializeField] private GenerationType genType = GenerationType.Maze;

public void Initialize()
{
    if (genType == GenerationType.Dungeon)
    {
        dungeonGenerator.Generate(seed); // Phase 1
    }
    else
    {
        mazeGenerator.Generate(seed); // Phase 2
    }
}
```

**점진적 테스트 단계:**
1. Maze 생성만 테스트 (Phase 1 DungeonGenerator 비활성)
2. Fog-of-War 셀 기반 전환 테스트
3. JSON 로딩 단독 테스트
4. Enemy 스폰만 테스트 (AI 없이)
5. AI Patrol만 테스트
6. 전투 추가
7. 전체 통합

---

## 1. 실제 Unity 프로젝트 통합 가이드

### A) 프로젝트/패키지/레이어/정렬 세팅 체크리스트

#### 1.1 새 프로젝트 생성

- [ ] Unity Hub에서 "New Project" → URP 2D Template 선택
- [ ] Unity 버전: 2022.3 LTS 또는 2023.1+
- [ ] 프로젝트 이름: Labyrinth
- [ ] 생성 후 첫 실행 대기 (패키지 임포트)

#### 1.2 필수 패키지 설치

- [ ] Window → Package Manager
  - [ ] TextMeshPro (필수, UI용)
  - [ ] 2D Tilemap Editor (URP 템플릿에 포함)
  - [ ] Input System (선택, Phase 3에서는 Legacy Input 사용 가능)

#### 1.3 Layers 설정

- [ ] Edit → Project Settings → Tags and Layers
  - [ ] Layer 6: `Wall`
  - [ ] Layer 7: `VisionBlocker` (확장용)
  - [ ] Layer 8: `Player`
  - [ ] Layer 9: `Enemy`
  - [ ] Layer 10: `Projectile` (확장용)

#### 1.4 Tags 설정

- [ ] Tags:
  - [ ] `Player`
  - [ ] `Enemy`
  - [ ] `Exit`

#### 1.5 Sorting Layers 설정

- [ ] Edit → Project Settings → Tags and Layers → Sorting Layers
  - [ ] 0: `Ground` (Floor + Wall Tilemaps)
  - [ ] 5: `Objects` (Player, Enemy)
  - [ ] 10: `Overlay` (Fog Tilemap)
  - [ ] 15: `UI` (Canvas - Screen Space Overlay)

#### 1.6 Physics2D Layer Collision Matrix

- [ ] Edit → Project Settings → Physics 2D → Layer Collision Matrix
  - [ ] `Player` ↔ `Wall`: ✅ (충돌)
  - [ ] `Player` ↔ `Enemy`: ✅ (충돌)
  - [ ] `Player` ↔ `Player`: ❌
  - [ ] `Enemy` ↔ `Wall`: ✅ (충돌)
  - [ ] `Enemy` ↔ `Enemy`: ❌
  - [ ] `Wall` ↔ `VisionBlocker`: ❌

#### 1.7 폴더 구조 생성

```
Assets/
├── _Project/
│   ├── Core/
│   ├── DataJson/
│   ├── Systems/
│   ├── Gameplay/
│   ├── UI/
│   ├── Runtime/
│   ├── Prefabs/
│   │   ├── Player/
│   │   └── Enemies/
│   ├── Tiles/
│   │   ├── Floor/
│   │   └── Wall/
│   └── Scenes/
│       ├── Title.unity
│       ├── Run.unity
│       └── Result.unity
│
StreamingAssets/
└── GameData/
    ├── monsters.json
    └── encounters.json
```

#### 1.8 Assembly Definition 생성

- [ ] `Assets/_Project/Core/` → 우클릭 → Create → Assembly Definition
  - Name: `Game.Core`
  - References: (없음)

- [ ] `Assets/_Project/DataJson/` → Assembly Definition
  - Name: `Game.DataJson`
  - References: `Game.Core`

- [ ] `Assets/_Project/Systems/` → Assembly Definition
  - Name: `Game.Systems`
  - References: `Game.Core`, `Game.DataJson`

- [ ] `Assets/_Project/Gameplay/` → Assembly Definition
  - Name: `Game.Gameplay`
  - References: `Game.Core`, `Game.DataJson`, `Game.Systems`

- [ ] `Assets/_Project/UI/` → Assembly Definition
  - Name: `Game.UI`
  - References: `Game.Core`, `Game.DataJson`, `Game.Systems`, `Game.Gameplay`

- [ ] `Assets/_Project/Runtime/` → Assembly Definition
  - Name: `Game.Runtime`
  - References: 모든 모듈 (Core, DataJson, Systems, Gameplay, UI)

---

### B) Run 씬 구성 체크리스트

#### 2.1 기본 오브젝트 생성

- [ ] 새 씬 생성: `Assets/_Project/Scenes/Run.unity`
- [ ] Main Camera 설정
  - [ ] Projection: Orthographic
  - [ ] Size: 10
  - [ ] Follow Script 추가 (나중에)

#### 2.2 Grid + Tilemap 생성

- [ ] Hierarchy → 우클릭 → 2D Object → Tilemap → Rectangular
- [ ] Grid 오브젝트 이름 변경: `Grid`
- [ ] Tilemap 오브젝트 3개 생성:
  - [ ] `FloorTilemap`
    - Tilemap Renderer → Sorting Layer: `Ground`, Order: 0
  - [ ] `WallTilemap`
    - Tilemap Renderer → Sorting Layer: `Ground`, Order: 1
    - Add Component → Tilemap Collider 2D
    - Add Component → Composite Collider 2D
    - Rigidbody2D → Body Type: Static
    - Tilemap Collider 2D → Used By Composite: ✅
    - GameObject Layer: `Wall`
  - [ ] `FogTilemap`
    - Tilemap Renderer → Sorting Layer: `Overlay`, Order: 0

#### 2.3 Tiles 생성

- [ ] `Assets/_Project/Tiles/Floor/` → 우클릭 → Create → 2D → Tiles → Rule Tile (or Single Tile)
  - [ ] 임시 스프라이트 할당 (흰색 사각형)
  - [ ] 이름: `FloorTile`

- [ ] `Assets/_Project/Tiles/Wall/` → Rule Tile
  - [ ] 임시 스프라이트 할당 (회색 사각형)
  - [ ] 이름: `WallTile`

- [ ] `Assets/_Project/Tiles/Fog/` → Tile
  - [ ] 검은색 반투명 스프라이트 (알파 0.9)
  - [ ] 이름: `UnexploredTile`
  - [ ] 회색 반투명 스프라이트 (알파 0.5)
  - [ ] 이름: `ExploredTile`

#### 2.4 MazeGenerator 오브젝트 생성

- [ ] Hierarchy → Create Empty → 이름: `MazeGenerator`
- [ ] Add Component → `MazeGenerator` (스크립트)
- [ ] Add Component → `MazeTilemapPainter` (스크립트, 자식 오브젝트로)
- [ ] Inspector 설정:
  - [ ] Config: 인라인 설정 (width: 41, height: 41)
  - [ ] Painter 참조 연결
  - [ ] autoGenerate: ✅ (테스트용)

#### 2.5 MazeTilemapPainter 설정

- [ ] Inspector 설정:
  - [ ] Floor Tilemap: `FloorTilemap` 드래그
  - [ ] Wall Tilemap: `WallTilemap` 드래그
  - [ ] Floor Tile: `FloorTile` 드래그
  - [ ] Wall Tile: `WallTile` 드래그

#### 2.6 GameRunManager 생성

- [ ] Hierarchy → Create Empty → 이름: `GameRunManager`
- [ ] Add Component → `GameRunManager` (스크립트)
- [ ] Add Component → `JsonDataLoader` (스크립트)
- [ ] Add Component → `EnemyRegistry` (스크립트, 싱글톤)

#### 2.7 EncounterDirector 생성

- [ ] Hierarchy → Create Empty → 이름: `EncounterDirector`
- [ ] Add Component → `EncounterDirector`
- [ ] 자식 오브젝트:
  - [ ] `SpawnPlanner` (Add Component)
  - [ ] `PatrolPlanner` (Add Component)

#### 2.8 FogOfWarSystem 생성

- [ ] Hierarchy → Create Empty → 이름: `FogOfWarSystem`
- [ ] Add Component → `FogOfWarSystem`
- [ ] 자식 오브젝트:
  - [ ] `FogRenderer` (Add Component)
- [ ] Inspector 설정:
  - [ ] Fog Renderer 참조 연결
  - [ ] Maze Generator 참조 연결

#### 2.9 CombatSystem 생성

- [ ] Hierarchy → Create Empty → 이름: `CombatSystem`
- [ ] Add Component → `CombatSystem` (스크립트)

---

### C) 프리팹 구성 체크리스트

#### 3.1 Player 프리팹

**생성:**
- [ ] Hierarchy → 2D Object → Sprite → 이름: `Player`
- [ ] Sprite Renderer: 임시 스프라이트 (파란색 원)
- [ ] Sorting Layer: `Objects`, Order: 0
- [ ] Tag: `Player`, Layer: `Player`

**컴포넌트 추가:**
- [ ] Add Component → Circle Collider 2D
  - Radius: 0.4
- [ ] Add Component → Rigidbody 2D
  - Body Type: Kinematic
  - Constraints: Freeze Rotation Z ✅
- [ ] Add Component → PlayerController
- [ ] Add Component → PlayerStats
- [ ] Add Component → PlayerAttack
- [ ] Add Component → FacingProvider
- [ ] Add Component → FieldOfView2D
  - View Range: 8
  - Update Rate: 0.1
  - Maze Generator 참조

**프리팹 저장:**
- [ ] `Assets/_Project/Prefabs/Player/Player.prefab`

**필수 컴포넌트 표 (Player):**

| 컴포넌트 | 역할 | 필수 설정 |
|---------|------|-----------|
| SpriteRenderer | 시각화 | Sorting Layer: Objects |
| CircleCollider2D | 충돌 | Radius: 0.4 |
| Rigidbody2D | 물리 | Kinematic, Freeze Rotation Z |
| PlayerController | 이동 | moveSpeed: 5 |
| PlayerStats | 체력/골드 | maxHealth: 100 |
| PlayerAttack | 공격 | attackDamage: 10, attackRange: 1.5 |
| FacingProvider | 방향 | (없음) |
| FieldOfView2D | 시야 | viewRange: 8, mazeGenerator 참조 |

#### 3.2 Enemy 프리팹

**생성:**
- [ ] Hierarchy → 2D Object → Sprite → 이름: `Enemy_Goblin`
- [ ] Sprite Renderer: 임시 스프라이트 (빨간색 원)
- [ ] Sorting Layer: `Objects`, Order: 0
- [ ] Tag: `Enemy`, Layer: `Enemy`

**컴포넌트 추가:**
- [ ] Add Component → Circle Collider 2D
  - Radius: 0.4
- [ ] Add Component → Rigidbody 2D
  - Body Type: Kinematic
  - Constraints: Freeze Rotation Z ✅
- [ ] Add Component → EnemyController
- [ ] Add Component → EnemyAI
- [ ] Add Component → EnemySensing
- [ ] Add Component → EnemyPatrol
- [ ] Add Component → EnemyChase
- [ ] Add Component → EnemyAttack
- [ ] Add Component → Health

**프리팹 저장:**
- [ ] `Assets/_Project/Prefabs/Enemies/Enemy_Goblin.prefab`
- [ ] `Assets/_Project/Prefabs/Enemies/Enemy_Skeleton.prefab` (복제 후 스프라이트만 변경)

**필수 컴포넌트 표 (Enemy):**

| 컴포넌트 | 역할 | 필수 설정 |
|---------|------|-----------|
| SpriteRenderer | 시각화 | Sorting Layer: Objects |
| CircleCollider2D | 충돌 | Radius: 0.4 |
| Rigidbody2D | 물리 | Kinematic, Freeze Rotation Z |
| EnemyController | 메인 컨트롤러 | (런타임 초기화) |
| EnemyAI | FSM | (런타임 초기화) |
| EnemySensing | 감지 | aggroRadius: 7, fovCheck: true |
| EnemyPatrol | 순찰 | moveSpeed: 2, waitTime: 2 |
| EnemyChase | 추적 | chaseSpeed: 3.5, giveUpTime: 5 |
| EnemyAttack | 공격 | attackRange: 1.5, cooldown: 2 |
| Health | 체력 | maxHealth: 30 (런타임 설정) |

---

### D) 통합 중 흔한 에러 10개 + 빠른 해결

#### 1. `NullReferenceException: Object reference not set to an instance of an object`

**원인**: Inspector에서 참조 연결 누락

**해결**:
- MazeGenerator → MazeTilemapPainter 참조 확인
- FieldOfView2D → MazeGenerator 참조 확인
- FogRenderer → Tilemap 참조 확인
- EncounterDirector → SpawnPlanner/PatrolPlanner 참조 확인

#### 2. `Tilemap.SetTile: Tile is null`

**원인**: TilePalette에 타일 할당 안 됨

**해결**:
- `Assets/_Project/Tiles/` 폴더에 FloorTile/WallTile 생성 확인
- MazeTilemapPainter Inspector에서 Tile 드래그 확인

#### 3. `Physics2D.Raycast returns nothing`

**원인**: LayerMask 설정 오류

**해결**:
```csharp
// FieldOfView2D에서
[SerializeField] private LayerMask visionBlockerMask = 1 << 6; // Wall layer
```
- Inspector에서 LayerMask에 `Wall` 체크 확인

#### 4. `Player falls through floor`

**원인**: Tilemap Collider 설정 누락

**해결**:
- WallTilemap에 Tilemap Collider 2D + Composite Collider 2D 확인
- Rigidbody2D → Body Type: Static 확인
- Tilemap Collider 2D → Used By Composite ✅ 확인

#### 5. `Maze not painting`

**원인**: Tilemap bounds 문제 또는 타일 null

**해결**:
- MazeGenerator.Generate() 호출 확인 (autoGenerate ✅)
- 콘솔에 `[MazeGenerator] Generating...` 로그 확인
- Tilemap.SetTile(pos, tile) 전에 tile != null 체크
- Scene View에서 Grid 오브젝트가 (0, 0, 0)에 있는지 확인

#### 6. `Enemy not spawning`

**원인**: EnemyFactory prefab 로드 실패

**해결**:
```csharp
// EnemyFactory.cs
GameObject prefab = Resources.Load<GameObject>(definition.prefabPath);
if (prefab == null)
{
    Debug.LogError($"Prefab not found: {definition.prefabPath}");
    return null;
}
```
- monsters.json에 `"prefabPath": "Prefabs/Enemies/Enemy_Goblin"` 확인
- `Resources/Prefabs/Enemies/Enemy_Goblin.prefab` 경로 확인

#### 7. `JSON file not found`

**원인**: StreamingAssets 경로 오류

**해결**:
- `StreamingAssets/GameData/monsters.json` 경로 확인
- Windows: `Application.streamingAssetsPath + "/GameData/monsters.json"`
- 빌드 후에는 StreamingAssets 폴더가 빌드 폴더에 복사되었는지 확인

#### 8. `Fog not updating`

**원인**: GameEvents 구독 누락

**해결**:
```csharp
// FogOfWarSystem.cs
private void OnEnable()
{
    GameEvents.OnVisionCellsUpdated += OnVisionCellsUpdated;
}

private void OnDisable()
{
    GameEvents.OnVisionCellsUpdated -= OnVisionCellsUpdated;
}
```
- OnDisable에서 구독 해제 확인 (메모리 누수 방지)

#### 9. `Assembly Definition reference error`

**원인**: asmdef 의존성 순환 참조

**해결**:
- 의존성 방향 확인: Runtime → UI → Gameplay → Systems → DataJson → Core
- Core는 다른 모듈 참조 금지
- DataJson은 Core만 참조 가능

#### 10. `Camera not following player`

**원인**: Camera Follow 스크립트 누락 또는 타겟 미설정

**해결**:
```csharp
// CameraFollow.cs (간단 구현)
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }
}
```
- Main Camera에 스크립트 추가 후 Player 참조 설정

---

## 2. JSON 파일 생성/배치 + 로딩/검증 파이프라인

### A) 파일 트리 + 경로

```
StreamingAssets/
└── GameData/
    ├── monsters.json       # 몬스터 정의
    └── encounters.json     # Trap/Loot/Event 정의
```

**경로 접근 (런타임):**

```csharp
// JsonDataLoader.cs
private string GetFullPath(string fileName)
{
    return Path.Combine(Application.streamingAssetsPath, "GameData", fileName);
}

// 예시
// Windows Editor: C:/ProjectPath/Assets/StreamingAssets/GameData/monsters.json
// Windows Build: C:/BuildPath/Labyrinth_Data/StreamingAssets/GameData/monsters.json
```

---

### B) 샘플 JSON 파일

#### monsters.json

```json
{
  "version": "1.0",
  "monsters": [
    {
      "id": "goblin_scout",
      "displayName": "Goblin Scout",
      "archetype": "Melee",
      "prefabPath": "Prefabs/Enemies/Enemy_Goblin",
      "stats": {
        "maxHealth": 30,
        "moveSpeed": 2.0,
        "attackDamage": 5,
        "attackRange": 1.2,
        "detectionRange": 6.0,
        "attackCooldown": 1.5
      },
      "ai": {
        "behavior": "Patrol",
        "aggroRadius": 7.0,
        "chaseSpeed": 3.0,
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
      "prefabPath": "Prefabs/Enemies/Enemy_Skeleton",
      "stats": {
        "maxHealth": 50,
        "moveSpeed": 1.5,
        "attackDamage": 10,
        "attackRange": 1.5,
        "detectionRange": 8.0,
        "attackCooldown": 2.0
      },
      "ai": {
        "behavior": "Aggressive",
        "aggroRadius": 10.0,
        "chaseSpeed": 2.5,
        "giveUpTime": 8.0
      },
      "loot": {
        "goldMin": 10,
        "goldMax": 25,
        "dropChance": 0.5,
        "itemPool": ["health_potion_medium", "rusty_sword"]
      }
    },
    {
      "id": "dark_mage",
      "displayName": "Dark Mage",
      "archetype": "Ranged",
      "prefabPath": "Prefabs/Enemies/Enemy_Mage",
      "stats": {
        "maxHealth": 25,
        "moveSpeed": 1.8,
        "attackDamage": 15,
        "attackRange": 5.0,
        "detectionRange": 10.0,
        "attackCooldown": 3.0
      },
      "ai": {
        "behavior": "Defensive",
        "aggroRadius": 12.0,
        "chaseSpeed": 2.0,
        "giveUpTime": 3.0
      },
      "loot": {
        "goldMin": 20,
        "goldMax": 40,
        "dropChance": 0.7,
        "itemPool": ["mana_potion", "magic_staff"]
      }
    }
  ]
}
```

#### encounters.json

```json
{
  "version": "1.0",
  "encounters": [
    {
      "id": "spike_trap",
      "type": "Trap",
      "displayName": "Spike Trap",
      "description": "Hidden spikes shoot up from the floor!",
      "trapType": "Damage",
      "damageAmount": 15,
      "duration": 0
    },
    {
      "id": "poison_gas",
      "type": "Trap",
      "displayName": "Poison Gas",
      "description": "Toxic fumes fill the corridor!",
      "trapType": "Poison",
      "damageAmount": 5,
      "duration": 10.0
    },
    {
      "id": "treasure_chest",
      "type": "Loot",
      "displayName": "Treasure Chest",
      "description": "A dusty chest filled with gold!",
      "goldAmount": 50,
      "itemReward": null
    },
    {
      "id": "healing_fountain",
      "type": "Loot",
      "displayName": "Healing Fountain",
      "description": "A magical fountain that restores health.",
      "goldAmount": 0,
      "healthRestore": 30
    },
    {
      "id": "mysterious_statue",
      "type": "EventText",
      "displayName": "Mysterious Statue",
      "description": "A stone statue holds out its hands. One hand glows red, the other blue.",
      "choices": [
        {
          "text": "Touch the red hand",
          "result": "The statue crumbles, revealing gold inside!",
          "goldReward": 30,
          "healthPenalty": 0
        },
        {
          "text": "Touch the blue hand",
          "result": "A healing light washes over you.",
          "goldReward": 0,
          "healthRestore": 20
        },
        {
          "text": "Leave it alone",
          "result": "You walk away cautiously.",
          "goldReward": 0,
          "healthPenalty": 0
        }
      ]
    }
  ]
}
```

---

### C) 로딩/검증 최종 호출 흐름

```
[GameRunManager.Start()]
    ↓
1. JsonDataLoader.LoadMonsters()
   - Path.Combine(StreamingAssets, "GameData", "monsters.json")
   - File.ReadAllText(path)
   - JsonUtility.FromJson<MonstersData>(json)
    ↓
2. DataValidator.Validate(monstersData, out errors)
   - ID 중복 검사
   - 필수 필드 검사 (id, displayName, stats, ai)
   - 수치 범위 검사 (maxHealth > 0, moveSpeed >= 0)
   - PrefabPath 존재 여부 검사 (Resources.Load)
    ↓
3. [검증 실패 시]
   - errors 리스트 로그 출력
   - 안전한 기본값 사용 또는 런 중단
    ↓
4. [검증 성공 시]
   EnemyRegistry.Instance.Register(monstersData)
   - Dictionary<string, EnemyDefinition> 생성
   - Debug.Log($"Registered {count} enemies")
    ↓
5. 런타임 사용
   EnemyDefinition def = EnemyRegistry.Instance.Get("goblin_scout")
   EnemyFactory.Create(def, position)
```

**의사코드:**

```csharp
// GameRunManager.cs
void Start()
{
    // 1. JSON 로드
    JsonDataLoader loader = GetComponent<JsonDataLoader>();
    MonstersData monstersData = loader.LoadMonsters();

    if (monstersData == null)
    {
        Debug.LogError("[GameRunManager] Failed to load monsters.json. Aborting run.");
        return; // 런 중단
    }

    // 2. 검증
    List<string> errors;
    if (!DataValidator.Validate(monstersData, out errors))
    {
        Debug.LogError($"[GameRunManager] Validation failed with {errors.Count} errors:");
        foreach (var error in errors)
        {
            Debug.LogError($"  - {error}");
        }

        // 정책: 검증 실패 시 기본 몬스터만 사용
        monstersData = GetFallbackMonsters();
    }

    // 3. 레지스트리 등록
    EnemyRegistry.Instance.Register(monstersData);

    // 4. 미로 생성
    MazeResult maze = mazeGenerator.Generate(seed);

    // 5. EncounterDirector 초기화
    encounterDirector.Initialize(maze, seed);
}

MonstersData GetFallbackMonsters()
{
    // 하드코딩된 안전한 기본 몬스터
    MonstersData fallback = new MonstersData();
    fallback.version = "1.0";
    fallback.monsters = new List<EnemyDefinition>
    {
        new EnemyDefinition
        {
            id = "default_enemy",
            displayName = "Default Enemy",
            archetype = "Melee",
            prefabPath = "Prefabs/Enemies/Enemy_Goblin",
            stats = new EnemyStats { maxHealth = 30, moveSpeed = 2, attackDamage = 5, /* ... */ },
            ai = new EnemyAI { behavior = "Patrol", aggroRadius = 7, /* ... */ },
            loot = new EnemyLoot { goldMin = 5, goldMax = 10, dropChance = 0.3f, itemPool = new List<string>() }
        }
    };
    return fallback;
}
```

---

### D) 실패 시 처리 정책

#### 정책 1: 파일 로드 실패

**상황**: `monsters.json` 파일이 없거나 경로 오류

**처리**:
1. `Debug.LogError` 출력
2. Fallback 몬스터 데이터 사용 (하드코딩)
3. 런 계속 진행 (개발 편의)

**코드**:
```csharp
if (!File.Exists(path))
{
    Debug.LogError($"[JsonDataLoader] File not found: {path}. Using fallback data.");
    return GameRunManager.Instance.GetFallbackMonsters();
}
```

#### 정책 2: JSON 파싱 실패

**상황**: JSON 형식 오류 (괄호 누락, 쉼표 오류 등)

**처리**:
1. `try-catch`로 예외 포착
2. 오류 메시지 로그
3. Fallback 데이터 사용

**코드**:
```csharp
try
{
    MonstersData data = JsonUtility.FromJson<MonstersData>(json);
    return data;
}
catch (System.Exception ex)
{
    Debug.LogError($"[JsonDataLoader] JSON parsing failed: {ex.Message}");
    return GameRunManager.Instance.GetFallbackMonsters();
}
```

#### 정책 3: 검증 실패 (필수 필드 누락, 수치 오류)

**상황**: `DataValidator.Validate()` 실패

**처리**:
1. 모든 에러 리스트 로그 출력
2. **옵션 A**: 문제 있는 몬스터만 제외하고 나머지 사용
3. **옵션 B**: Fallback 데이터 사용 (추천, Init)

**코드**:
```csharp
if (!DataValidator.Validate(monstersData, out errors))
{
    Debug.LogWarning($"[GameRunManager] Validation failed. Using fallback.");
    foreach (var error in errors)
    {
        Debug.LogError($"  - {error}");
    }
    monstersData = GetFallbackMonsters();
}
```

#### 정책 4: Prefab 로드 실패

**상황**: `Resources.Load<GameObject>(prefabPath)` 반환 null

**처리**:
1. `Debug.LogError` 출력
2. 기본 프리팹 사용 (예: Enemy_Goblin)
3. 또는 스폰 스킵

**코드**:
```csharp
// EnemyFactory.cs
GameObject prefab = Resources.Load<GameObject>(definition.prefabPath);
if (prefab == null)
{
    Debug.LogError($"[EnemyFactory] Prefab not found: {definition.prefabPath}. Using default.");
    prefab = Resources.Load<GameObject>("Prefabs/Enemies/Enemy_Goblin");
}
```

#### 정책 5: 런타임 리로드 (DebugPanel)

**상황**: 개발 중 JSON 수정 후 즉시 반영하고 싶음

**처리**:
1. DebugPanel에 "Reload JSON" 버튼
2. 버튼 클릭 시 `JsonDataLoader.LoadMonsters()` 재호출
3. `EnemyRegistry.Instance.Register()` 재등록
4. 기존 스폰된 적은 유지 (새 적은 새 데이터 사용)

**코드**:
```csharp
// DebugPanel.cs
public void OnReloadJSONButtonClicked()
{
    JsonDataLoader loader = FindObjectOfType<JsonDataLoader>();
    MonstersData data = loader.LoadMonsters();

    if (data != null && DataValidator.Validate(data, out _))
    {
        EnemyRegistry.Instance.Register(data);
        Debug.Log("[DebugPanel] JSON reloaded successfully.");
    }
    else
    {
        Debug.LogError("[DebugPanel] JSON reload failed.");
    }
}
```

---

## 3. Enemy AI 구현 (순찰/추적/공격)

### A) 상태 머신 설계

#### FSM 상태 정의

```
States:
- Init        # 초기화 (순찰 경로 설정)
- Patrol      # 순찰 경로 따라 이동
- Chase       # 플레이어 추적
- Attack      # 사거리 내 공격
- Return      # 순찰 경로로 복귀
- Stunned     # 기절 (옵션, 확장용)
```

#### 상태 전이 표

| 현재 상태 | 조건 | 다음 상태 | 비고 |
|----------|------|----------|------|
| **Init** | 항상 | Patrol | 순찰 경로 할당 후 |
| **Patrol** | 플레이어 감지 | Chase | FOV 또는 거리 감지 |
| **Patrol** | - | Patrol | 계속 순찰 |
| **Chase** | 사거리 내 | Attack | attackRange 체크 |
| **Chase** | GiveUpTime 초과 | Return | 추적 실패 |
| **Chase** | 플레이어 계속 보임 | Chase | 경로 갱신 |
| **Attack** | 쿨다운 중 | Attack | 대기 |
| **Attack** | 사거리 밖 | Chase | 다시 추적 |
| **Attack** | 플레이어 사망 | Return | 전투 종료 |
| **Return** | 순찰 경로 도달 | Patrol | 복귀 완료 |
| **Stunned** | stun 시간 종료 | 이전 상태 | 옵션 |

**전이 우선순위**:
1. Stunned (최우선)
2. 플레이어 사망 → Return
3. 사거리 내 → Attack
4. 플레이어 감지 → Chase
5. GiveUpTime 초과 → Return

---

### B) 필요한 컴포넌트/스크립트 목록

#### 3.1 EnemyController (메인 컨트롤러)

**역할**:
- EnemyDefinition 데이터 수신
- 하위 컴포넌트 초기화 (AI, Sensing, Patrol, Chase, Attack)
- Health 초기화
- 사망 처리

**필수 참조**:
- `EnemyAI ai`
- `EnemySensing sensing`
- `EnemyPatrol patrol`
- `EnemyChase chase`
- `EnemyAttack attack`
- `Health health`

#### 3.2 EnemyAI (FSM)

**역할**:
- 상태 관리 (currentState)
- Update()에서 상태별 로직 실행
- 상태 전이 판단

**필수 참조**:
- `EnemyController controller`
- `EnemySensing sensing`

#### 3.3 EnemySensing (감지)

**역할**:
- 플레이어 감지 (FOV 또는 거리)
- 마지막 감지 위치 기록

**필수 참조**:
- `Transform playerTransform`
- `FieldOfView2D playerFOV` (옵션, 양방향 감지)

#### 3.4 EnemyPatrol (순찰)

**역할**:
- PatrolPath 노드 따라 이동
- 노드 도달 시 대기 후 다음 노드

**필수 참조**:
- `Rigidbody2D rb`
- `List<Vector2Int> patrolNodes`

#### 3.5 EnemyChase (추적)

**역할**:
- 플레이어 마지막 위치로 이동
- A* 경로 또는 직선 이동

**필수 참조**:
- `Rigidbody2D rb`
- `GridPathfinder pathfinder`

#### 3.6 EnemyAttack (공격)

**역할**:
- 사거리 내 공격
- 쿨다운 관리
- CombatSystem 호출

**필수 참조**:
- `Transform playerTransform`
- `CombatSystem combatSystem`

---

### C) 코드 스켈레톤

#### EnemyController.cs

```csharp
namespace Game.Gameplay.Enemy
{
    using UnityEngine;
    using Game.DataJson.Schema;
    using Game.Core.Events;

    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Components")]
        private EnemyAI ai;
        private EnemySensing sensing;
        private EnemyPatrol patrol;
        private EnemyChase chase;
        private EnemyAttack attack;
        private Health health;
        private Rigidbody2D rb;

        [Header("Runtime")]
        public EnemyDefinition Definition { get; private set; }
        public Vector2Int SpawnPoint { get; private set; }

        private void Awake()
        {
            ai = GetComponent<EnemyAI>();
            sensing = GetComponent<EnemySensing>();
            patrol = GetComponent<EnemyPatrol>();
            chase = GetComponent<EnemyChase>();
            attack = GetComponent<EnemyAttack>();
            health = GetComponent<Health>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(EnemyDefinition definition, Vector2Int spawnPoint, PatrolPath patrolPath)
        {
            Definition = definition;
            SpawnPoint = spawnPoint;

            // Stats
            health.SetMaxHealth(definition.stats.maxHealth);
            patrol.SetMoveSpeed(definition.stats.moveSpeed);
            chase.SetChaseSpeed(definition.ai.chaseSpeed);
            attack.SetAttackDamage(definition.stats.attackDamage);
            attack.SetAttackRange(definition.stats.attackRange);
            attack.SetAttackCooldown(definition.stats.attackCooldown);

            // AI
            sensing.SetAggroRadius(definition.ai.aggroRadius);
            sensing.SetGiveUpTime(definition.ai.giveUpTime);
            patrol.SetPatrolPath(patrolPath);

            // FSM
            ai.Initialize(this);

            Debug.Log($"[EnemyController] Initialized {definition.displayName} at {spawnPoint}");
        }

        private void OnEnable()
        {
            health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            Debug.Log($"[EnemyController] {Definition.displayName} died");

            // Loot 드랍
            DropLoot();

            // Budget 갱신
            GameEvents.TriggerEnemyDied(gameObject);

            // 오브젝트 파괴 (또는 풀링)
            Destroy(gameObject, 0.5f);
        }

        private void DropLoot()
        {
            if (Definition == null || Definition.loot == null) return;

            float roll = Random.value;
            if (roll < Definition.loot.dropChance)
            {
                int gold = Random.Range(Definition.loot.goldMin, Definition.loot.goldMax + 1);
                // TODO: 실제 골드/아이템 드랍 처리
                Debug.Log($"[EnemyController] Dropped {gold} gold");
            }
        }

        public Rigidbody2D GetRigidbody() => rb;
        public EnemySensing GetSensing() => sensing;
        public EnemyPatrol GetPatrol() => patrol;
        public EnemyChase GetChase() => chase;
        public EnemyAttack GetAttack() => attack;
    }
}
```

#### EnemyAI.cs (FSM)

```csharp
namespace Game.Gameplay.Enemy
{
    using UnityEngine;

    public enum EnemyState
    {
        Init,
        Patrol,
        Chase,
        Attack,
        Return,
        Stunned
    }

    public class EnemyAI : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private EnemyState currentState = EnemyState.Init;

        [Header("Runtime")]
        private EnemyController controller;
        private EnemySensing sensing;
        private EnemyPatrol patrol;
        private EnemyChase chase;
        private EnemyAttack attack;

        public void Initialize(EnemyController ctrl)
        {
            controller = ctrl;
            sensing = ctrl.GetSensing();
            patrol = ctrl.GetPatrol();
            chase = ctrl.GetChase();
            attack = ctrl.GetAttack();

            currentState = EnemyState.Init;
        }

        private void Update()
        {
            switch (currentState)
            {
                case EnemyState.Init:
                    StateInit();
                    break;
                case EnemyState.Patrol:
                    StatePatrol();
                    break;
                case EnemyState.Chase:
                    StateChase();
                    break;
                case EnemyState.Attack:
                    StateAttack();
                    break;
                case EnemyState.Return:
                    StateReturn();
                    break;
                case EnemyState.Stunned:
                    StateStunned();
                    break;
            }
        }

        private void StateInit()
        {
            // 순찰 경로 설정 확인
            if (patrol.HasPath())
            {
                TransitionTo(EnemyState.Patrol);
            }
        }

        private void StatePatrol()
        {
            // 플레이어 감지 체크
            if (sensing.IsPlayerDetected())
            {
                TransitionTo(EnemyState.Chase);
                return;
            }

            // 순찰 이동
            patrol.MoveAlongPath();
        }

        private void StateChase()
        {
            // 사거리 내?
            if (sensing.IsInAttackRange())
            {
                TransitionTo(EnemyState.Attack);
                return;
            }

            // 추적 포기?
            if (sensing.ShouldGiveUp())
            {
                TransitionTo(EnemyState.Return);
                return;
            }

            // 추적 이동
            chase.ChasePlayer(sensing.GetLastKnownPlayerPosition());
        }

        private void StateAttack()
        {
            // 사거리 밖?
            if (!sensing.IsInAttackRange())
            {
                TransitionTo(EnemyState.Chase);
                return;
            }

            // 공격
            attack.TryAttack();
        }

        private void StateReturn()
        {
            // 순찰 경로 복귀
            if (patrol.IsAtSpawnPoint())
            {
                TransitionTo(EnemyState.Patrol);
                return;
            }

            // 복귀 이동
            patrol.ReturnToSpawn();
        }

        private void StateStunned()
        {
            // TODO: Stun 시간 체크
        }

        private void TransitionTo(EnemyState newState)
        {
            Debug.Log($"[EnemyAI] {currentState} -> {newState}");

            // Exit 로직
            switch (currentState)
            {
                case EnemyState.Patrol:
                    patrol.StopPatrol();
                    break;
                case EnemyState.Chase:
                    chase.StopChase();
                    break;
            }

            currentState = newState;

            // Enter 로직
            switch (newState)
            {
                case EnemyState.Patrol:
                    patrol.StartPatrol();
                    break;
                case EnemyState.Chase:
                    sensing.ResetGiveUpTimer();
                    break;
                case EnemyState.Attack:
                    attack.ResetCooldown();
                    break;
            }
        }

        public EnemyState GetCurrentState() => currentState;

        private void OnDrawGizmos()
        {
            if (sensing == null) return;

            // 상태 표시
            Gizmos.color = GetStateColor();
            Gizmos.DrawWireSphere(transform.position, 0.6f);

            // 감지 반경
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, sensing.GetAggroRadius());
        }

        private Color GetStateColor()
        {
            switch (currentState)
            {
                case EnemyState.Patrol: return Color.green;
                case EnemyState.Chase: return Color.yellow;
                case EnemyState.Attack: return Color.red;
                case EnemyState.Return: return Color.cyan;
                default: return Color.white;
            }
        }
    }
}
```

#### EnemySensing.cs

```csharp
namespace Game.Gameplay.Enemy
{
    using UnityEngine;

    public class EnemySensing : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float aggroRadius = 7f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float giveUpTime = 5f;
        [SerializeField] private bool useFOVCheck = true;

        [Header("Runtime")]
        private Transform playerTransform;
        private Vector2 lastKnownPlayerPosition;
        private float lastSeenTime;
        private bool isPlayerDetected;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            CheckPlayerDetection();
        }

        private void CheckPlayerDetection()
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);

            // 거리 체크
            if (dist <= aggroRadius)
            {
                // FOV 체크 (옵션)
                if (useFOVCheck)
                {
                    // TODO: Raycast로 시야 차단 체크
                    Vector2 direction = (playerTransform.position - transform.position).normalized;
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dist, LayerMask.GetMask("Wall"));

                    if (hit.collider == null) // 시야 차단 없음
                    {
                        OnPlayerDetected();
                    }
                    else
                    {
                        isPlayerDetected = false;
                    }
                }
                else
                {
                    OnPlayerDetected();
                }
            }
            else
            {
                isPlayerDetected = false;
            }
        }

        private void OnPlayerDetected()
        {
            isPlayerDetected = true;
            lastKnownPlayerPosition = playerTransform.position;
            lastSeenTime = Time.time;
        }

        public bool IsPlayerDetected() => isPlayerDetected;

        public bool IsInAttackRange()
        {
            if (playerTransform == null) return false;
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            return dist <= attackRange;
        }

        public bool ShouldGiveUp()
        {
            return Time.time - lastSeenTime > giveUpTime;
        }

        public Vector2 GetLastKnownPlayerPosition() => lastKnownPlayerPosition;

        public void ResetGiveUpTimer()
        {
            lastSeenTime = Time.time;
        }

        // Inspector 설정용
        public void SetAggroRadius(float radius) => aggroRadius = radius;
        public void SetGiveUpTime(float time) => giveUpTime = time;
        public float GetAggroRadius() => aggroRadius;
    }
}
```

#### EnemyPatrol.cs

```csharp
namespace Game.Gameplay.Enemy
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Systems.Encounter;

    public class EnemyPatrol : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float waitTime = 2f;
        [SerializeField] private float nodeReachThreshold = 0.2f;

        [Header("Runtime")]
        private List<Vector2Int> patrolNodes;
        private int currentNodeIndex = 0;
        private bool isWaiting = false;
        private float waitTimer = 0f;
        private Rigidbody2D rb;
        private Vector2Int spawnPoint;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void SetPatrolPath(PatrolPath path)
        {
            if (path == null || path.Nodes == null || path.Nodes.Count == 0)
            {
                Debug.LogWarning("[EnemyPatrol] No patrol path assigned");
                return;
            }

            patrolNodes = path.Nodes;
            spawnPoint = path.SpawnPoint.Position;
            currentNodeIndex = 0;
        }

        public bool HasPath() => patrolNodes != null && patrolNodes.Count > 0;

        public void StartPatrol()
        {
            currentNodeIndex = 0;
            isWaiting = false;
        }

        public void StopPatrol()
        {
            rb.velocity = Vector2.zero;
        }

        public void MoveAlongPath()
        {
            if (patrolNodes == null || patrolNodes.Count == 0) return;

            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTime)
                {
                    isWaiting = false;
                    waitTimer = 0f;
                    currentNodeIndex = (currentNodeIndex + 1) % patrolNodes.Count;
                }
                return;
            }

            Vector2Int targetNode = patrolNodes[currentNodeIndex];
            Vector2 targetPos = new Vector2(targetNode.x, targetNode.y);
            Vector2 currentPos = rb.position;

            float dist = Vector2.Distance(currentPos, targetPos);

            if (dist < nodeReachThreshold)
            {
                // 노드 도달
                isWaiting = true;
                rb.velocity = Vector2.zero;
            }
            else
            {
                // 이동
                Vector2 direction = (targetPos - currentPos).normalized;
                rb.velocity = direction * moveSpeed;
            }
        }

        public void ReturnToSpawn()
        {
            Vector2 targetPos = new Vector2(spawnPoint.x, spawnPoint.y);
            Vector2 currentPos = rb.position;
            Vector2 direction = (targetPos - currentPos).normalized;
            rb.velocity = direction * moveSpeed;
        }

        public bool IsAtSpawnPoint()
        {
            Vector2 targetPos = new Vector2(spawnPoint.x, spawnPoint.y);
            float dist = Vector2.Distance(rb.position, targetPos);
            return dist < nodeReachThreshold;
        }

        public void SetMoveSpeed(float speed) => moveSpeed = speed;
    }
}
```

#### EnemyChase.cs

```csharp
namespace Game.Gameplay.Enemy
{
    using UnityEngine;

    public class EnemyChase : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float chaseSpeed = 3.5f;

        [Header("Runtime")]
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void ChasePlayer(Vector2 targetPosition)
        {
            Vector2 currentPos = rb.position;
            Vector2 direction = (targetPosition - currentPos).normalized;

            // 간단 직선 이동 (Phase 3 Init)
            rb.velocity = direction * chaseSpeed;

            // TODO: A* 경로 사용 (확장)
        }

        public void StopChase()
        {
            rb.velocity = Vector2.zero;
        }

        public void SetChaseSpeed(float speed) => chaseSpeed = speed;
    }
}
```

#### EnemyAttack.cs

```csharp
namespace Game.Gameplay.Enemy
{
    using UnityEngine;

    public class EnemyAttack : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("Runtime")]
        private float lastAttackTime = -999f;
        private Transform playerTransform;
        private Health playerHealth;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerHealth = player.GetComponent<Health>();
            }
        }

        public void TryAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown)
                return;

            if (playerTransform == null || playerHealth == null)
                return;

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= attackRange)
            {
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            lastAttackTime = Time.time;

            Debug.Log($"[EnemyAttack] Attacked player for {attackDamage} damage");

            // CombatSystem을 통해 데미지 처리 (Phase 4에서 상세 구현)
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            // TODO: 공격 애니메이션, 사운드
        }

        public void ResetCooldown()
        {
            lastAttackTime = -999f;
        }

        public void SetAttackDamage(int damage) => attackDamage = damage;
        public void SetAttackRange(float range) => attackRange = range;
        public void SetAttackCooldown(float cooldown) => attackCooldown = cooldown;
    }
}
```

---

## 4. 전투 시스템 구현 (피격/사망/디스폰)

### A) 전투 이벤트 흐름

```
[PlayerAttack 발동]
    ↓
1. PlayerAttack.TryAttack()
   - Input 체크 (마우스 클릭 or 키)
   - Cooldown 체크
    ↓
2. OverlapCircle 또는 Raycast로 적 감지
   - attackRange 내 Enemy 태그 오브젝트 찾기
    ↓
3. CombatSystem.ApplyDamage(attacker, target, damage)
   - GameEvents.OnDamageDealt 발행
    ↓
4. Target.Health.TakeDamage(damage)
   - currentHealth -= damage
   - GameEvents.OnHealthChanged 발행
    ↓
5. [Health <= 0?]
   YES:
   - Health.Die()
   - GameEvents.OnDeath 발행
   - EnemyController.HandleDeath()
     - Loot 드랍
     - EncounterBudget.OnEnemyDespawned()
     - Destroy() 또는 Pool 반환
   NO:
   - 계속 진행
```

**양방향 전투:**
- PlayerAttack → EnemyHealth
- EnemyAttack → PlayerHealth

---

### B) 코드 스켈레톤

#### IDamageable.cs

```csharp
namespace Game.Core.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(int amount);
        void Heal(int amount);
        bool IsDead();
    }
}
```

#### Health.cs

```csharp
namespace Game.Gameplay.Combat
{
    using UnityEngine;
    using System;
    using Game.Core.Interfaces;
    using Game.Core.Events;

    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] private int maxHealth = 100;

        [Header("Runtime")]
        [SerializeField] private int currentHealth;

        public event Action OnDeath;
        public event Action<int, int> OnHealthChanged; // (current, max)

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void SetMaxHealth(int value)
        {
            maxHealth = value;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead()) return;

            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);

            Debug.Log($"[Health] {gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            GameEvents.TriggerHealthChanged(gameObject, currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead()) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            GameEvents.TriggerHealthChanged(gameObject, currentHealth, maxHealth);
        }

        private void Die()
        {
            Debug.Log($"[Health] {gameObject.name} died");

            OnDeath?.Invoke();
            GameEvents.TriggerDeath(gameObject);
        }

        public bool IsDead() => currentHealth <= 0;

        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public float GetHealthPercent() => (float)currentHealth / maxHealth;
    }
}
```

#### CombatSystem.cs

```csharp
namespace Game.Gameplay.Combat
{
    using UnityEngine;
    using Game.Core.Events;

    public class CombatSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool enableKnockback = false;
        [SerializeField] private float knockbackForce = 5f;

        public void ApplyDamage(GameObject attacker, GameObject target, int damage)
        {
            if (target == null)
            {
                Debug.LogWarning("[CombatSystem] Target is null");
                return;
            }

            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth == null)
            {
                Debug.LogWarning($"[CombatSystem] {target.name} has no Health component");
                return;
            }

            // 데미지 처리
            targetHealth.TakeDamage(damage);

            // 이벤트 발행
            GameEvents.TriggerDamageDealt(attacker, target, damage);

            // 넉백 (옵션)
            if (enableKnockback)
            {
                ApplyKnockback(attacker, target);
            }

            Debug.Log($"[CombatSystem] {attacker.name} dealt {damage} damage to {target.name}");
        }

        private void ApplyKnockback(GameObject attacker, GameObject target)
        {
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb == null) return;

            Vector2 direction = (target.transform.position - attacker.transform.position).normalized;
            targetRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
```

#### PlayerAttack.cs

```csharp
namespace Game.Gameplay.Player
{
    using UnityEngine;
    using Game.Gameplay.Combat;

    public class PlayerAttack : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private LayerMask enemyLayer = 1 << 9; // Enemy layer

        [Header("References")]
        [SerializeField] private CombatSystem combatSystem;

        [Header("Runtime")]
        private float lastAttackTime = -999f;

        private void Start()
        {
            if (combatSystem == null)
            {
                combatSystem = FindObjectOfType<CombatSystem>();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                TryAttack();
            }
        }

        private void TryAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown)
                return;

            lastAttackTime = Time.time;

            // 공격 범위 내 적 탐색
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Enemy"))
                    {
                        PerformAttack(hit.gameObject);
                    }
                }
            }

            Debug.Log($"[PlayerAttack] Attacked, hit {hits.Length} enemies");
        }

        private void PerformAttack(GameObject target)
        {
            if (combatSystem != null)
            {
                combatSystem.ApplyDamage(gameObject, target, attackDamage);
            }

            // TODO: 공격 애니메이션, 사운드, VFX
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
```

---

### C) EncounterDirector 연동 포인트

#### GameEvents 확장

```csharp
// Core/Events/GameEvents.cs
namespace Game.Core.Events
{
    using System;
    using UnityEngine;

    public static class GameEvents
    {
        // Combat Events
        public static event Action<GameObject, GameObject, int> OnDamageDealt; // (attacker, target, damage)
        public static event Action<GameObject> OnDeath; // (deadObject)
        public static event Action<GameObject, int, int> OnHealthChanged; // (object, current, max)

        // Enemy Events
        public static event Action<GameObject> OnEnemyDied; // (enemy)

        // Trigger Methods
        public static void TriggerDamageDealt(GameObject attacker, GameObject target, int damage)
            => OnDamageDealt?.Invoke(attacker, target, damage);

        public static void TriggerDeath(GameObject deadObject)
            => OnDeath?.Invoke(deadObject);

        public static void TriggerHealthChanged(GameObject obj, int current, int max)
            => OnHealthChanged?.Invoke(obj, current, max);

        public static void TriggerEnemyDied(GameObject enemy)
            => OnEnemyDied?.Invoke(enemy);
    }
}
```

#### EncounterDirector 수정

```csharp
// Systems/Encounter/EncounterDirector.cs
namespace Game.Systems.Encounter
{
    using UnityEngine;
    using Game.Core.Events;

    public class EncounterDirector : MonoBehaviour
    {
        // ... 기존 코드 ...

        private void OnEnable()
        {
            GameEvents.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= HandleEnemyDied;
        }

        private void HandleEnemyDied(GameObject enemy)
        {
            // Budget 감소
            budget.OnEnemyDespawned();

            // SpawnPoint 정리
            foreach (var sp in spawnPoints)
            {
                if (sp.SpawnedEnemy == enemy)
                {
                    sp.SpawnedEnemy = null;
                    sp.IsActive = false;
                    Debug.Log($"[EncounterDirector] Spawn point {sp.Position} cleared");
                    break;
                }
            }

            Debug.Log($"[EncounterDirector] Enemy died. Active: {budget.currentActiveEnemies}/{budget.maxConcurrentEnemies}");
        }
    }
}
```

---

## 5. DebugPanel UI 구현

### A) UI 레이아웃 (텍스트 와이어프레임)

```
┌─────────────────────────────────────────┐
│ DEBUG PANEL                        [X]  │ <- Toggle 버튼으로 On/Off
├─────────────────────────────────────────┤
│ [Toggles]                               │
│  ☑ Show Spawn Points                    │
│  ☑ Show Patrol Paths                    │
│  ☐ Show FOV Cells                       │
│  ☐ Show Fog Cells                       │
├─────────────────────────────────────────┤
│ [Metrics]                               │
│  Seed: 12345                            │
│  Active Enemies: 3 / 5                  │
│  Total Spawned: 8 / 30                  │
│  Exploration: 45.2%                     │
│  Budget Cooldown: 2.3s                  │
├─────────────────────────────────────────┤
│ [Actions]                               │
│  [Regenerate Maze]                      │
│  [Force Spawn Enemy]                    │
│  [Reset Fog]                            │
│  [Reload JSON]                          │
└─────────────────────────────────────────┘
```

**위치**: 화면 우상단, Canvas (Screen Space - Overlay)

---

### B) DebugPanel 스크립트 스켈레톤

#### DebugPanel.cs

```csharp
namespace Game.UI.HUD
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using Game.Systems.Maze;
    using Game.Systems.Encounter;
    using Game.Systems.FogOfWar;
    using Game.DataJson.Loader;
    using Game.DataJson.Registry;

    public class DebugPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button toggleButton;

        [Header("Toggles")]
        [SerializeField] private Toggle spawnPointsToggle;
        [SerializeField] private Toggle patrolPathsToggle;
        [SerializeField] private Toggle fovCellsToggle;
        [SerializeField] private Toggle fogCellsToggle;

        [Header("Metrics")]
        [SerializeField] private TextMeshProUGUI seedText;
        [SerializeField] private TextMeshProUGUI activeEnemiesText;
        [SerializeField] private TextMeshProUGUI totalSpawnedText;
        [SerializeField] private TextMeshProUGUI explorationText;
        [SerializeField] private TextMeshProUGUI budgetCooldownText;

        [Header("Actions")]
        [SerializeField] private Button regenerateButton;
        [SerializeField] private Button forceSpawnButton;
        [SerializeField] private Button resetFogButton;
        [SerializeField] private Button reloadJsonButton;

        [Header("References")]
        [SerializeField] private MazeGenerator mazeGenerator;
        [SerializeField] private EncounterDirector encounterDirector;
        [SerializeField] private FogOfWarSystem fogSystem;
        [SerializeField] private JsonDataLoader jsonLoader;

        private bool isPanelVisible = true;

        private void Start()
        {
            // Toggle 리스너
            toggleButton.onClick.AddListener(TogglePanel);
            spawnPointsToggle.onValueChanged.AddListener(OnSpawnPointsToggled);
            patrolPathsToggle.onValueChanged.AddListener(OnPatrolPathsToggled);
            fovCellsToggle.onValueChanged.AddListener(OnFovCellsToggled);
            fogCellsToggle.onValueChanged.AddListener(OnFogCellsToggled);

            // Action 버튼 리스너
            regenerateButton.onClick.AddListener(OnRegenerateClicked);
            forceSpawnButton.onClick.AddListener(OnForceSpawnClicked);
            resetFogButton.onClick.AddListener(OnResetFogClicked);
            reloadJsonButton.onClick.AddListener(OnReloadJsonClicked);

            // 초기 상태
            panelRoot.SetActive(isPanelVisible);
        }

        private void Update()
        {
            UpdateMetrics();

            // 단축키 (F1)
            if (Input.GetKeyDown(KeyCode.F1))
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            isPanelVisible = !isPanelVisible;
            panelRoot.SetActive(isPanelVisible);
        }

        private void UpdateMetrics()
        {
            if (!isPanelVisible) return;

            // Seed
            if (mazeGenerator != null)
            {
                MazeResult maze = mazeGenerator.GetCurrentMaze();
                if (maze != null)
                {
                    seedText.text = $"Seed: {maze.Seed}";
                }
            }

            // Budget
            if (encounterDirector != null)
            {
                var budget = encounterDirector.GetBudget();
                if (budget != null)
                {
                    activeEnemiesText.text = $"Active Enemies: {budget.currentActiveEnemies} / {budget.maxConcurrentEnemies}";
                    totalSpawnedText.text = $"Total Spawned: {budget.totalSpawned} / {budget.totalSpawnBudget}";

                    float cooldown = Mathf.Max(0, budget.encounterCooldown - (Time.time - budget.lastEncounterTime));
                    budgetCooldownText.text = $"Budget Cooldown: {cooldown:F1}s";
                }
            }

            // Exploration
            if (fogSystem != null)
            {
                float progress = fogSystem.GetExplorationProgress();
                explorationText.text = $"Exploration: {progress * 100:F1}%";
            }
        }

        // Toggle Callbacks
        private void OnSpawnPointsToggled(bool isOn)
        {
            // TODO: Gizmos 표시 토글 (EncounterDirector.drawSpawnPoints)
            Debug.Log($"[DebugPanel] Spawn points: {isOn}");
        }

        private void OnPatrolPathsToggled(bool isOn)
        {
            // TODO: Gizmos 표시 토글
            Debug.Log($"[DebugPanel] Patrol paths: {isOn}");
        }

        private void OnFovCellsToggled(bool isOn)
        {
            // TODO: FOV 셀 시각화 토글
            Debug.Log($"[DebugPanel] FOV cells: {isOn}");
        }

        private void OnFogCellsToggled(bool isOn)
        {
            // TODO: Fog 셀 시각화 토글
            Debug.Log($"[DebugPanel] Fog cells: {isOn}");
        }

        // Action Callbacks
        private void OnRegenerateClicked()
        {
            if (mazeGenerator != null)
            {
                int newSeed = Random.Range(0, int.MaxValue);
                mazeGenerator.Generate(newSeed);
                Debug.Log($"[DebugPanel] Regenerated maze with seed {newSeed}");
            }
        }

        private void OnForceSpawnClicked()
        {
            if (encounterDirector != null)
            {
                encounterDirector.ForceSpawnEnemy();
                Debug.Log("[DebugPanel] Forced enemy spawn");
            }
        }

        private void OnResetFogClicked()
        {
            if (fogSystem != null)
            {
                fogSystem.ResetFog();
                Debug.Log("[DebugPanel] Fog reset");
            }
        }

        private void OnReloadJsonClicked()
        {
            if (jsonLoader != null)
            {
                var monstersData = jsonLoader.LoadMonsters();
                if (monstersData != null)
                {
                    EnemyRegistry.Instance.Register(monstersData);
                    Debug.Log("[DebugPanel] JSON reloaded");
                }
                else
                {
                    Debug.LogError("[DebugPanel] JSON reload failed");
                }
            }
        }
    }
}
```

---

### C) UI 프리팹 생성 가이드

1. **Canvas 생성**
   - Hierarchy → UI → Canvas
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080

2. **Panel Root**
   - Canvas 자식 → UI → Panel
   - 이름: `DebugPanel`
   - Anchor: Top-Right
   - Width: 400, Height: 600
   - Position: (-220, -320, 0)

3. **Header**
   - Text: "DEBUG PANEL"
   - Font Size: 24, Bold

4. **Toggles Section**
   - Vertical Layout Group
   - 4개 Toggle: Spawn Points, Patrol Paths, FOV Cells, Fog Cells

5. **Metrics Section**
   - 5개 TextMeshPro: Seed, Active Enemies, Total Spawned, Exploration, Budget Cooldown

6. **Actions Section**
   - 4개 Button: Regenerate, Force Spawn, Reset Fog, Reload JSON

7. **Toggle Button (F1)**
   - Canvas 자식 → UI → Button
   - Anchor: Top-Right
   - Text: "[F1] Debug"

---

## 6. 성능 최적화 + 테스트/밸런싱

### A) 성능 최적화

#### 6.1 Fog/Tilemap 업데이트 최적화

**문제**: 매 프레임 전체 Tilemap 업데이트 시 성능 저하

**해결: 변경된 셀만 업데이트 (Diff)**

```csharp
// FogRenderer.cs
namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using UnityEngine.Tilemaps;
    using System.Collections.Generic;

    public class FogRenderer : MonoBehaviour
    {
        [SerializeField] private Tilemap fogTilemap;
        [SerializeField] private TileBase unexploredTile;
        [SerializeField] private TileBase exploredTile;

        private HashSet<Vector2Int> previousVisibleCells = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> previousVisitedCells = new HashSet<Vector2Int>();

        public void Render(FogOfWarGrid grid)
        {
            HashSet<Vector2Int> currentVisibleCells = grid.GetVisibleCells();
            HashSet<Vector2Int> currentVisitedCells = grid.GetVisitedCells();

            // 변경된 셀만 업데이트
            UpdateChangedCells(currentVisibleCells, currentVisitedCells);

            // 이전 상태 저장
            previousVisibleCells = new HashSet<Vector2Int>(currentVisibleCells);
            previousVisitedCells = new HashSet<Vector2Int>(currentVisitedCells);
        }

        private void UpdateChangedCells(HashSet<Vector2Int> visibleCells, HashSet<Vector2Int> visitedCells)
        {
            // 1. 이전에 visible이었지만 지금은 아닌 셀 → explored로 변경
            foreach (var cell in previousVisibleCells)
            {
                if (!visibleCells.Contains(cell))
                {
                    Vector3Int pos = new Vector3Int(cell.x, cell.y, 0);
                    fogTilemap.SetTile(pos, exploredTile);
                }
            }

            // 2. 새로 visible된 셀 → 타일 제거
            foreach (var cell in visibleCells)
            {
                if (!previousVisibleCells.Contains(cell))
                {
                    Vector3Int pos = new Vector3Int(cell.x, cell.y, 0);
                    fogTilemap.SetTile(pos, null); // 밝게
                }
            }

            // 3. 새로 visited된 셀 (처음 탐색) → unexplored 제거
            foreach (var cell in visitedCells)
            {
                if (!previousVisitedCells.Contains(cell))
                {
                    // 이미 visible 처리에서 제거됨
                }
            }
        }
    }
}
```

**추가 최적화: SetTilesBlock 사용**

```csharp
// 대량 셀 업데이트 시
Vector3Int[] positions = changedCells.Select(c => new Vector3Int(c.x, c.y, 0)).ToArray();
TileBase[] tiles = new TileBase[positions.Length];
for (int i = 0; i < tiles.Length; i++)
{
    tiles[i] = exploredTile;
}
fogTilemap.SetTiles(positions, tiles);
```

#### 6.2 FOV updateRate 조절

```csharp
// FieldOfView2D.cs
[Header("Performance")]
[SerializeField] private float updateRate = 0.1f; // 10 FPS (0.1초마다)
[SerializeField] private int viewRange = 8; // 범위 제한

private float updateTimer = 0f;

private void Update()
{
    updateTimer += Time.deltaTime;
    if (updateTimer >= updateRate)
    {
        updateTimer = 0f;
        UpdateVision();
    }
}
```

#### 6.3 A* 캐싱/재사용

**문제**: 매 프레임 A* 경로 계산 비용

**해결: 경로 캐싱 + 유효성 체크**

```csharp
// PathCache.cs
namespace Game.Systems.Pathfinding
{
    using UnityEngine;
    using System.Collections.Generic;

    public class PathCache
    {
        private List<Vector2Int> cachedPath;
        private Vector2Int cachedTarget;
        private float cacheTime;
        private float cacheLifetime = 0.5f; // 0.5초 유효

        public List<Vector2Int> GetPath(Vector2Int start, Vector2Int target, GridPathfinder pathfinder)
        {
            // 캐시 유효성 체크
            if (cachedPath != null && cachedTarget == target && Time.time - cacheTime < cacheLifetime)
            {
                return cachedPath;
            }

            // 새 경로 계산
            cachedPath = pathfinder.FindPath(start, target);
            cachedTarget = target;
            cacheTime = Time.time;

            return cachedPath;
        }

        public void Invalidate()
        {
            cachedPath = null;
        }
    }
}

// EnemyChase.cs 수정
private PathCache pathCache = new PathCache();

public void ChasePlayer(Vector2 targetPosition)
{
    Vector2Int start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    Vector2Int target = new Vector2Int(Mathf.RoundToInt(targetPosition.x), Mathf.RoundToInt(targetPosition.y));

    List<Vector2Int> path = pathCache.GetPath(start, target, pathfinder);

    if (path != null && path.Count > 1)
    {
        Vector2 nextNode = new Vector2(path[1].x, path[1].y);
        Vector2 direction = (nextNode - (Vector2)transform.position).normalized;
        rb.velocity = direction * chaseSpeed;
    }
    else
    {
        // 캐시 실패 시 직선 이동
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = direction * chaseSpeed;
    }
}
```

#### 6.4 오브젝트 풀링 (간단 구현)

```csharp
// SimplePoolManager.cs
namespace Game.Gameplay.Pooling
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Core.Interfaces;

    public class SimplePoolManager : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int initialSize = 10;
        }

        [SerializeField] private List<Pool> pools = new List<Pool>();

        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (var pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.initialSize; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[Pool] Tag {tag} not found");
                return null;
            }

            GameObject obj = poolDictionary[tag].Dequeue();

            obj.SetActive(true);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            IPoolable poolable = obj.GetComponent<IPoolable>();
            poolable?.OnSpawn();

            poolDictionary[tag].Enqueue(obj);

            return obj;
        }

        public void Despawn(GameObject obj)
        {
            IPoolable poolable = obj.GetComponent<IPoolable>();
            poolable?.OnDespawn();

            obj.SetActive(false);
        }
    }
}
```

#### 6.5 ProfilerMarker 측정 포인트

```csharp
using Unity.Profiling;

// FieldOfView2D.cs
private static readonly ProfilerMarker s_UpdateVisionMarker = new ProfilerMarker("FieldOfView2D.UpdateVision");

private void UpdateVision()
{
    s_UpdateVisionMarker.Begin();

    // ... 기존 로직 ...

    s_UpdateVisionMarker.End();
}

// FogRenderer.cs
private static readonly ProfilerMarker s_RenderMarker = new ProfilerMarker("FogRenderer.Render");

// GridPathfinder.cs
private static readonly ProfilerMarker s_FindPathMarker = new ProfilerMarker("GridPathfinder.FindPath");
```

**Profiler 확인**:
- Window → Analysis → Profiler
- CPU Usage → Deep Profile
- 마커 검색: "FieldOfView2D", "FogRenderer", "GridPathfinder"

---

### B) 테스트 체크리스트

#### 1. Seed 재현성 테스트

- [ ] 동일 Seed로 2회 생성 → 미로 구조 동일
- [ ] 동일 Seed로 2회 생성 → 스폰 포인트 위치 동일
- [ ] 동일 Seed로 2회 생성 → 순찰 경로 동일
- [ ] DebugPanel에서 Seed 표시 확인
- [ ] Seed 변경 시 미로/스폰/순찰 모두 변경 확인

#### 2. JSON 검증 테스트

**Case 1: 정상 JSON**
- [ ] monsters.json 로드 성공
- [ ] 콘솔: "Registered X enemies"
- [ ] EnemyRegistry.Get("goblin_scout") 정상 반환

**Case 2: 파일 없음**
- [ ] StreamingAssets/GameData/monsters.json 삭제
- [ ] 콘솔: "File not found" 에러
- [ ] Fallback 데이터 사용 확인

**Case 3: JSON 파싱 오류**
- [ ] monsters.json 괄호 누락
- [ ] 콘솔: "JSON parsing failed" 에러
- [ ] Fallback 데이터 사용 확인

**Case 4: 검증 실패 (필수 필드 누락)**
- [ ] monsters.json에서 "id" 필드 제거
- [ ] 콘솔: "Validation failed" + 상세 에러
- [ ] Fallback 데이터 사용 확인

**Case 5: Prefab 로드 실패**
- [ ] prefabPath를 존재하지 않는 경로로 변경
- [ ] 콘솔: "Prefab not found" 에러
- [ ] 기본 프리팹 사용 확인

#### 3. AI 상태 전이 테스트

- [ ] Patrol → Chase: 플레이어 접근 시 추적 시작
- [ ] Chase → Attack: 사거리 내 진입 시 공격 시작
- [ ] Attack → Chase: 사거리 밖으로 이탈 시 다시 추적
- [ ] Chase → Return: GiveUpTime(5초) 후 순찰 복귀
- [ ] Return → Patrol: 순찰 경로 도달 시 정상 순찰 재개
- [ ] Gizmos 색상 변화 확인 (Patrol: 녹색, Chase: 노란색, Attack: 빨간색)

#### 4. 전투 사망 처리 테스트

**플레이어 → 적**
- [ ] 적 공격 시 Health 감소 확인
- [ ] Health 0 도달 시 사망 처리
- [ ] Budget.currentActiveEnemies 감소 확인
- [ ] SpawnPoint.IsActive = false 확인
- [ ] Loot 드랍 확인 (골드)

**적 → 플레이어**
- [ ] 플레이어 피격 시 Health 감소 확인
- [ ] HUD Health Bar 업데이트 확인
- [ ] Health 0 도달 시 게임 오버 처리

#### 5. EncounterBudget 동작 테스트

- [ ] maxConcurrentEnemies(5) 도달 시 추가 스폰 중단
- [ ] 적 사망 시 카운트 감소 후 새 적 스폰 가능
- [ ] encounterCooldown(5초) 동안 스폰 차단
- [ ] totalSpawnBudget(30) 도달 시 더 이상 스폰 없음
- [ ] DebugPanel에서 Budget 현황 실시간 업데이트 확인

#### 6. CorridorTrigger 1회 보장 테스트

- [ ] Trigger 진입 시 1회만 발동
- [ ] 재진입 시 발동 안 함
- [ ] hasTriggered 플래그 확인
- [ ] DebugPanel에서 Reset 시 재발동 가능 (개발용)

#### 7. FOV/Fog 통합 테스트

- [ ] 플레이어 이동 시 FOV 업데이트 (0.1초마다)
- [ ] Visible 셀에 Fog 제거 확인
- [ ] Explored 셀에 반투명 Fog 표시 확인
- [ ] Unexplored 셀에 어두운 Fog 표시 확인
- [ ] 벽 뒤 셀은 Visible 안 됨 확인
- [ ] 탐색률(Exploration) 증가 확인

---

### C) 밸런싱 가이드

#### 기본 밸런스 테이블 (Init)

| 항목 | 추천값 | 비고 |
|------|--------|------|
| **Maze** |
| Grid Size | 41x41 | 홀수, 너무 크면 탐색 지루 |
| deadEndRemovalRate | 0.3 | 30% 제거, 루프 적당 |
| **Player** |
| Max Health | 100 | |
| Move Speed | 5 | |
| Attack Damage | 10 | |
| Attack Range | 1.5 | |
| Attack Cooldown | 0.5s | |
| **Enemy (Goblin)** |
| Max Health | 30 | 공격 3회 |
| Move Speed | 2.0 | 플레이어보다 느림 |
| Attack Damage | 5 | 체력 20회 |
| Attack Range | 1.2 | 플레이어보다 짧음 |
| Aggro Radius | 7.0 | |
| Chase Speed | 3.0 | 플레이어보다 느림 (도망 가능) |
| Give Up Time | 5.0s | |
| **Enemy (Skeleton)** |
| Max Health | 50 | 공격 5회 |
| Move Speed | 1.5 | 느림 |
| Attack Damage | 10 | 체력 10회 |
| Attack Range | 1.5 | 플레이어와 동일 |
| Aggro Radius | 10.0 | 더 넓음 |
| Chase Speed | 2.5 | 느림 |
| Give Up Time | 8.0s | 더 오래 추적 |
| **Budget** |
| maxConcurrentEnemies | 5 | 동시 적 수 |
| maxEnemiesInRadius | 2 | 반경 10칸 내 |
| totalSpawnBudget | 30 | 런 전체 |
| encounterCooldown | 5.0s | |
| **FOV/Fog** |
| viewRange | 8 | 타일 단위 |
| updateRate | 0.1s | 10 FPS |

**밸런싱 팁:**
- 플레이어 이동속도 > 적 추적속도 → 도망 가능
- 플레이어 공격력 × 3 = 약한 적 체력 → 3타 처치
- 적 공격력 × 10~20 = 플레이어 체력 → 10~20타 견딤
- Aggro Radius < viewRange → 적 먼저 발견 가능

---

### D) Phase 3 Done 기준 (DoD) 12개

1. **미로 생성**: Seed 기반 41x41 미로 생성, start-exit 보장
2. **JSON 로드**: monsters.json 정상 로드 → EnemyRegistry 등록
3. **스폰 배치**: 교차로 우선 5~15개 스폰 포인트 배치
4. **Enemy AI**: Patrol → Chase → Attack → Return 상태 전이 정상 작동
5. **전투 시스템**: 플레이어/적 피격/사망 처리 정상
6. **Budget 제한**: maxConcurrentEnemies/totalSpawnBudget/cooldown 동작
7. **FOV**: Shadowcasting 기반 시야, 벽 Occlusion 정확
8. **Fog**: 미탐색/탐색/현재시야 3단계 명확히 구분
9. **DebugPanel**: 토글/메트릭스/액션 버튼 모두 정상 작동
10. **Seed 재현성**: 동일 Seed → 동일 미로/스폰/순찰
11. **성능**: 60 FPS 유지 (41x41 미로, 5마리 동시 활성)
12. **플레이 가능**: 시작 → 탐색 → 조우 → 전투 → 탈출/사망 전체 플로우 완성

---

### E) 성능/리스크 체크리스트 12개

| 항목 | 리스크 | 대응 방안 |
|------|--------|-----------|
| **Shadowcasting 비용** | viewRange 크면 셀 순회 과다 | viewRange 8~10, updateRate 0.1s |
| **Fog Tilemap 업데이트** | 매 프레임 전체 셀 업데이트 시 GC | 변경된 셀만 diff, SetTiles 배치 |
| **A* 경로 계산** | 매 프레임 계산 시 CPU 부하 | PathCache로 0.5초 캐싱 |
| **JSON 파싱** | 큰 파일 로드 시 프리징 | 코루틴 비동기 로드 (확장) |
| **오브젝트 생성/파괴** | GC Spike | SimplePoolManager로 풀링 |
| **스폰 포인트 과다** | 너무 많은 적 동시 활성화 | maxConcurrentEnemies=5 cap |
| **Tilemap Collider** | CompositeCollider 없으면 콜라이더 과다 | TilemapCollider + Composite 필수 |
| **미로 크기** | 61x61 이상 시 생성 지연 | 41x41 기본, 최대 61x61 제한 |
| **JSON 검증 누락** | 잘못된 데이터로 런타임 오류 | DataValidator 필수 호출 |
| **EnemyFactory Prefab 로드** | Resources.Load 동기 지연 | Awake/Start 사전 로드 |
| **Gizmos 과다** | OnDrawGizmos 비용 | DebugPanel 토글로 On/Off |
| **이벤트 구독 누수** | OnDisable 해제 누락 시 메모리 누수 | 모든 GameEvents 구독 해제 확인 |

---

### F) 다음 확장 로드맵 6개

| 순서 | 확장 항목 | 설명 |
|------|-----------|------|
| 1 | **다층 던전** | 층별 미로 생성, 난이도 곡선, 계단 연결, JSON 테이블 층별 분리 |
| 2 | **바이옴 시스템** | TilePalette 바이옴별, 시각적 다양성, 특수 규칙 (용암 데미지 등) |
| 3 | **아이템/장비 시스템** | items.json, 장비 슬롯, 능력치 보정, 빌드 다양성 |
| 4 | **BehaviorTree AI** | 복잡한 패턴 (원거리/마법/소환), 협동 AI, 상태 트리 |
| 5 | **RunMetrics & Replay** | 탐색률/조우 수/시간 기록, 입력 녹화/재생, 리더보드 |
| 6 | **메타 진행** | 런 종료 후 언락, 영구 업그레이드, 스토리 진행, 도전 과제 |

---

## 마무리

### 통합 완료 확인 순서

1. [ ] Assembly Definition 모두 생성 및 참조 확인
2. [ ] Run 씬 하이어라키 구성 완료
3. [ ] Tilemap/Collider/Layer/Sorting 설정 완료
4. [ ] Player/Enemy 프리팹 생성 및 컴포넌트 추가
5. [ ] StreamingAssets/GameData/monsters.json 생성
6. [ ] GameRunManager 참조 연결 완료
7. [ ] DebugPanel UI 생성 및 연결
8. [ ] Play 버튼 → 미로 생성 → 플레이어 스폰 확인
9. [ ] 적 스폰 → AI Patrol → Chase → Attack 확인
10. [ ] 전투 → 사망 → Budget 갱신 확인
11. [ ] FOV/Fog 업데이트 확인
12. [ ] DebugPanel 모든 기능 테스트

### 다음 단계 (Phase 4 예정)

Phase 3에서 완성한 "플레이 가능한 1층 런"을 바탕으로:
- 다층 던전 (층별 난이도)
- 바이옴 시스템 (시각적 다양성)
- 아이템/장비 시스템
- 메타 진행 (영구 업그레이드)
- 완전한 게임으로 확장

---

이상으로 **Phase 3 Response**를 완료합니다.
