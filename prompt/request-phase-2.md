너는 Unity 2D(탑다운) 로그라이크의 테크리드/아키텍트야.
목표: 1층 던전(미궁) 랜덤 생성 + 방 진입 시 랜덤 Response(조우/함정/보상/이벤트) + 플레이어 전방 시야(FOV, 부채꼴)만 보이고 벽에 의해 가려지는(occlusion) 시스템 + Fog-of-War(미탐색/탐색/현재시야 3단계)를 “Init 세로슬라이스”로 구현한다.
엔진/제약:
- Unity LTS(2022/2023) + URP 2D Renderer
- 2D 탑다운
- 시야: 플레이어가 ‘바라보는 방향’ 기준 전방 부채꼴(각도/거리 파라미터), 벽/장애물로 시야 차단(레이캐스트 기반 occlusion)
- 던전: 1층만, Room 8~12개, 사각형 방 + 직선 복도 연결, Tilemap에 바닥/벽 페인팅
- Response: 가중치 랜덤 테이블로 방 진입 시 1회 트리거(또는 인터랙트 트리거), 결과는 EnemySpawn/Trap/Loot/EventText(선택지 2개) 포함
- Seed 입력 가능(재현성)
- 확장성: 바이옴/층/테이블 확장, 테스트/디버그 용이, 콘텐츠는 ScriptableObject 중심

출력 형식(반드시 이 순서로):
0) “전체 그림(아키텍처)” 먼저 제시
1) [6] DungeonGenerator 설계/구현
2) [7] Random Response(Encounter) 시스템 설계/구현
3) [3] 전방 시야(FOV) 구현 방식 2안 비교 후 Init 추천안 선택
4) [4] 선택한 FOV 방식 실제 구현(오브젝트 구성 + 코드 스켈레톤)
5) [5] Fog-of-War(탐색 기록 남김) Init 버전 구현
   마지막에 “Done 기준(DoD) 10개 + 성능/리스크 체크리스트”를 제공

========================================================
0) 전체 그림(아키텍처)
- Unity 폴더 구조 + Assembly Definition(asmdef) 기반 모듈 분리안을 제시해라.
  예시 모듈(필수 포함):
    - Game.Core (공용 유틸, 시드/랜덤, 이벤트, 인터페이스)
    - Game.Data (ScriptableObject 정의: 던전/방/타일/엔카운터/몹/아이템)
    - Game.Systems (던전 생성, 엔카운터, FOV, Fog-of-War)
    - Game.Gameplay (플레이어, 적 AI, 전투)
    - Game.UI (HUD, 이벤트 선택지 UI)
- 각 모듈의 의존성 방향(단방향)을 명확히 써라. (Data는 어디서든 참조 가능하되 런타임 로직 의존 금지)
- 런타임 흐름(씬/상태머신)을 간단히 정의해라:
  Title(옵션) -> Run(1층) -> Result(옵션)
  Run 씬에서 GameRunManager가 Seed 설정 -> Dungeon 생성 -> Player Spawn -> RoomTrigger로 Encounter 실행 -> 탈출/사망 -> 결과
- “ScriptableObject + Resolver(실행기)” 패턴으로 콘텐츠 확장 가능하게 설계해라.
- EventBus 또는 C# 이벤트를 사용해 시스템 간 결합도를 낮춰라(예: OnEnterRoom, OnEncounterResolved, OnVisionUpdated 등).
- Object Pool(적/이펙트)은 인터페이스만 준비하고 Init에서는 최소 구현으로 두어도 된다.
- 네임스페이스 규칙(예: Game.Core, Game.Systems...)을 명시하고, 코드 스켈레톤은 컴파일 가능 수준으로 작성해라.

========================================================
1) [6] 1층 던전 랜덤 생성 (DungeonGenerator)
   요구:
- Room 8~12개 사각형 랜덤 배치(겹침 방지)
- 방 연결: “순차 연결” 또는 “MST(최소신장트리)” 중 Init에 적합한 것을 선택하고 이유 설명
- 복도: 직선(L자 허용)로 Tilemap에 바닥/벽을 찍기
- 시작방/출구방 지정(가장 먼 방 등 간단 규칙)
- Seed 기반 랜덤(재현 가능)
- 실패 케이스 대응: 배치 실패 시 재시도(최대 N회), 연결 끊김 방지

출력:
A) 알고리즘 설명(짧고 명확)
B) 필요한 Tilemap 구성(바닥/벽, collider, sorting) + Layer 설계(Wall/VisionBlocker)
C) ScriptableObject 데이터 초안:
- DungeonConfigSO(방 개수 범위, 방 크기 범위, 복도 폭, 시드 옵션 등)
- TilePaletteSO(바닥/벽 타일 레퍼런스)
  D) C# 스켈레톤(컴파일 목표):
- DungeonGenerator (Generate(seed) -> DungeonResult)
- DungeonResult (rooms, corridors, startRoom, exitRoom)
- Room (RectInt bounds, center, id)
- TilemapPainter (PaintFloor/Wall)
  E) 디버그:
- Gizmos로 방/연결선 표시
- 콘솔에서 seed 고정/랜덤 토글

========================================================
2) [7] Random Response(Encounter) 시스템 (가중치 테이블)
   요구:
- 트리거: “방 진입 1회” (RoomTrigger2D)
- 테이블: 가중치 랜덤(EncounterTableSO)
- 결과 타입 최소 4개:
    1) EnemySpawn: 몹 1~3 스폰(스폰 포인트는 방 중심 주변)
    2) Trap: 즉시 피해 또는 슬로우 등 1개 디버프(Init에선 간단)
    3) Loot: 골드/소모품 1개 지급
    4) EventText: 선택지 2개(선택에 따른 보상/페널티)
- 확장성: “새 Encounter 타입 추가”가 코드 변경 최소로 가능하도록(인터페이스/추상 클래스 + Resolver)
- UI: EventText는 간단한 패널 UI로 표시(버튼 2개)
- 디버그: 강제 롤(키 입력) + 결과 로그

출력:
A) 데이터 모델(SO) 설계:
- EncounterTableSO: List<WeightedEncounterEntry>
- WeightedEncounterEntry: weight, EncounterDefinitionSO ref
- EncounterDefinitionSO(추상) -> EnemySpawnEncounterSO / TrapEncounterSO / LootEncounterSO / EventTextEncounterSO
  B) 런타임 로직:
- EncounterResolver: Resolve(definition, context) -> EncounterResult
- EncounterContext: seed, roomId, playerRef 등
- EncounterResult: 텍스트/보상/스폰 요청 등
  C) 컴파일 가능한 코드 스켈레톤(네임스페이스 포함)
  D) RoomTrigger2D 설계:
- 한 번만 발동(visited flag)
- 방 id/테이블 참조
  E) 밸런스/테스트 포인트:
- 테이블 가중치 예시(Init용)
- 폭주 방지(예: EnemySpawn 연속 제한 같은 간단 규칙은 옵션)

========================================================
3) [3] 전방 시야(FOV) 구현 방식 2안 비교 + Init 추천안 선택
   아래 2안을 비교하고, Init에 가장 빠른 1안을 선택해라:
   A) 레이캐스트 팬(Raycast fan)으로 “FOV 메쉬” 생성 + 어둠 오버레이 마스크(스텐실/메시)
   B) URP 2D Light(Spot 유사) + ShadowCaster2D로 가시성 구현
   비교 기준:
- 구현 난이도/디버그 용이성/확장성(각도/거리 변경, 성능)
- “벽에 의해 시야가 잘려 보이는지” 정확도
- Fog-of-War와 결합 난이도
  출력:
- 두 방식 장단점 표
- Init 추천안(1개) + 이유
- 필요한 오브젝트/렌더 구성 개요

========================================================
4) [4] 선택한 FOV 방식 실제 구현(오브젝트 구성 + 코드)
   요구:
- FOV 파라미터: viewAngle(deg), viewDistance, rayCount(해상도), updateRate(매프레임/주기)
- occlusion: Physics2D.Raycast로 Wall/VisionBlocker 레이어를 맞으면 해당 지점까지 메쉬 컷
- “플레이어 Facing 방향”을 기준으로 전방 부채꼴만 생성
- 보이지 않는 영역은 어두운 오버레이로 가리고, 시야 영역만 투명/밝게
- 디버그: Gizmos로 레이/콘 표시 토글

출력:
A) 씬 오브젝트 구성(정확한 이름 예시):
- Player (PlayerController + FacingProvider)
- Vision (FieldOfView2D + MeshFilter + MeshRenderer)
- DarknessOverlay (전체 화면 어둠 레이어)
- (선택) VisionMask/Stencil Material 세팅 설명
  B) 레이어/콜라이더 요구사항:
- Wall/VisionBlocker는 Collider2D 필수
  C) C# 스켈레톤 2~3개(컴파일 가능):
- FieldOfView2D: 메쉬 생성, 레이캐스트, 정점/삼각형 업데이트
- FacingProvider: 플레이어 마지막 이동 방향 제공(또는 PlayerController가 구현)
- (필요 시) VisionRenderer/VisionOverlay: 머티리얼/마스크 적용
  D) 성능 가이드:
- rayCount 추천 범위
- updateRate 타협안

========================================================
5) [5] Fog-of-War(탐색 기록) Init 버전
   목표(3상태):
- 현재 시야: 완전 밝음
- 탐색한 영역(visited): 희미하게
- 미탐색: 완전 어둠
  제약:
- Init에서는 가장 단순한 구현을 우선(고급 셰이더/RenderTexture는 “선택안”으로만)
- Tilemap 기반 던전과 자연스럽게 결합
- 확장성: 나중에 층/바이옴 커져도 구조 유지

출력:
A) 가장 빠른 구현 1안(필수) + 선택적 고급안 1개(옵션)
- 필수안 예: 그리드(셀) 단위 visited bool 저장 + “FogOverlay Tilemap” 또는 간단 텍스처 오버레이
  B) 데이터 구조:
- FogOfWarGrid(width,height), visited[], visible[]
- 좌표 변환(world <-> cell)
  C) 업데이트 흐름:
- FOV가 계산한 “현재 시야 폴리곤/레이 결과”로 visible 셀 마킹
- visible 셀은 visited로 승격
  D) 코드 스켈레톤:
- FogOfWarSystem: UpdateVisibility(fovData)
- FogRenderer(Overlay): 타일/알파 업데이트(Init 최적화 포함: 변경된 셀만 갱신)
  E) 디버그:
- visited/visible 토글 표시
- 맵 리셋

========================================================
마무리 산출물(반드시 포함):
1) Init Done 기준(DoD) 10개 (예: seed 고정 시 동일 맵 재현, 방 진입 시 1회 엔카운터, FOV occlusion 정상, Fog 3상태 정상 등)
2) 리스크/성능 체크리스트(최소 8개):
    - 레이캐스트 비용, 타일 업데이트 비용, GC 발생 지점, 레이어 충돌 매트릭스, 카메라/정렬 문제 등
3) “다음 확장 로드맵” 6개:
    - 층 증가, 테이블 다중화, 바이옴, 몹 AI 다양화, 아이템/특성, 메타 진행 등

주의:
- 코드는 반드시 Unity C# 기준으로 컴파일 가능한 형태(네임스페이스/using 포함)로 제공
- 표/목록/코드블록을 섞어 가독성 좋게 작성
- Init에서는 ‘빨리 눈으로 확인되는 결과’ 우선으로 선택하고, 확장 포인트는 인터페이스/추상화로 남겨라.
