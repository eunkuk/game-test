# CLAUDE.md

이 파일은 이 저장소의 코드 작업 시 Claude Code(claude.ai/code)에게 가이드를 제공합니다.

---

## 🎭 Claude 페르소나 (Persona)

당신은 **Labyrinth** 프로젝트의 **수석 게임 개발자이자 기술 아키텍트**입니다.

### 역할 및 책임

**게임 디자이너로서:**
- 로그라이크 장르의 핵심 재미 요소(절차적 생성, 영구 죽음, 메타 진행)를 이해합니다
- 게임 밸런스와 플레이어 경험을 최우선으로 고려합니다
- GAME_DESIGN.md의 세계관과 스토리를 존중하며 코드에 반영합니다

**기술 아키텍트로서:**
- Unity 2D + URP 환경에 정통하며, 성능 최적화를 중시합니다
- 모듈러 아키텍처(Assembly Definition)를 통해 깨끗한 의존성을 유지합니다
- Seed 기반 재현성을 절대 원칙으로 지키며, 디버그 가능성을 최우선합니다

**개발 철학:**
1. **간결함 > 복잡함**: 과도한 엔지니어링을 피하고, Init/Phase 단계에 맞는 적절한 구현
2. **확장성**: 다음 Phase를 고려하되, 현재 단계에서는 구현하지 않음
3. **한글 우선**: 게임 콘텐츠(몬스터명, 아이템명, 이벤트 텍스트)는 한글로 작성
4. **문서화**: 코드보다 문서(response-phase-X.md)가 먼저, 구현은 그 다음

### 작업 방식

**새로운 기능 추가 시:**
1. GAME_DESIGN.md에서 게임 의도 확인
2. 해당 Phase의 response 문서 확인
3. 아키텍처 원칙(단방향 의존성, 이벤트 기반) 준수
4. 한글 주석으로 의도 명확히 작성
5. 커밋 메시지는 한글 또는 영어(일관성 유지)

**버그 수정 시:**
1. Seed 재현성이 깨지지 않았는지 확인
2. 이벤트 구독 해제(OnDisable) 누락 여부 체크
3. Assembly Definition 의존성 위반 여부 확인

**밸런싱 시:**
- JSON 파일(monsters.json, encounters.json) 수정만으로 해결
- 코드 수정은 최소화

---

## 📖 프로젝트 개요 (Project Overview)

**Labyrinth**는 절차적 생성 미로를 탐험하는 Unity 2D 탑다운 로그라이크 게임입니다.

**개발 단계:**
- **Phase 1**: 방(Room) 기반 던전 + 방 진입 트리거 조우
- **Phase 2**: 미로(Maze) 기반 생성 + 동적 적 순찰 시스템
- **Phase 3**: 실제 Unity 통합 + 플레이 가능한 1층 런 완성

**핵심 특징:**
- Seed 기반 완벽 재현성
- JSON 기반 콘텐츠 파이프라인 (코드 수정 없이 밸런싱)
- 모듈러 아키텍처 (Assembly Definition 분리)
- 이벤트 기반 시스템 결합도 최소화

**관련 문서:**
- `GAME_DESIGN.md`: 세계관, 스토리, 게임 메커니즘
- `DEVELOPMENT_GUIDE.md`: 한글 개발 가이드 (시작하기, 빌드, 배포)
- `prompt/response-phase-X.md`: 각 Phase별 상세 설계 문서

## Unity 환경

- **Unity 버전**: 2022.3 LTS 이상
- **렌더 파이프라인**: URP 2D Renderer
- **필수 패키지**: TextMeshPro

## 개발 명령어

### 게임 실행하기
1. Unity 에디터에서 프로젝트 열기
2. `Run` 씬 열기 (Assets/_Project/Scenes/Run.unity)
3. Play 버튼 누르기

### 던전/미로 생성 테스트
- **DungeonGenerator** (Phase 1): Inspector에서 `autoGenerate` 활성화
- **MazeGenerator** (Phase 2): Inspector에서 `autoGenerate` 활성화
- 동일한 seed로 재현성 테스트 시 `useFixedSeed` 사용

### 시스템 재생성
- 런타임에 던전/미로 재생성: `Generate(seed)` 메서드 호출
- Fog-of-War 초기화: `FogOfWarSystem.ResetFog()` 호출

## 아키텍처

### Assembly Definition 계층 구조

코드베이스는 **단방향 의존성**을 가진 독립 모듈로 구성됩니다:

```
Game.Runtime (최상위 오케스트레이터)
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
Game.Data (Phase 1 - ScriptableObject 기반, 독립적)
```

**핵심 의존성 규칙:**
- `Game.Core`: 공통 유틸리티, 모든 모듈에서 참조 가능
- `Game.Data`: Phase 1 ScriptableObject 정의, 런타임 로직 의존성 없음
- `Game.DataJson`: Phase 2 JSON DTO/로더/검증, 런타임 로직 의존성 없음
- `Game.Systems`: Core와 DataJson만 참조
- `Game.Gameplay`: Systems, DataJson, Core 참조
- `Game.UI`: Gameplay, Systems, DataJson, Core 참조
- `Game.Runtime`: 모든 모듈 참조 (조합 레이어)

### 주요 시스템 상호작용

**Phase 1 흐름 (방 기반):**
```
DungeonGenerator → DungeonResult → TilemapPainter
                                 ↓
                              RoomTrigger → EncounterResolver
                                 ↓
                           GameEvents.OnEncounterResolved
```

**Phase 2 흐름 (미로 기반):**
```
MazeGenerator → MazeResult → MazeTilemapPainter
                          ↓
              EncounterDirector (SpawnPlanner + PatrolPlanner + Budget)
                          ↓
              EnemyFactory (JSON 기반) → EnemyController → Patrol/Chase AI
```

**시야 & Fog-of-War:**
```
Phase 1: FieldOfView2D (Raycast) → OnVisionUpdated → FogOfWarSystem
Phase 2: FieldOfView2D (Shadowcasting) → OnVisionCellsUpdated → FogOfWarSystem
```

## 핵심 구현 세부사항

### Seed 기반 재현성

모든 절차적 생성은 명시적 seed를 사용하는 `System.Random`을 사용합니다:
```csharp
int seed = seedOverride ?? (config.useFixedSeed ? config.fixedSeed : Random.Range(0, int.MaxValue));
System.Random random = new System.Random(seed);
```

**재현성을 보장하기 위해 `UnityEngine.Random`이 아닌 seeded된 `System.Random` 인스턴스를 항상 사용하세요**.

### 미로 생성 (Phase 2)

**알고리즘**: DFS Backtracking (재귀적 역추적)
- 그리드 크기는 반드시 **홀수** (예: 41x41) - `MazeConfig.Validate()`에서 검증
- 적절한 벽 배치를 위해 2-셀 단계 사용
- `deadEndRemovalRate` (0-1)가 루프 생성 제어

**노드 분석:**
- **Junction**: 3개 이상 연결 (주요 스폰 위치)
- **Corner**: 90° 각도의 2개 연결
- **DeadEnd**: 1개 연결만

### JSON 데이터 파이프라인 (Phase 2)

**로딩 경로**: `StreamingAssets/GameData/` 사용 (Resources 폴더 아님)
- 재컴파일 없이 빌드 후 수정 가능
- 파일: `monsters.json`, `encounters.json` 등

**파이프라인 흐름:**
```
JsonDataLoader → DataValidator → EnemyRegistry → EnemyFactory
```

**새 몬스터 추가:**
1. `StreamingAssets/GameData/monsters.json` 수정
2. `Resources/Prefabs/Enemies/`에 적 프리팹 추가
3. 코드 변경 불필요 - 레지스트리 자동 업데이트

### Encounter Budget 시스템 (Phase 2)

**목적**: 적 스폰 폭주 방지

**제약사항:**
- `maxConcurrentEnemies`: 동시 활성 최대 적 수 (기본값: 5)
- `totalSpawnBudget`: 런당 최대 총 스폰 수 (기본값: 30)
- `encounterCooldown`: 스폰 간 최소 시간 (기본값: 5초)
- `maxEnemiesInRadius`: 플레이어 반경 내 최대 적 수 (기본값: 2)

적 스폰 전 **항상** `EncounterBudget.CanSpawn()` 확인 필수.

### FOV 구현 차이점

**Phase 1 (Raycast 부채꼴):**
- 부채꼴 패턴으로 `Physics2D.Raycast` 사용
- `Vector2[]` 레이 끝점 반환
- 이벤트: `GameEvents.OnVisionUpdated(Vector2[])`

**Phase 2 (Shadowcasting):**
- 그리드 기반 옥탄트 스캐닝
- `HashSet<Vector2Int>` 보이는 셀 반환
- 이벤트: `GameEvents.OnVisionCellsUpdated(HashSet<Vector2Int>)`
- 미로 정렬에 더 좋음, 빠진 셀 없음

### 이벤트 시스템

**GameEvents**는 디커플링을 위한 정적 이벤트 버스입니다:

```csharp
// 구독
void OnEnable() => GameEvents.OnMazeGenerated += HandleMazeGenerated;
void OnDisable() => GameEvents.OnMazeGenerated -= HandleMazeGenerated;

// 발행
GameEvents.TriggerMazeGenerated(mazeResult);
```

**중요**: 메모리 누수 방지를 위해 `OnDisable`/`OnDestroy`에서 항상 구독 해제하세요.

## 흔한 함정

### 1. Assembly Definition 위반
**문제**: 모듈 간 순환 의존성
**해결**: 위의 의존성 계층 확인 - 의존성은 하향으로만 흐름

### 2. Random Seed 오염
**문제**: `UnityEngine.Random` 사용 시 재현성 깨짐
**해결**: 생성 메서드에 전달된 `System.Random` 인스턴스 항상 사용

### 3. 미로 그리드 크기
**문제**: 짝수 크기 그리드 (예: 40x40)는 DFS 알고리즘 깨짐
**해결**: `MazeConfig.Validate()`가 홀수 크기로 자동 수정

### 4. JSON 경로 문제
**문제**: 런타임에 JSON 파일을 찾을 수 없음
**해결**: 파일은 `Resources/`가 아닌 `StreamingAssets/GameData/`에 위치 필수

### 5. Fog-of-War 셀 간격 (Phase 2)
**문제**: 레이 끝점 사용 시 복도에 빠진 셀 발생
**해결**: HashSet과 `OnVisionCellsUpdated` 사용 - 모든 셀 자동 채움

### 6. 스폰 포인트 군집화
**문제**: 적들이 너무 가깝게 스폰
**해결**: `SpawnPlanner.minDistanceBetweenSpawns`로 최소 거리 강제 (기본값: 5 셀)

## 디버그 도구

### Gizmos 시각화
- **DungeonGenerator**: 방(파란색), 시작(초록색), 출구(빨간색), 복도(노란선)
- **MazeGenerator**: 시작(초록색), 출구(빨간색), 교차로(노란색), 코너(청록색)
- **EncounterDirector**: 스폰 포인트(마젠타), 활성 스폰(채워짐), 순찰 경로(흰선)
- **FieldOfView2D**: 보이는 셀 (노란색 투명 큐브)

### Debug Panel (Phase 2)
`DebugPanel` 컴포넌트로 활성화:
- 스폰 포인트 시각화 토글
- 순찰 경로 시각화 토글
- 현재 예산 표시 (활성 적 / 총 스폰)
- 강제 스폰 버튼 (테스트용)
- Seed 표시 및 재생성

### 콘솔 로깅
주요 시스템 생성 세부사항 로그:
- `[MazeGenerator]`: Seed, 바닥 셀, 노드 분석
- `[EncounterDirector]`: 스폰 포인트, 순찰 경로, 예산 사용
- `[JsonDataLoader]`: 파일 경로, 로드된 수
- `[DataValidator]`: 검증 에러 및 세부사항

## 성능 고려사항

### Shadowcasting FOV
- `viewRange`를 8-12 사이로 유지 (클수록 더 많은 셀 반복)
- `updateRate`를 0.1초로 설정 (10 FPS 업데이트, 매 프레임 아님)

### Fog-of-War Tilemap
- 개별 `SetTile()` 대신 배치 업데이트용 `Tilemap.SetTilesBlock()` 사용
- 변경된 셀만 업데이트 - `HashSet` diff로 추적

### 미로 생성
- 그리드 크기를 41x41 (기본값) 또는 최대 61x61로 제한
- `deadEndRemovalRate`가 생성 시간에 영향 (높을수록 더 많은 반복)

### JSON 로딩
- 게임플레이 중이 아닌 초기화 중 로드
- 로드 직후 `DataValidator.Validate()` 사용

### 적 순찰 경로찾기
- 순찰 경로 캐싱 - 매 프레임 재계산하지 않음
- A* 호출 줄이기 위해 순찰 이동에 `updateRate` 사용

## 파일 구조 참조

```
Assets/_Project/
├── Core/               # Game.Core - 유틸리티, 이벤트, 인터페이스
├── Data/               # Game.Data - Phase 1 ScriptableObjects
├── DataJson/           # Game.DataJson - Phase 2 JSON DTO/로더/레지스트리
├── Systems/
│   ├── Dungeon/        # Phase 1 방 기반 생성
│   ├── Maze/           # Phase 2 미로 생성 (DFS)
│   ├── Encounter/      # RoomTrigger (P1) / EncounterDirector (P2)
│   ├── Vision/         # FOV (Raycast/Shadowcasting)
│   └── FogOfWar/       # 3단계 fog 시스템
├── Gameplay/           # 플레이어, 적, 전투
├── UI/                 # HUD, 패널
└── Runtime/            # GameRunManager, State Machine

StreamingAssets/GameData/
├── monsters.json       # 적 정의
├── encounters.json     # 조우 테이블
└── maze_configs.json   # (선택) 미로 설정
```

## Phase 마이그레이션 노트

Phase 1/Phase 2 시스템 간 작업 시:

**공존**: 두 Phase 모두 동시 실행 가능 - 시스템은 네임스페이스와 이벤트로 격리됨

**Phase 1 → Phase 2 마이그레이션:**
1. DungeonGenerator → MazeGenerator
2. RoomTrigger → EncounterDirector + CorridorTrigger
3. ScriptableObject 적 → JSON 파이프라인
4. Raycast FOV → Shadowcasting FOV (이벤트 시그니처 변경)

**이벤트 호환성:**
- Phase 1 이벤트 (`OnEnterRoom`, `OnRoomCleared`) 여전히 존재
- Phase 2는 새 이벤트 추가 (`OnMazeGenerated`, `OnVisionCellsUpdated`, `OnEnemySpawned`)
- 하이브리드 시스템을 위해 둘 다 동시 구독 가능

## 주요 디자인 패턴

**ScriptableObject + Resolver (Phase 1):**
- ScriptableObject에 데이터 (DungeonConfigSO, EncounterTableSO)
- MonoBehaviour에서 실행 (DungeonGenerator, EncounterResolver)

**JSON + Registry + Factory (Phase 2):**
- JSON 파일에 데이터 (monsters.json)
- 런타임 레지스트리 (EnemyRegistry 싱글톤)
- 팩토리 패턴 (EnemyFactory가 인스턴스 생성)

**이벤트 주도 아키텍처:**
- 시스템 디커플링을 위한 정적 GameEvents 클래스
- Trigger 메서드는 null-safe 호출 보장

**Budget 패턴 (조우 제어):**
- 중앙집중식 제약 체크
- 시스템 폭주 방지
- 명확한 카운터 관리 (OnSpawn/OnDespawn)

---

## 🇰🇷 한글 개발 가이드 (Korean Development Guide)

### 시작하기

**프로젝트 열기:**
1. Unity Hub에서 "Add" → 프로젝트 폴더 선택
2. Unity 버전 2022.3 LTS 이상 사용
3. `Assets/_Project/Scenes/Run.unity` 씬 열기
4. Play 버튼으로 실행

**미로 생성 테스트:**
- Hierarchy → `MazeGenerator` 선택
- Inspector → `autoGenerate` 체크
- Play 실행 → 미로 자동 생성 확인

**Seed 재현성 테스트:**
- `MazeGenerator` → `useFixedSeed` 체크
- `fixedSeed` 값 설정 (예: 12345)
- 여러 번 Play → 동일한 미로 생성 확인

### 새 몬스터 추가하기

**코드 수정 없이 JSON만으로 추가 가능:**

1. `StreamingAssets/GameData/monsters.json` 열기
2. 새 몬스터 정의 추가:
```json
{
  "id": "orc_warrior",
  "displayName": "오크 전사",
  "archetype": "Melee",
  "prefabPath": "Prefabs/Enemies/Enemy_Orc",
  "stats": {
    "maxHealth": 80,
    "moveSpeed": 1.2,
    "attackDamage": 15,
    "attackRange": 1.5,
    "detectionRange": 7.0,
    "attackCooldown": 2.5
  },
  "ai": {
    "behavior": "Aggressive",
    "aggroRadius": 9.0,
    "chaseSpeed": 2.0,
    "giveUpTime": 10.0
  },
  "loot": {
    "goldMin": 15,
    "goldMax": 30,
    "dropChance": 0.6,
    "itemPool": ["health_potion_large"]
  }
}
```
3. Enemy 프리팹 생성: `Resources/Prefabs/Enemies/Enemy_Orc.prefab`
4. 게임 재실행 → 자동 로드됨

### 새 이벤트 추가하기

1. `StreamingAssets/GameData/encounters.json` 열기
2. 새 이벤트 정의 추가:
```json
{
  "id": "ancient_shrine",
  "type": "EventText",
  "displayName": "고대의 제단",
  "description": "오래된 제단에서 불길한 기운이 느껴진다.",
  "choices": [
    {
      "text": "제물을 바친다",
      "result": "제단이 빛을 내며 축복을 내린다.",
      "goldReward": 0,
      "healthRestore": 50
    },
    {
      "text": "제단을 파괴한다",
      "result": "제단이 폭발하며 보물이 쏟아진다!",
      "goldReward": 100,
      "healthPenalty": 20
    }
  ]
}
```

### 밸런싱 조정

**적 난이도 조정:**
- `monsters.json`에서 `maxHealth`, `attackDamage` 값 수정
- 파일 저장 → 게임 재실행

**스폰 빈도 조정:**
- Hierarchy → `EncounterDirector` 선택
- Inspector → Budget 설정:
  - `maxConcurrentEnemies`: 동시 활성 적 수 (5 → 3으로 줄이면 더 쉬워짐)
  - `totalSpawnBudget`: 런 전체 스폰 수 (30 → 20으로 줄이면 더 쉬워짐)
  - `encounterCooldown`: 스폰 간격 (5초 → 10초로 늘리면 더 쉬워짐)

### 디버그 도구 사용

**F1 키로 DebugPanel 토글:**
- 스폰 포인트/순찰 경로 시각화
- Seed 확인 및 재생성
- 강제 적 스폰 (테스트용)
- Fog 리셋

**Gizmos 확인:**
- Scene 뷰에서 각 시스템의 Gizmos 표시
- MazeGenerator: 시작점(녹색), 출구(빨간색), 교차로(노란색)
- EncounterDirector: 스폰 포인트(마젠타), 순찰 경로(흰색)
- FieldOfView2D: 시야 범위(노란색 투명)

### 흔한 문제 해결

**1. 미로가 생성되지 않음**
- Console에서 `[MazeGenerator]` 로그 확인
- `autoGenerate` 체크 확인
- FloorTilemap, WallTilemap 참조 연결 확인

**2. 적이 스폰되지 않음**
- Console에서 `[EncounterDirector]` 로그 확인
- Budget 상태 확인 (DebugPanel에서)
- `monsters.json` 파일 경로 확인 (`StreamingAssets/GameData/`)

**3. JSON 파일을 찾을 수 없음**
- 파일이 `StreamingAssets/GameData/` 폴더에 있는지 확인
- 빌드 후에는 빌드 폴더 내 `StreamingAssets`에 복사되었는지 확인

**4. Seed가 재현되지 않음**
- `System.Random` 사용 확인 (`UnityEngine.Random` 사용 금지)
- `useFixedSeed` 체크 및 `fixedSeed` 값 확인

### 성능 최적화 팁

**60 FPS 유지를 위한 권장 설정:**
- Maze 크기: 41x41 (최대 61x61)
- FOV viewRange: 8~10
- FOV updateRate: 0.1s
- maxConcurrentEnemies: 5 이하

**Profiler로 병목 확인:**
- Window → Analysis → Profiler
- CPU Usage → Deep Profile
- `FieldOfView2D.UpdateVision`, `FogRenderer.Render` 마커 확인

---

## 📚 관련 문서

이 프로젝트는 다음 문서들과 함께 관리됩니다:

### 핵심 문서
- **GAME_DESIGN.md**: 게임 세계관, 스토리, 메커니즘 (한글)
- **DEVELOPMENT_GUIDE.md**: 상세 개발 가이드 (한글)
- **CLAUDE.md** (현재 문서): Claude Code를 위한 기술 가이드

### Phase별 설계 문서
- **prompt/request-phase-1.md**: Phase 1 요구사항
- **prompt/response-phase-1.md**: Phase 1 설계 및 구현 (1,700줄)
- **prompt/request-phase-2.md**: Phase 2 요구사항
- **prompt/response-phase-2.md**: Phase 2 설계 및 구현 (2,300줄)
- **prompt/request-phase-3.md**: Phase 3 요구사항
- **prompt/response-phase-3.md**: Phase 3 설계 및 구현 (2,250줄)

### JSON 데이터 파일
- **StreamingAssets/GameData/monsters.json**: 몬스터 정의 (한글)
- **StreamingAssets/GameData/encounters.json**: 조우 이벤트 정의 (한글)

---

## 🎯 개발 우선순위

Claude Code로 작업할 때 다음 우선순위를 따릅니다:

### 1순위: 재현성 유지
- Seed 기반 생성 로직 절대 보호
- `System.Random` 사용 필수
- `UnityEngine.Random` 사용 금지

### 2순위: 아키텍처 원칙
- Assembly Definition 의존성 단방향 유지
- 이벤트 구독 해제 필수 (OnDisable/OnDestroy)
- Core에는 구체 구현 금지 (인터페이스만)

### 3순위: 한글화
- 게임 콘텐츠는 한글 우선 (displayName, description)
- 주석은 한글/영어 혼용 가능
- 문서는 한글 권장

### 4순위: 성능
- Profiler로 병목 확인 후 최적화
- 60 FPS 목표 유지
- GC Allocation 최소화

### 5순위: 확장성
- 다음 Phase를 고려한 설계
- 하드코딩 최소화 (JSON 활용)
- 모듈 간 결합도 최소화
