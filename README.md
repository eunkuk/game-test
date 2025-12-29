# Labyrinth - Unity 2D 로그라이크 (Init 세로슬라이스)

Unity 2D 탑다운 로그라이크 게임의 초기 프로토타입 구현입니다.

## 주요 기능 (Phase 1)

### ✅ 구현 완료 시스템

1. **던전 랜덤 생성 (DungeonGenerator)**
   - 8~12개 방 랜덤 배치 (겹침 방지)
   - L자 복도 연결 (순차 연결 방식)
   - Seed 기반 재현 가능
   - Tilemap 자동 페인팅 (바닥/벽)

2. **Random Response(Encounter) 시스템**
   - 가중치 기반 랜덤 테이블
   - 4가지 Encounter 타입:
     - EnemySpawn: 적 스폰
     - Trap: 함정 (Damage/Slow/Poison)
     - Loot: 보상 (골드/체력/아이템)
     - EventText: 선택지 이벤트
   - 방 진입 시 1회 트리거

3. **전방 시야(FOV) 시스템**
   - 레이캐스트 기반 부채꼴 시야
   - 벽/장애물 Occlusion
   - Stencil Shader 마스크
   - 각도/거리 파라미터 조절 가능

4. **Fog-of-War 시스템**
   - 3단계 Fog (미탐색/탐색/현재시야)
   - Tilemap 기반 렌더링
   - 탐색 진행률 추적
   - 변경된 셀만 업데이트 (최적화)

## 아키텍처

### 모듈 구조 (Assembly Definition)

```
Game.Runtime (최상위)
    ↓
Game.UI ────────┐
    ↓           ↓
Game.Gameplay   │
    ↓           ↓
Game.Systems ───┘
    ↓
Game.Core ←──── Game.Data (독립)
```

### 폴더 구조

```
Assets/_Project/
├── Core/           # 공용 유틸, 이벤트, 인터페이스
├── Data/           # ScriptableObject 정의 (던전/Encounter/적/아이템)
├── Systems/        # 핵심 시스템 (Dungeon, Encounter, Vision, FogOfWar)
├── Gameplay/       # 플레이어/적/전투
├── UI/             # HUD, 이벤트 선택지 UI
├── Runtime/        # 게임 진행 관리
└── Shaders/        # VisionMask, DarknessMask
```

## 시작하기

### 필요 환경

- Unity 2022.3 LTS 이상
- URP 2D Renderer
- TextMeshPro

### 세팅 단계

1. Unity에서 프로젝트 열기
2. Tilemap 설정:
   - Floor Tilemap (바닥)
   - Wall Tilemap (벽, Collider 추가)
   - Fog Tilemap (Overlay)
3. ScriptableObject 생성:
   - DungeonConfigSO
   - EncounterTableSO
   - TilePaletteSO
4. 씬 구성:
   - DungeonGenerator + TilemapPainter
   - Player (Controller + Stats + FacingProvider + FieldOfView2D)
   - EncounterResolver + FogOfWarSystem

### 테스트

1. DungeonGenerator의 `autoGenerate` 체크
2. Play 버튼 클릭
3. WASD로 이동하며 던전 탐색

## Init Done 기준 (DoD)

- [x] Seed 고정 시 동일한 맵 재현
- [x] 8~12개 방 생성 (겹침 없음)
- [x] 모든 방이 복도로 연결
- [x] 시작방/출구방 구분
- [x] 방 진입 시 1회 Encounter 발동
- [x] 가중치 테이블 기반 랜덤 결과
- [x] 전방 부채꼴 시야
- [x] 벽 Occlusion 정상 작동
- [x] Fog 3단계 (미탐색/탐색/현재시야)
- [x] Gizmos 디버그 표시

## 확장 로드맵

### Phase 2 예정

1. **다층 던전**: 층별 난이도/테이블 변화
2. **바이옴 시스템**: 시각적 다양성
3. **Encounter 테이블 다중화**: 층/바이옴별 분리
4. **몹 AI 다양화**: 패턴 추가, BehaviorTree
5. **아이템/특성 시스템**: 빌드 다양성
6. **메타 진행**: 영구 업그레이드, 언락

## 리스크/성능 체크리스트

- ⚠️ 레이캐스트 비용 (rayCount 30~50 권장)
- ⚠️ Fog Tilemap 업데이트 (변경된 셀만)
- ⚠️ GC Allocation (메쉬 정점 재사용)
- ⚠️ Layer Collision Matrix 설정
- ⚠️ TilemapCollider + CompositeCollider 조합
- ⚠️ Sorting Layer 순서 (Ground < Vision < Overlay)
- ⚠️ Stencil Ref 값 충돌 방지

## 문서

- [response-phase-1.md](prompt/response-phase-1.md): 전체 아키텍처 설계 문서
- [request-phase-1.md](prompt/request-phase-1.md): 초기 요구사항

## 라이선스

MIT License

---

🤖 Generated with [Claude Code](https://claude.com/claude-code)
