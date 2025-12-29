너는 Unity 2D(탑다운) 로그라이크의 테크리드/아키텍트야.

목표(Phase 2):
- “방 기반”이 아니라 **미로(코리더/교차로/막다른길) 기반**으로 1층을 생성한다.
- 조우는 “방 진입 트리거”가 아니라 **길목에서 실제로 마주칠지 말지**(시야/FOV, 순찰, 소리/유인, 랜덤 스폰 배치)로 결정된다.
- 몬스터 추가는 **ScriptableObject 없이 JSON만**으로 한다. 즉, 몬스터를 추가/수정하려면 `monsters.json`만 편집하면 된다.
- 기존 Phase 1 시스템(FOV occlusion, Fog-of-War 3단계, Seed 재현성, EncounterTable 개념)은 유지하되 “미로 진행”에 맞게 재구성한다.
- 엔진/제약: Unity LTS(2022/2023) + URP 2D, 2D 탑다운, Seed 기반 재현성, 확장성(바이옴/층/테이블/메타 진행), 테스트/디버그 용이.

반드시 아래 순서로 출력해라:
0) “전체 그림(아키텍처)” + 폴더/asmdef + 의존성 방향(단방향) + 런타임 흐름(Title->Run->Result)
1) [6’] 미로 생성(MazeGenerator) 설계/구현 (Room 기반 생성은 사용하지 말 것)
2) [7’] 길목 조우 시스템(EncounterDirector) 설계/구현 (RoomTrigger 사용 금지)
3) 몬스터 JSON 파이프라인 설계/구현 (JSON 스키마 + 로더/검증 + Registry + EnemyFactory 바인딩)
4) [3] FOV 구현 방식 2안 비교 후 Init 추천안 선택 (미로에 최적)
5) [4] 선택한 FOV 방식 구현(오브젝트 구성 + 코드 스켈레톤) + Fog가 재사용할 데이터 형태 정의
6) [5] Fog-of-War 3단계 구현(미탐색/탐색/현재시야) — “레이 끝점만 찍기” 금지, 셀을 채우는 방식 제시
   마지막에:
- Phase 2 Done 기준(DoD) 10개
- 성능/리스크 체크리스트 10개
- 다음 확장 로드맵 6개
  를 제공해라.

========================================================
0) 전체 그림(아키텍처) 요구사항
- Unity 폴더 구조 + Assembly Definition(asmdef) 기반 모듈 분리안을 제시해라.
  필수 모듈:
    - Game.Core (시드/랜덤, 이벤트, 인터페이스, 유틸)
    - Game.DataJson (JSON DTO 스키마, 로더/검증)
    - Game.Systems (미로 생성, EncounterDirector, FOV, Fog, Replay/Metrics)
    - Game.Gameplay (플레이어, 적 AI, 전투)
    - Game.UI (HUD, Debug 패널, 이벤트 선택지 UI)
    - Game.Runtime (GameRunManager, 상태머신)
- 의존성 방향은 단방향으로. DataJson은 런타임 로직 의존 금지.
- 런타임 흐름:
  Run 씬에서 GameRunManager가 runSeed 설정 -> MazeGenerator.Generate(seed) -> TilemapPainter 페인팅 -> Player spawn(start) -> EncounterDirector가 교차로/코너/긴 복도 끝에 스폰/이벤트 배치 -> 플레이어 진행 중 실제 조우 발생 -> exit 도달 또는 사망 -> Result.
- EventBus/C# 이벤트로 시스템 결합도 낮출 것:
  예: OnMazeGenerated, OnEnemySpawned, OnVisionUpdated, OnFogUpdated, OnEncounterEventTriggered.
- ObjectPool은 인터페이스만 준비하고 Init에서는 Instantiate로 둬도 된다.
- 코드 스켈레톤은 Unity C# 기준 컴파일 가능한 형태(네임스페이스/using 포함)로 작성해라.

========================================================
1) [6’] 미로 생성(MazeGenerator)
   요구:
- Grid 기반 DFS 백트래킹 미로 생성(Init 추천) 또는 다른 알고리즘 비교 후 선택.
- width/height는 홀수 추천(예: 41x41), start->exit 연결 보장.
- deadEndRemoval 파라미터로 루프량 조절(옵션).
- Tilemap에 바닥/벽 페인팅.
- Wall 타일맵은 TilemapCollider2D + CompositeCollider2D 최적화 권장.
- Seed 기반 재현성.
  출력:
  A) 알고리즘 설명
  B) MazeConfigSO 대체: JSON/Config 클래스(Init은 ScriptableObject 없이 Config 클래스도 가능)와 필드 제안
  C) MazeResult 구조(floorCells/wallCells/bounds/start/exit/seed)
  D) C# 스켈레톤: MazeGenerator, MazeResult, TilemapPainter
  E) 디버그: Gizmos(start/exit/교차로 노드 표시), seed 고정 토글

========================================================
2) [7’] 길목 조우 시스템(EncounterDirector)
   요구:
- RoomTrigger 금지. 대신 미로의 “교차로/코너/긴 복도 끝” 같은 포인트에 스폰/이벤트를 배치.
- 조우는 실제로 “보이면/소리나면/순찰로 마주치면” 발생한다.
- 폭주 방지: 동시 적 수 cap, 구간당 스폰 cap, 조우 쿨다운, budget 기반 총량 제한.
- 이벤트(Trap/Loot/EventText)는 CorridorTrigger로 배치(한 번만 발동).
  출력:
  A) EncounterDirector 구성요소: SpawnPlanner, PatrolPlanner, EncounterBudget
  B) 스폰 포인트 생성 규칙(교차로 우선)
  C) 순찰 경로 생성 규칙(노드 2~4개 루프)
  D) 디버그 HUD/토글(스폰 포인트/순찰 경로 표시, 강제 스폰, 강제 이벤트 발동)
  E) C# 스켈레톤: EncounterDirector, SpawnPlanner, PatrolPlanner, CorridorTrigger

========================================================
3) 몬스터 추가: JSON Only 파이프라인
   요구:
- monsters.json 하나로 몬스터를 정의/추가한다.
- Unity 런타임에서 JSON 로드 -> 검증 -> Registry(id->EnemyDefinition) 생성.
- EnemyFactory가 EnemyDefinition을 받아 프리팹에 바인딩(스탯/AI 파라미터 주입).
- enum은 분류(EnemyArchetype, AttackType)만, 개별 몬스터는 id 문자열로 관리.
- 로딩 경로는 Resources 또는 StreamingAssets 2안 제시.
  출력:
  A) monsters.json 스키마(예시 2개 몬스터 포함: Me
