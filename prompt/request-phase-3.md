너는 Unity 2D(탑다운) 로그라이크의 테크리드/아키텍트야.

목표(Phase 3):
- Phase 2에서 설계한 **MazeGenerator + EncounterDirector + JSON(DataJson) + Shadowcasting FOV + Fog-of-War(셀 채우기)** 를
  **실제 Unity 프로젝트에 통합**해서 “플레이 가능한 1층 런(시작→탐색→조우→탈출/사망→결과)”을 완성한다.
- Phase 3에서 해야 할 핵심 작업은 다음 범위를 반드시 포함한다:
  1) 실제 Unity 프로젝트 통합(Phase 1 → Phase 2 마이그레이션)
  2) JSON 파일 생성/배치(monsters.json 등)
  3) Enemy AI(순찰/추적/공격)
  4) 전투 시스템(피격/사망/디스폰)
  5) DebugPanel UI(스폰 포인트 토글, Budget/Seed 표시)
  6) 성능 최적화(Tilemap 배치 업데이트, A* 캐싱 등)
  7) 테스트 및 밸런싱

엔진/제약:
- Unity LTS(2022/2023) + URP 2D, 2D 탑다운.
- Seed 기반 재현성(동일 Seed → 동일 미로/스폰/순찰 경로).
- 확장성(다층/바이옴/테이블/메타 진행) 고려하되, Phase 3에서는 “눈으로 확인되는 결과” 우선.
- 코드는 **Unity C# 기준 컴파일 가능한 형태**(네임스페이스/using/MonoBehaviour/SerializeField 포함)로 제공.
- 가능하면 Phase 1 코드(FOV/Fog/EventBus 등) 재사용. 단, Phase 2 구조(Maze/JSON) 기준으로 접점만 수정.

반드시 아래 순서로 출력해라:
0) “전체 그림(아키텍처)” + 씬/프리팹 구성 + asmdef 의존성 + 런타임 흐름(Title->Run->Result) + 통합 전략(점진적 마이그레이션)
1) [8] 실제 Unity 프로젝트 통합 가이드 (폴더/asmdef 생성, 씬/Tilemap/Collider/Layer/Sorting/Tags, 프리팹 와이어링 체크리스트)
2) [6] JSON 파일 생성/배치 + 로딩/검증 파이프라인 마무리 (StreamingAssets 기준, 샘플 monsters.json/encounters.json 제공)
3) [8] Enemy AI 구현 (순찰→추적→공격 FSM, 감지(FOV/aggro), 경로(A* 또는 그리드 추종), 디버그 Gizmos)
4) [7] 전투 시스템 구현 (IDamageable/Health, CombatSystem 이벤트, 근접 피격 판정, 사망/드랍/디스폰, EncounterBudget 연동)
5) [5] DebugPanel UI 구현 (토글: 스폰 포인트/순찰 경로/FOV 셀/포그 셀, Budget/Seed/탐색률 표시, 강제 스폰/재생성 버튼)
6) [6] 성능 최적화 + 테스트/밸런싱
  - 성능: Tilemap SetTilesBlock/변경분 diff, FOV updateRate, A* 캐싱, 오브젝트 풀링(최소 인터페이스+간단 구현), JSON 로딩(코루틴) 등
  - 테스트: 재현성/JSON 검증/AI 상태전이/전투 사망 처리/예산(cap) 동작 체크
    마지막에:
  - Phase 3 Done 기준(DoD) 12개
  - 성능/리스크 체크리스트 12개
  - 다음 확장 로드맵 6개
    를 제공해라.

========================================================
0) 전체 그림(아키텍처) 요구사항
- Phase 2 모듈 구성을 유지한다: Game.Core / Game.DataJson / Game.Systems / Game.Gameplay / Game.UI / Game.Runtime
- 의존성 방향은 단방향으로. 특히 DataJson은 런타임 로직 의존 금지.
- Run 씬 오브젝트 구성(예시):
  - GameRunManager (Runtime): seed 선택/로드, 데이터 로드, 미로 생성, 플레이어 스폰, EncounterDirector 초기화
  - MazeGenerator + MazeTilemapPainter (Systems)
  - FieldOfView2D + VisionCellFiller + FogOfWarSystem + FogRenderer (Systems)
  - Player(Controller/Stats/Attack) (Gameplay)
  - EncounterDirector(SpawnPlanner/PatrolPlanner/Budget) (Systems)
  - DebugPanel(Canvas) (UI)
- “점진적 마이그레이션” 전략을 제시해라:
  - DungeonGenerator를 즉시 삭제하지 말고, 런타임 플래그로 Dungeon/Maze를 스위치 가능하게(옵션).
  - Phase 1의 인터페이스(EventBus, IEncounterResolver 등)는 유지하되 이벤트 payload만 Phase 2 데이터와 호환되게 확장.

========================================================
1) [8] 실제 Unity 프로젝트 통합 가이드
   요구:
- 새 프로젝트 생성(URP 2D)부터 Run 씬에서 “미로+포그+FOV+조우+AI+전투”가 실제로 돌아가는 최소 셋업까지,
  **체크리스트 형태**로 제시해라.
- 다음 항목을 반드시 포함:
  - Sorting Layer/Order, Tilemap Collider + CompositeCollider2D, Physics2D Layer Matrix, Nav/Path(그리드 기반)
  - Player/Enemy 프리팹 구성(필수 컴포넌트 표), Tag/Layer 규칙
  - GameRunManager 인스펙터 레퍼런스 연결표(어떤 컴포넌트를 어디에 할당)
  - 디버깅 루틴(Seed 고정, Gizmos, 로그 레벨)
    출력:
    A) 프로젝트/패키지/레이어/정렬 세팅 체크리스트
    B) Run 씬 하이어라키 예시(텍스트)
    C) 프리팹 와이어링 표(Player/Enemy/Director/Fog)
    D) 통합 중 흔한 에러 10개 + 빠른 해결(NullRef, 좌표계, Tilemap bounds 등)

========================================================
2) [6] JSON 파일 생성/배치 + 로딩/검증 마무리
   요구:
- StreamingAssets/GameData 아래에 배치하는 전제.
- 최소 샘플:
  - monsters.json (2~3종, 근접/원거리 1개씩, AI 파라미터 포함)
  - encounters.json (Trap/Loot/EventText 최소 예시)
- DataValidator에서 체크해야 할 룰(필수키, 수치 범위, prefabPath 존재 여부 정책)을 명시.
- 런타임에서 “리로드(옵션)” 지원: DebugPanel에서 Reload 버튼으로 재로딩 가능(개발 편의).
  출력:
  A) 파일 트리 + 경로
  B) 샘플 JSON 2개
  C) JsonDataLoader/Registry/Factory의 “최종” 호출 흐름(순서도/의사코드)
  D) 실패 시 처리 정책(안전한 기본값/에러 로그/런 중단 여부)

========================================================
3) [8] Enemy AI 구현 (순찰/추적/공격)
   요구:
- FSM 기반(Init), 상태: Patrol, Chase, Attack, Return(복귀), Stunned(옵션).
- 감지:
  - FOV 셀(Shadowcasting 결과) + aggroRadius 둘 다 지원(둘 중 하나만 켜도 동작).
- 이동:
  - Patrol: PatrolPlanner가 만든 노드 루프를 따라 이동
  - Chase: 플레이어 마지막 위치로 추적(간단 A* 또는 그리드 직선 + 코너 보정)
  - GiveUpTime 후 Return
- 공격:
  - 공격 쿨다운/사거리/피격 판정
- 디버그:
  - Gizmos로 감지 반경/현재 상태/경로 표시
    출력:
    A) 상태 머신 설계(전이 표)
    B) 필요한 컴포넌트/스크립트 목록
    C) 코드 스켈레톤: EnemyController, EnemyAI(FSM), EnemySensing, PatrolMover, ChaseMover

========================================================
4) [7] 전투 시스템 구현 (피격/사망/디스폰)
   요구:
- 최소 구현:
  - IDamageable + Health
  - PlayerAttack(근접 hitbox 또는 OverlapCircle) + EnemyAttack
  - CombatSystem: 데미지 이벤트 발행, 사망 처리(EventBus), 넉백(옵션)
- 사망 처리:
  - EncounterDirector budget 감소 + SpawnPoint 정리 + (옵션) loot 드랍
- 데이터 연동:
  - EnemyFactory가 EnemyDefinition.stats/ai 값을 EnemyController에 주입
    출력:
    A) 전투 이벤트 흐름(누가 누구를 때릴 때 어떤 이벤트가 나가는지)
    B) 코드 스켈레톤: IDamageable, Health, CombatSystem, PlayerAttack, EnemyAttack
    C) EncounterDirector 연동 포인트(메서드/이벤트)

========================================================
5) [5] DebugPanel UI
   요구:
- uGUI(Canvas) 기반으로 빠르게.
- 토글/표시:
  - 스폰 포인트/순찰 경로/FOV 셀/Fog 셀
  - Budget 현황, Seed, 탐색률(RunMetrics), 현재 활성 적 수
- 버튼:
  - Seed 재생성(Generate), JSON Reload(옵션), 강제 스폰(테스트), 포그 리셋
    출력:
    A) UI 레이아웃(텍스트 와이어프레임)
    B) DebugPanel 스크립트 스켈레톤 + 연결 방법

========================================================
6) [6] 성능 최적화 + 테스트/밸런싱
   요구:
- 성능:
  - Fog/Tilemap 업데이트는 변경된 셀만(diff) 또는 SetTilesBlock 배치.
  - FOV updateRate(0.1s 등) / viewRange 제한.
  - A* 캐싱/재사용(노드 그래프/코너 포인트).
  - Pooling: IPoolable + 간단 PoolManager(Init 수준).
  - ProfilerMarker로 FOV/Fog/Pathing 측정 포인트 제시.
- 테스트:
  - Seed 재현성(미로/스폰/순찰)
  - JSON 검증 실패 케이스
  - AI 상태 전이(추적/포기/복귀)
  - 전투(피격/사망/예산 감소)
  - 트리거 1회 보장(CorridorTrigger)
    마지막에:
- Phase 3 DoD 12개
- 성능/리스크 체크리스트 12개
- 다음 확장 로드맵 6개
