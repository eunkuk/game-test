# Phase 5 Implementation Request - 최종 층 및 엔딩 시스템

> **작성일**: 2025-12-30
> **대상**: Claude Code
> **목적**: Phase 5 (최종 층, 엔딩 시스템, 신 시스템, 지식 체계) 구현 상세 가이드

---

## 📋 목차

0. [아키텍처 개요](#0-아키텍처-개요)
1. [최종 층: 영원의 전당](#1-최종-층-영원의-전당)
2. [최종 보스: 시간의 수호자](#2-최종-보스-시간의-수호자)
3. [멀티 엔딩 시스템](#3-멀티-엔딩-시스템)
4. [신 시스템 (God System)](#4-신-시스템-god-system)
5. [지식 시스템 (Morgue File)](#5-지식-시스템-morgue-file)
6. [업적 시스템](#6-업적-시스템)
7. [스토리 완성](#7-스토리-완성)
8. [종족/직업 확장](#8-종족직업-확장)
9. [Definition of Done](#9-definition-of-done)
10. [성능 및 리스크 체크리스트](#10-성능-및-리스크-체크리스트)
11. [Phase 6 로드맵](#11-phase-6-로드맵)

---

## 0. 아키텍처 개요

### Phase 4 → Phase 5 전환

```
Phase 4 완료 상태:
├── 3개 브랜치 (2-4층)
├── 장비/소비 아이템 시스템
├── 스킬 시스템 (5개 카테고리, 0-27 레벨)
├── 상태 효과 시스템 (6종)
├── 보스 AI (각 브랜치 보스)
└── 메타 진행도 (언락 시스템)

Phase 5 추가 요구사항:
├── 5층: 영원의 전당 (Hall of Eternity)
├── 최종 보스: 시간의 수호자 (5단계 패턴)
├── 멀티 엔딩 (3개 엔딩)
├── 신 시스템 (3명의 신)
├── 지식 시스템 (Morgue File 기반)
├── 업적 시스템 (30+ 업적)
├── 스토리 완성 (종족별 엔딩 변화)
└── 종족/직업 확장 (8 종족, 10 직업)
```

### 아키텍처 다이어그램

```
Assets/_Project/
├── Core/
│   ├── Events/
│   │   └── GameEvents.cs (엔딩, 신, 업적 이벤트 추가)
│   └── Interfaces/
│       └── IGodBlessingReceiver.cs (신 축복 인터페이스)
│
├── Systems/
│   ├── God/
│   │   ├── GodManager.cs (신 관리)
│   │   ├── GodFavorSystem.cs (호감도 시스템)
│   │   └── Blessing.cs (축복 효과)
│   ├── Ending/
│   │   ├── EndingManager.cs (엔딩 분기 관리)
│   │   ├── EndingConditionEvaluator.cs (조건 평가)
│   │   └── EndingCutscene.cs (엔딩 연출)
│   ├── Knowledge/
│   │   ├── MorgueFileGenerator.cs (사망 기록 생성)
│   │   ├── MonsterKnowledgeTracker.cs (몬스터 도감)
│   │   └── ItemKnowledgeTracker.cs (아이템 도감)
│   ├── Achievement/
│   │   ├── AchievementManager.cs (업적 관리)
│   │   └── AchievementCondition.cs (업적 조건)
│   └── Maze/
│       └── FinalFloorGenerator.cs (5층 생성)
│
├── Gameplay/
│   ├── Boss/
│   │   └── TimeKeeperAI.cs (최종 보스 AI)
│   └── Player/
│       └── PlayerGodRelation.cs (플레이어-신 관계)
│
├── UI/
│   ├── GodPanel.cs (신 UI)
│   ├── KnowledgePanel.cs (지식 UI)
│   ├── AchievementPanel.cs (업적 UI)
│   └── EndingCredits.cs (엔딩 크레딧)
│
└── DataJson/
    ├── Schemas/
    │   ├── god_schema.json
    │   ├── ending_schema.json
    │   ├── achievement_schema.json
    │   └── final_floor_schema.json
    └── Loaders/
        ├── GodDataLoader.cs
        ├── EndingDataLoader.cs
        └── AchievementDataLoader.cs

StreamingAssets/GameData/
├── gods.json (3명의 신 정의)
├── blessings.json (신 축복 효과)
├── endings.json (3개 엔딩 분기)
├── achievements.json (30+ 업적)
├── final_floor.json (5층 구조)
└── monsters_final.json (5층 전용 몬스터)
```

---

## 1. 최종 층: 영원의 전당

### 1.1 층 구조 설계

**영원의 전당 (Hall of Eternity)** - 5층
- **진입 조건**: 2개 이상의 룬 획득 (Phase 4 브랜치 클리어)
- **구조**: 원형 대칭 구조 (4개 방향 대칭)
- **크기**: 61x61 그리드 (기존 41x41보다 확장)
- **특징**:
  - 중앙: 최종 보스 룸 (시간의 수호자)
  - 4개 날개: 각 방향에 미니 보스 (시간의 파편)
  - Vault 룸: 최고급 아이템 (엔딩 장비)
  - 성소 (Shrine): 신의 축복 강화
  - 지식의 방: 모든 몬스터/아이템 정보 열람

### 1.2 JSON 스키마 (final_floor_schema.json)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "FinalFloorSchema",
  "type": "object",
  "required": ["version", "floorData"],
  "properties": {
    "version": { "type": "string" },
    "floorData": {
      "type": "object",
      "required": ["id", "displayName", "gridSize", "structure", "miniBosses", "vaults", "shrines"],
      "properties": {
        "id": { "type": "string" },
        "displayName": { "type": "string" },
        "gridSize": {
          "type": "object",
          "properties": {
            "width": { "type": "integer", "minimum": 61 },
            "height": { "type": "integer", "minimum": 61 }
          }
        },
        "structure": {
          "type": "object",
          "properties": {
            "centerRoom": {
              "type": "object",
              "properties": {
                "size": { "type": "integer" },
                "bossId": { "type": "string" }
              }
            },
            "wings": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "direction": { "enum": ["north", "east", "south", "west"] },
                  "miniBossId": { "type": "string" },
                  "roomCount": { "type": "integer" }
                }
              }
            }
          }
        },
        "miniBosses": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "id": { "type": "string" },
              "displayName": { "type": "string" },
              "aspect": { "enum": ["past", "present", "future", "eternity"] },
              "health": { "type": "integer" },
              "mechanics": { "type": "array", "items": { "type": "string" } }
            }
          }
        },
        "vaults": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "id": { "type": "string" },
              "layout": { "type": "string" },
              "lootTable": { "type": "array", "items": { "type": "string" } }
            }
          }
        },
        "shrines": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "godId": { "type": "string" },
              "blessing": { "type": "string" },
              "cost": { "type": "integer" }
            }
          }
        }
      }
    }
  }
}
```

### 1.3 JSON 데이터 (final_floor.json)

```json
{
  "version": "1.0",
  "floorData": {
    "id": "hall_of_eternity",
    "displayName": "영원의 전당",
    "description": "시간의 끝에서 기다리는 수호자의 영역",
    "gridSize": {
      "width": 61,
      "height": 61
    },
    "structure": {
      "centerRoom": {
        "size": 15,
        "bossId": "time_keeper",
        "roomType": "circular",
        "entranceRequirement": "defeat_all_mini_bosses"
      },
      "wings": [
        {
          "direction": "north",
          "theme": "과거의 기억",
          "miniBossId": "fragment_of_past",
          "roomCount": 8,
          "corridorStyle": "ancient"
        },
        {
          "direction": "east",
          "theme": "현재의 순간",
          "miniBossId": "fragment_of_present",
          "roomCount": 8,
          "corridorStyle": "modern"
        },
        {
          "direction": "south",
          "theme": "미래의 환영",
          "miniBossId": "fragment_of_future",
          "roomCount": 8,
          "corridorStyle": "futuristic"
        },
        {
          "direction": "west",
          "theme": "영원의 순환",
          "miniBossId": "fragment_of_eternity",
          "roomCount": 8,
          "corridorStyle": "timeless"
        }
      ]
    },
    "miniBosses": [
      {
        "id": "fragment_of_past",
        "displayName": "과거의 파편",
        "aspect": "past",
        "health": 800,
        "mechanics": [
          "회귀 패턴: 3초마다 이전 위치로 순간이동",
          "기억 투사: 과거 패턴 재생 (ghost trail)",
          "시간 지연: 플레이어 공격 1초 지연"
        ],
        "loot": {
          "guaranteed": ["rune_of_past"],
          "pool": ["ancient_weapon", "memory_armor"]
        }
      },
      {
        "id": "fragment_of_present",
        "displayName": "현재의 파편",
        "aspect": "present",
        "health": 900,
        "mechanics": [
          "순간 포착: 플레이어 위치 고정 (2초)",
          "동시 공격: 모든 방향 동시 패턴",
          "현재 강화: 시간이 지날수록 공격력 증가"
        ],
        "loot": {
          "guaranteed": ["rune_of_present"],
          "pool": ["living_blade", "momentary_shield"]
        }
      },
      {
        "id": "fragment_of_future",
        "displayName": "미래의 파편",
        "aspect": "future",
        "health": 850,
        "mechanics": [
          "예지 회피: 플레이어 공격 50% 회피 (미리 예측)",
          "미래 투사: 5초 후 발동할 패턴 미리 표시",
          "시간 가속: 패턴 속도 50% 증가"
        ],
        "loot": {
          "guaranteed": ["rune_of_future"],
          "pool": ["prophetic_staff", "foresight_ring"]
        }
      },
      {
        "id": "fragment_of_eternity",
        "displayName": "영원의 파편",
        "aspect": "eternity",
        "health": 1000,
        "mechanics": [
          "순환 재생: 체력 50% 이하 시 전체 회복 (1회)",
          "무한 복제: 2개의 환영 생성 (체력 30%)",
          "시간 정지: 5초마다 1초 시간 정지 (플레이어만)"
        ],
        "loot": {
          "guaranteed": ["rune_of_eternity"],
          "pool": ["eternal_crown", "infinity_amulet"]
        }
      }
    ],
    "vaults": [
      {
        "id": "vault_weapons",
        "layout": "armory",
        "description": "최고급 무기 보관소",
        "lootTable": [
          "legendary_sword_tier9",
          "legendary_staff_tier9",
          "legendary_bow_tier9"
        ],
        "trap": "laser_grid"
      },
      {
        "id": "vault_artifacts",
        "layout": "treasury",
        "description": "고대 유물 금고",
        "lootTable": [
          "artifact_ring_timeless",
          "artifact_amulet_eternity",
          "artifact_crown_omniscience"
        ],
        "trap": "time_warp"
      }
    ],
    "shrines": [
      {
        "godId": "god_of_war",
        "blessing": "ultimate_strength",
        "displayName": "전쟁의 성소",
        "cost": 500,
        "effect": "+50% 공격력, +30% 치명타 (영구)"
      },
      {
        "godId": "god_of_magic",
        "blessing": "arcane_mastery",
        "displayName": "마법의 성소",
        "cost": 500,
        "effect": "마법 스킬 레벨 +5, 마나 재생 +200% (영구)"
      },
      {
        "godId": "god_of_death",
        "blessing": "immortal_soul",
        "displayName": "죽음의 성소",
        "cost": 500,
        "effect": "부활 1회, 부활 시 체력 100% (영구)"
      }
    ]
  }
}
```

### 1.4 C# 구현 (FinalFloorGenerator.cs)

```csharp
namespace Game.Systems.Maze
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.DataJson.Loaders;

    /// <summary>
    /// 5층 "영원의 전당" 생성기 (원형 대칭 구조)
    /// </summary>
    public class FinalFloorGenerator : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 61;
        [SerializeField] private int gridHeight = 61;

        [Header("Structure")]
        [SerializeField] private int centerRoomSize = 15;
        [SerializeField] private int wingRoomCount = 8;

        [Header("References")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject shrineBasePrefab;

        private MazeCell[,] grid;
        private System.Random rng;
        private FinalFloorData floorData;

        public void GenerateFloor(int seed)
        {
            rng = new System.Random(seed);
            floorData = JsonDataLoader.LoadFinalFloorData();

            InitializeGrid();
            GenerateCenterRoom();
            GenerateFourWings();
            PlaceMiniBosses();
            PlaceVaults();
            PlaceShrines();
            InstantiateVisuals();
        }

        private void InitializeGrid()
        {
            grid = new MazeCell[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = new MazeCell(x, y, MazeCellType.Wall);
                }
            }
        }

        /// <summary>
        /// 중앙 원형 보스 룸 생성
        /// </summary>
        private void GenerateCenterRoom()
        {
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;
            int radius = centerRoomSize / 2;

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(centerX, centerY)
                    );

                    if (distance <= radius)
                    {
                        if (IsInBounds(x, y))
                        {
                            grid[x, y].cellType = MazeCellType.Floor;
                            grid[x, y].roomId = "center_boss_room";
                        }
                    }
                }
            }

            // 보스 스폰 포인트
            grid[centerX, centerY].hasBoss = true;
            grid[centerX, centerY].bossId = floorData.structure.centerRoom.bossId;
        }

        /// <summary>
        /// 4개 날개 생성 (북, 동, 남, 서)
        /// </summary>
        private void GenerateFourWings()
        {
            Vector2Int center = new Vector2Int(gridWidth / 2, gridHeight / 2);
            int wingLength = 20;

            foreach (var wing in floorData.structure.wings)
            {
                Vector2Int direction = GetDirectionVector(wing.direction);
                GenerateWing(center, direction, wingLength, wing);
            }
        }

        private void GenerateWing(Vector2Int start, Vector2Int dir, int length, WingData wing)
        {
            Vector2Int current = start;

            // 메인 복도
            for (int i = 0; i < length; i++)
            {
                current += dir;
                if (!IsInBounds(current.x, current.y)) break;

                CreateCorridor(current, dir);

                // 8개 룸마다 사이드 룸 생성
                if (i % 3 == 0 && i > 0)
                {
                    CreateSideRoom(current, dir);
                }
            }

            // 끝에 미니보스 룸
            CreateMiniBossRoom(current, wing.miniBossId);
        }

        private void CreateCorridor(Vector2Int pos, Vector2Int dir)
        {
            // 3칸 너비 복도
            Vector2Int perpendicular = new Vector2Int(-dir.y, dir.x);

            for (int offset = -1; offset <= 1; offset++)
            {
                Vector2Int corridorPos = pos + perpendicular * offset;
                if (IsInBounds(corridorPos.x, corridorPos.y))
                {
                    grid[corridorPos.x, corridorPos.y].cellType = MazeCellType.Floor;
                }
            }
        }

        private void CreateSideRoom(Vector2Int corridorPos, Vector2Int mainDir)
        {
            Vector2Int perpendicular = new Vector2Int(-mainDir.y, mainDir.x);
            Vector2Int roomStart = corridorPos + perpendicular * 3;

            int roomWidth = 5 + rng.Next(3);
            int roomHeight = 5 + rng.Next(3);

            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    Vector2Int roomPos = roomStart + new Vector2Int(x, y);
                    if (IsInBounds(roomPos.x, roomPos.y))
                    {
                        grid[roomPos.x, roomPos.y].cellType = MazeCellType.Floor;
                    }
                }
            }
        }

        private void CreateMiniBossRoom(Vector2Int pos, string miniBossId)
        {
            int roomSize = 10;

            for (int x = -roomSize / 2; x <= roomSize / 2; x++)
            {
                for (int y = -roomSize / 2; y <= roomSize / 2; y++)
                {
                    Vector2Int roomPos = pos + new Vector2Int(x, y);
                    if (IsInBounds(roomPos.x, roomPos.y))
                    {
                        grid[roomPos.x, roomPos.y].cellType = MazeCellType.Floor;
                        grid[roomPos.x, roomPos.y].roomId = $"miniboss_{miniBossId}";
                    }
                }
            }

            grid[pos.x, pos.y].hasBoss = true;
            grid[pos.x, pos.y].bossId = miniBossId;
        }

        private void PlaceMiniBosses()
        {
            // grid 순회하며 hasBoss == true인 셀에 보스 스폰
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y].hasBoss && !string.IsNullOrEmpty(grid[x, y].bossId))
                    {
                        SpawnBoss(new Vector2Int(x, y), grid[x, y].bossId);
                    }
                }
            }
        }

        private void PlaceVaults()
        {
            // Vault 위치 랜덤 배치 (빈 사이드 룸 활용)
            foreach (var vault in floorData.vaults)
            {
                Vector2Int vaultPos = FindEmptyRoomForVault();
                CreateVaultRoom(vaultPos, vault);
            }
        }

        private void PlaceShrines()
        {
            // 각 날개마다 1개 성소 배치
            foreach (var shrine in floorData.shrines)
            {
                Vector2Int shrinePos = FindShrinePosition(shrine.godId);
                InstantiateShrine(shrinePos, shrine);
            }
        }

        private void InstantiateVisuals()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 worldPos = new Vector3(x, y, 0);

                    if (grid[x, y].cellType == MazeCellType.Wall)
                    {
                        Instantiate(wallPrefab, worldPos, Quaternion.identity, transform);
                    }
                    else
                    {
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, transform);
                    }
                }
            }
        }

        private Vector2Int GetDirectionVector(string direction)
        {
            return direction switch
            {
                "north" => Vector2Int.up,
                "east" => Vector2Int.right,
                "south" => Vector2Int.down,
                "west" => Vector2Int.left,
                _ => Vector2Int.zero
            };
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }

        private void SpawnBoss(Vector2Int pos, string bossId) { /* Boss spawning */ }
        private Vector2Int FindEmptyRoomForVault() { return Vector2Int.zero; /* 구현 */ }
        private void CreateVaultRoom(Vector2Int pos, VaultData vault) { /* 구현 */ }
        private Vector2Int FindShrinePosition(string godId) { return Vector2Int.zero; /* 구현 */ }
        private void InstantiateShrine(Vector2Int pos, ShrineData shrine) { /* 구현 */ }
    }

    // Data structures
    [System.Serializable]
    public class FinalFloorData
    {
        public string id;
        public string displayName;
        public GridSize gridSize;
        public FloorStructure structure;
        public List<MiniBossData> miniBosses;
        public List<VaultData> vaults;
        public List<ShrineData> shrines;
    }

    [System.Serializable]
    public class FloorStructure
    {
        public CenterRoomData centerRoom;
        public List<WingData> wings;
    }

    [System.Serializable]
    public class WingData
    {
        public string direction;
        public string theme;
        public string miniBossId;
        public int roomCount;
    }

    [System.Serializable]
    public class MiniBossData
    {
        public string id;
        public string displayName;
        public string aspect;
        public int health;
        public List<string> mechanics;
    }

    [System.Serializable]
    public class VaultData
    {
        public string id;
        public string layout;
        public List<string> lootTable;
    }

    [System.Serializable]
    public class ShrineData
    {
        public string godId;
        public string blessing;
        public string displayName;
        public int cost;
    }
}
```

---

## 2. 최종 보스: 시간의 수호자

### 2.1 보스 설계

**시간의 수호자 (Time Keeper)**
- **컨셉**: 시간을 조종하는 초월적 존재, DCSS의 Pan Lords 스타일
- **체력**: 3000 HP (5단계 페이즈)
- **페이즈 전환**: 체력 80% / 60% / 40% / 20% / 10%
- **특징**: 각 페이즈마다 다른 시간 조작 패턴

### 2.2 5단계 페이즈 패턴

#### Phase 1: 과거 회귀 (80-100% HP)
```yaml
패턴:
  - 시간 역행: 3초마다 보스가 3초 전 위치로 순간이동
  - 기억 투사: 플레이어의 5초 전 위치에 공격 투사
  - 과거 소환: 이전에 죽은 미니보스 환영 소환 (체력 30%)

시각 효과:
  - 보라색 잔상 효과
  - 시계 역방향 회전 이펙트

대응 전략:
  - 예측 불가능한 이동 패턴 유지
  - 환영 우선 처리
```

#### Phase 2: 현재 정지 (60-80% HP)
```yaml
패턴:
  - 시간 정지: 5초마다 3초간 플레이어 이동/공격 불가
  - 순간 포획: 플레이어를 시공간 구체에 가둠 (탈출 조건: 3회 공격)
  - 동시 공격: 8방향 동시 레이저 발사

시각 효과:
  - 청록색 시간 정지 필드
  - 화면 흑백 전환 효과

대응 전략:
  - 시간 정지 전조 (원형 표시) 확인 후 안전 지대 이동
  - 구체 탈출 시 스킬 쿨다운 짧은 공격 사용
```

#### Phase 3: 미래 예지 (40-60% HP)
```yaml
패턴:
  - 미래 투사: 5초 후 발동할 공격 위치 미리 표시 (붉은 경고)
  - 예지 회피: 플레이어 공격 70% 회피 (미리 예측)
  - 시간 가속: 보스 이동속도 +50%, 공격속도 +100%

시각 효과:
  - 금색 미래 투영 잔상
  - 공격 위치 사전 경고 (붉은 타일)

대응 전략:
  - 미리 표시된 공격 위치 피하기
  - 예지 불가능한 범위 공격 (AoE) 사용
```

#### Phase 4: 영원 순환 (20-40% HP)
```yaml
패턴:
  - 시간 복제: 보스가 3개로 분열 (각 체력 공유, 동시 공격)
  - 순환 재생: 체력 30% 이하 시 50% 회복 (1회만)
  - 과거/현재/미래 혼합 패턴: 위 3개 페이즈 패턴 랜덤 조합

시각 효과:
  - 무지개 색상 혼합
  - 시공간 왜곡 이펙트

대응 전략:
  - 3개 복제체 동시 공격으로 체력 공유 활용
  - 재생 전에 집중 딜 (재생은 1회만 가능)
```

#### Phase 5: 시간의 끝 (0-20% HP)
```yaml
패턴:
  - 시간 붕괴: 맵 전체에 시공간 균열 생성 (지속 피해 존)
  - 최후의 순간: 플레이어 체력 1로 감소 (1회, 무적 불가)
  - 영원한 종말: 10초 카운트다운 후 즉사 공격 (회피 가능)

시각 효과:
  - 검은 균열과 흰색 빛
  - 화면 파편화 효과

대응 전략:
  - 균열 피하며 기동력 유지
  - 체력 1 감소 후 즉시 회복 아이템 사용
  - 카운트다운 종료 전 피니시
```

### 2.3 C# 구현 (TimeKeeperAI.cs)

```csharp
namespace Game.Gameplay.Boss
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using Game.Core.Events;

    /// <summary>
    /// 최종 보스 "시간의 수호자" AI (5단계 페이즈)
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class TimeKeeperAI : MonoBehaviour
    {
        [Header("Boss Stats")]
        [SerializeField] private int maxHealth = 3000;
        [SerializeField] private float moveSpeed = 3f;

        [Header("Phase Thresholds")]
        [SerializeField] private float[] phaseThresholds = { 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };

        [Header("Pattern Prefabs")]
        [SerializeField] private GameObject pastEchoPrefab;
        [SerializeField] private GameObject timeSpherePrefa<br;
        [SerializeField] private GameObject futureWarningPrefab;
        [SerializeField] private GameObject riftPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip phaseTransitionSound;
        [SerializeField] private AudioClip timeStopSound;

        private Health health;
        private Transform player;
        private int currentPhase = 1;
        private bool isTransitioning = false;

        // Phase 1
        private Vector3[] positionHistory = new Vector3[300]; // 5분 @ 60fps
        private int historyIndex = 0;

        // Phase 2
        private bool isTimeStopActive = false;
        private float timeStopCooldown = 5f;
        private float timeStopTimer = 0f;

        // Phase 4
        private bool hasUsedRegeneration = false;

        private void Start()
        {
            health = GetComponent<Health>();
            health.OnHealthChanged += CheckPhaseTransition;

            player = GameObject.FindGameObjectWithTag("Player").transform;

            StartCoroutine(BossAI());
        }

        private IEnumerator BossAI()
        {
            while (health.GetCurrentHealth() > 0)
            {
                if (isTransitioning)
                {
                    yield return null;
                    continue;
                }

                switch (currentPhase)
                {
                    case 1: yield return Phase1_PastRegression(); break;
                    case 2: yield return Phase2_TimeStop(); break;
                    case 3: yield return Phase3_FutureSight(); break;
                    case 4: yield return Phase4_EternalCycle(); break;
                    case 5: yield return Phase5_EndOfTime(); break;
                }

                yield return new WaitForSeconds(0.1f);
            }

            OnBossDeath();
        }

        // ========== Phase 1: 과거 회귀 ==========
        private IEnumerator Phase1_PastRegression()
        {
            // 위치 기록
            positionHistory[historyIndex] = transform.position;
            historyIndex = (historyIndex + 1) % positionHistory.Length;

            // 3초마다 3초 전 위치로 순간이동
            if (Time.frameCount % 180 == 0)
            {
                int pastIndex = (historyIndex - 180 + positionHistory.Length) % positionHistory.Length;
                Vector3 pastPosition = positionHistory[pastIndex];

                if (pastPosition != Vector3.zero)
                {
                    transform.position = pastPosition;
                    SpawnEchoEffect(transform.position);
                }
            }

            // 기억 투사 (5초 전 플레이어 위치 공격)
            if (Time.frameCount % 300 == 0)
            {
                Vector3 pastPlayerPos = GetPlayerPastPosition(300);
                CastPastProjectile(pastPlayerPos);
            }

            yield return null;
        }

        // ========== Phase 2: 현재 정지 ==========
        private IEnumerator Phase2_TimeStop()
        {
            timeStopTimer += Time.deltaTime;

            // 5초마다 시간 정지
            if (timeStopTimer >= timeStopCooldown && !isTimeStopActive)
            {
                StartCoroutine(ActivateTimeStop());
                timeStopTimer = 0f;
            }

            // 8방향 레이저
            if (Time.frameCount % 120 == 0)
            {
                FireOmnidirectionalLasers();
            }

            yield return null;
        }

        private IEnumerator ActivateTimeStop()
        {
            isTimeStopActive = true;
            AudioSource.PlayClipAtPoint(timeStopSound, transform.position);

            // 플레이어 프리즈
            GameEvents.TriggerPlayerFreeze(3f);

            // 시각 효과
            ShowTimeStopEffect();

            yield return new WaitForSeconds(3f);

            isTimeStopActive = false;
        }

        // ========== Phase 3: 미래 예지 ==========
        private IEnumerator Phase3_FutureSight()
        {
            // 5초 후 공격 위치 미리 표시
            if (Time.frameCount % 300 == 0)
            {
                Vector3 futureAttackPos = PredictPlayerPosition(5f);
                StartCoroutine(DelayedAttack(futureAttackPos, 5f));
            }

            // 예지 회피 (70% 확률)
            // 플레이어 공격 감지 시 Health.cs에서 TakeDamage 호출 전 70% 회피

            yield return null;
        }

        private IEnumerator DelayedAttack(Vector3 position, float delay)
        {
            // 경고 표시
            GameObject warning = Instantiate(futureWarningPrefab, position, Quaternion.identity);

            yield return new WaitForSeconds(delay);

            // 실제 공격
            CastExplosion(position);
            Destroy(warning);
        }

        // ========== Phase 4: 영원 순환 ==========
        private IEnumerator Phase4_EternalCycle()
        {
            // 체력 30% 이하 시 1회 재생
            if (health.GetCurrentHealth() < maxHealth * 0.3f && !hasUsedRegeneration)
            {
                health.Heal(Mathf.RoundToInt(maxHealth * 0.5f));
                hasUsedRegeneration = true;
                SpawnRegenerationEffect();
            }

            // 과거/현재/미래 패턴 랜덤 조합
            int randomPattern = Random.Range(0, 3);
            switch (randomPattern)
            {
                case 0: yield return Phase1_PastRegression(); break;
                case 1: yield return Phase2_TimeStop(); break;
                case 2: yield return Phase3_FutureSight(); break;
            }
        }

        // ========== Phase 5: 시간의 끝 ==========
        private IEnumerator Phase5_EndOfTime()
        {
            // 시공간 균열 생성
            if (Time.frameCount % 60 == 0)
            {
                SpawnRift(GetRandomPositionInArena());
            }

            // 최후의 순간 (체력 1 감소) - 1회만
            if (health.GetCurrentHealth() < maxHealth * 0.15f && !hasUsedInstakill)
            {
                StartCoroutine(LastMomentAttack());
                hasUsedInstakill = true;
            }

            yield return null;
        }

        private bool hasUsedInstakill = false;

        private IEnumerator LastMomentAttack()
        {
            GameEvents.TriggerBossUltimate("최후의 순간");

            // 플레이어 체력 1로 감소 (무적 무시)
            var playerHealth = player.GetComponent<Health>();
            int currentPlayerHealth = playerHealth.GetCurrentHealth();
            playerHealth.TakeDamage(currentPlayerHealth - 1, true); // ignoreInvincibility

            // 10초 카운트다운
            yield return new WaitForSeconds(10f);

            // 화면 전체 즉사 공격 (회피 불가)
            if (Vector3.Distance(player.position, transform.position) < 50f)
            {
                playerHealth.TakeDamage(9999, true);
            }
        }

        // ========== Utility ==========
        private void CheckPhaseTransition(int currentHP, int maxHP)
        {
            float hpPercent = (float)currentHP / maxHP;

            for (int i = 0; i < phaseThresholds.Length; i++)
            {
                if (hpPercent <= phaseThresholds[i] && currentPhase == i + 1)
                {
                    StartCoroutine(TransitionToPhase(i + 2));
                    break;
                }
            }
        }

        private IEnumerator TransitionToPhase(int newPhase)
        {
            isTransitioning = true;
            AudioSource.PlayClipAtPoint(phaseTransitionSound, transform.position);

            // 페이즈 전환 연출
            Debug.Log($"[TimeKeeper] Phase {currentPhase} → Phase {newPhase}");
            ShowPhaseTransitionCutscene(newPhase);

            yield return new WaitForSeconds(2f);

            currentPhase = newPhase;
            isTransitioning = false;
        }

        private void OnBossDeath()
        {
            Debug.Log("[TimeKeeper] Boss defeated!");
            GameEvents.TriggerBossDefeated("time_keeper");

            // 엔딩 조건 체크
            EndingManager.Instance.EvaluateEnding();
        }

        private Vector3 GetPlayerPastPosition(int framesAgo) { return Vector3.zero; /* 구현 */ }
        private void SpawnEchoEffect(Vector3 pos) { /* 구현 */ }
        private void CastPastProjectile(Vector3 target) { /* 구현 */ }
        private void FireOmnidirectionalLasers() { /* 구현 */ }
        private void ShowTimeStopEffect() { /* 구현 */ }
        private Vector3 PredictPlayerPosition(float seconds) { return player.position; }
        private void CastExplosion(Vector3 pos) { /* 구현 */ }
        private void SpawnRegenerationEffect() { /* 구현 */ }
        private void SpawnRift(Vector3 pos) { Instantiate(riftPrefab, pos, Quaternion.identity); }
        private Vector3 GetRandomPositionInArena() { return Vector3.zero; /* 구현 */ }
        private void ShowPhaseTransitionCutscene(int phase) { /* 구현 */ }
    }
}
```

---

## 3. 멀티 엔딩 시스템

### 3.1 엔딩 분기 설계

Labyrinth는 **3개의 엔딩**을 제공하며, 플레이어의 선택과 플레이 스타일에 따라 결정됩니다.

| 엔딩 | 조건 | 테마 | 보상 |
|------|------|------|------|
| **엔딩 A: 영원한 순환** | 최종 보스를 신 축복 없이 처치 | 자력으로 운명 극복 | 타이틀: "자유의 사도", 스킨 언락 |
| **엔딩 B: 신의 대리인** | 최종 보스를 신 축복으로 처치 (호감도 100) | 신과의 계약 | 타이틀: "신의 사도", 종족 특성 강화 |
| **엔딩 C: 시간의 계승자** | 최종 보스를 흡수 (특수 아이템 사용) | 보스의 힘 흡수 | 타이틀: "시간 지배자", 뉴게임+ 모드 |

### 3.2 JSON 스키마 (ending_schema.json)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "EndingSchema",
  "type": "object",
  "required": ["version", "endings"],
  "properties": {
    "version": { "type": "string" },
    "endings": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "name", "conditions", "cutscene", "rewards"],
        "properties": {
          "id": { "type": "string" },
          "name": { "type": "string" },
          "description": { "type": "string" },
          "conditions": {
            "type": "object",
            "properties": {
              "bossDefeated": { "type": "boolean" },
              "godBlessingUsed": { "type": "boolean" },
              "specialItemUsed": { "type": "string" },
              "minGodFavor": { "type": "integer" },
              "maxDeaths": { "type": "integer" }
            }
          },
          "cutscene": {
            "type": "object",
            "properties": {
              "dialogues": { "type": "array", "items": { "type": "string" } },
              "cg": { "type": "string" },
              "bgm": { "type": "string" }
            }
          },
          "rewards": {
            "type": "object",
            "properties": {
              "title": { "type": "string" },
              "unlocks": { "type": "array", "items": { "type": "string" } },
              "achievements": { "type": "array", "items": { "type": "string" } }
            }
          }
        }
      }
    }
  }
}
```

### 3.3 JSON 데이터 (endings.json)

```json
{
  "version": "1.0",
  "endings": [
    {
      "id": "ending_a_freedom",
      "name": "영원한 순환",
      "description": "신의 도움 없이 스스로 운명을 깨뜨리다",
      "theme": "Independence",
      "conditions": {
        "bossDefeated": true,
        "godBlessingUsed": false,
        "minGodFavor": 0,
        "maxGodFavor": 30
      },
      "cutscene": {
        "dialogues": [
          "시간의 수호자가 쓰러졌다.",
          "신들의 축복도, 그 누구의 도움도 없이...",
          "오직 나 자신의 힘으로 운명을 깨뜨렸다.",
          "이제 이 미로는 더 이상 나를 가두지 못한다.",
          "--- 영원한 순환의 끝 ---"
        ],
        "cg": "ending_a_freedom.png",
        "bgm": "ending_freedom.ogg"
      },
      "rewards": {
        "title": "자유의 사도",
        "titleEffect": "모든 종족 시작 스킬 +2",
        "unlocks": [
          "skin_player_freedom",
          "species_dragonborn"
        ],
        "achievements": [
          "achievement_true_freedom",
          "achievement_godless_victory"
        ]
      },
      "specialCondition": "플레이어가 어떤 신의 축복도 받지 않았을 것"
    },
    {
      "id": "ending_b_apostle",
      "name": "신의 대리인",
      "description": "신의 축복을 받아 운명을 바꾸다",
      "theme": "Divine Covenant",
      "conditions": {
        "bossDefeated": true,
        "godBlessingUsed": true,
        "minGodFavor": 100,
        "godPactSigned": true
      },
      "cutscene": {
        "dialogues": [
          "시간의 수호자가 무너졌다.",
          "{GOD_NAME}의 축복이 나를 이끌었다.",
          "이제 나는 신의 대리인으로서 이 세계를 지킬 것이다.",
          "영원한 계약이 맺어졌다...",
          "--- 신의 대리인 ---"
        ],
        "cg": "ending_b_apostle_{GOD_ID}.png",
        "bgm": "ending_divine.ogg"
      },
      "rewards": {
        "title": "신의 사도",
        "titleEffect": "{GOD_NAME}의 영구 축복 (종족 특성 +1)",
        "unlocks": [
          "job_priest_of_{GOD_ID}",
          "species_demigod"
        ],
        "achievements": [
          "achievement_divine_champion",
          "achievement_max_favor"
        ]
      },
      "specialCondition": "특정 신 호감도 100 달성 + 신 계약 체결"
    },
    {
      "id": "ending_c_successor",
      "name": "시간의 계승자",
      "description": "시간의 수호자의 힘을 흡수하다",
      "theme": "Power Absorption",
      "conditions": {
        "bossDefeated": true,
        "specialItemUsed": "hourglass_of_eternity",
        "minSkillLevel": 20
      },
      "cutscene": {
        "dialogues": [
          "시간의 수호자가 쓰러진다.",
          "영원의 모래시계가 빛을 발한다...",
          "나는 보스의 힘을 흡수했다.",
          "이제 나는 새로운 시간의 수호자다.",
          "--- 시간의 계승자 ---"
        ],
        "cg": "ending_c_successor.png",
        "bgm": "ending_time.ogg",
        "additionalEffect": "화면이 역행하며 타임루프 암시"
      },
      "rewards": {
        "title": "시간 지배자",
        "titleEffect": "시간 조작 스킬 영구 획득",
        "unlocks": [
          "newgame_plus_mode",
          "skill_time_stop",
          "species_time_traveler"
        ],
        "achievements": [
          "achievement_time_master",
          "achievement_true_ending"
        ]
      },
      "specialCondition": "5층에서 '영원의 모래시계' 아이템 획득 + 사용"
    }
  ],
  "endingVariations": {
    "species": [
      {
        "speciesId": "human",
        "epilogue": "인간은 고향으로 돌아가 영웅이 되었다."
      },
      {
        "speciesId": "felid",
        "epilogue": "고양이인간은 자유를 찾아 영원히 떠돌았다."
      },
      {
        "speciesId": "dwarf",
        "epilogue": "드워프는 깊은 산맥으로 돌아가 전설을 남겼다."
      }
    ]
  }
}
```

### 3.4 C# 구현 (EndingManager.cs)

```csharp
namespace Game.Systems.Ending
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Core.Events;
    using Game.DataJson.Loaders;

    /// <summary>
    /// 엔딩 조건 평가 및 분기 관리
    /// </summary>
    public class EndingManager : MonoBehaviour
    {
        public static EndingManager Instance { get; private set; }

        [Header("Ending Conditions")]
        [SerializeField] private bool bossDefeated = false;
        [SerializeField] private bool godBlessingUsed = false;
        [SerializeField] private string specialItemUsed = "";
        [SerializeField] private int currentGodFavor = 0;

        [Header("References")]
        [SerializeField] private EndingCutscene cutsceneController;

        private List<EndingData> allEndings;
        private EndingData achievedEnding;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            allEndings = JsonDataLoader.LoadEndingData();

            GameEvents.OnBossDefeated += OnBossDefeated;
            GameEvents.OnGodBlessingUsed += OnGodBlessingUsed;
            GameEvents.OnSpecialItemUsed += OnSpecialItemUsed;
        }

        private void OnDestroy()
        {
            GameEvents.OnBossDefeated -= OnBossDefeated;
            GameEvents.OnGodBlessingUsed -= OnGodBlessingUsed;
            GameEvents.OnSpecialItemUsed -= OnSpecialItemUsed;
        }

        private void OnBossDefeated(string bossId)
        {
            if (bossId == "time_keeper")
            {
                bossDefeated = true;
                EvaluateEnding();
            }
        }

        private void OnGodBlessingUsed(string godId)
        {
            godBlessingUsed = true;
        }

        private void OnSpecialItemUsed(string itemId)
        {
            if (itemId == "hourglass_of_eternity")
            {
                specialItemUsed = itemId;
            }
        }

        /// <summary>
        /// 엔딩 조건 평가 및 결정
        /// </summary>
        public void EvaluateEnding()
        {
            if (!bossDefeated) return;

            EndingData selectedEnding = null;
            int highestPriority = -1;

            foreach (var ending in allEndings)
            {
                if (CheckEndingConditions(ending))
                {
                    int priority = GetEndingPriority(ending.id);
                    if (priority > highestPriority)
                    {
                        highestPriority = priority;
                        selectedEnding = ending;
                    }
                }
            }

            if (selectedEnding != null)
            {
                achievedEnding = selectedEnding;
                TriggerEnding(selectedEnding);
            }
            else
            {
                Debug.LogWarning("[EndingManager] No ending条件 met!");
            }
        }

        private bool CheckEndingConditions(EndingData ending)
        {
            var conditions = ending.conditions;

            // 보스 처치 확인
            if (conditions.bossDefeated && !bossDefeated) return false;

            // 신 축복 확인
            if (conditions.godBlessingUsed != godBlessingUsed) return false;

            // 특수 아이템 확인
            if (!string.IsNullOrEmpty(conditions.specialItemUsed))
            {
                if (conditions.specialItemUsed != specialItemUsed) return false;
            }

            // 신 호감도 확인
            if (conditions.minGodFavor > 0)
            {
                if (currentGodFavor < conditions.minGodFavor) return false;
            }

            return true;
        }

        private int GetEndingPriority(string endingId)
        {
            // 엔딩 우선순위: C > B > A
            return endingId switch
            {
                "ending_c_successor" => 3,
                "ending_b_apostle" => 2,
                "ending_a_freedom" => 1,
                _ => 0
            };
        }

        private void TriggerEnding(EndingData ending)
        {
            Debug.Log($"[EndingManager] Triggering ending: {ending.name}");

            // 엔딩 컷신 재생
            cutsceneController.PlayEnding(ending);

            // 업적 해제
            foreach (var achievementId in ending.rewards.achievements)
            {
                AchievementManager.Instance.UnlockAchievement(achievementId);
            }

            // 언락 적용
            foreach (var unlockId in ending.rewards.unlocks)
            {
                UnlockManager.Instance.Unlock(unlockId);
            }

            // 종족별 엔딩 변화 적용
            ApplySpeciesEpilogue(ending);

            GameEvents.TriggerGameEnding(ending.id);
        }

        private void ApplySpeciesEpilogue(EndingData ending)
        {
            string playerSpecies = PlayerDataManager.Instance.GetSpeciesId();
            // ending.epilogueVariations에서 종족별 에필로그 찾기
            // cutsceneController에 전달
        }

        public EndingData GetAchievedEnding() => achievedEnding;
    }

    // Data structures
    [System.Serializable]
    public class EndingData
    {
        public string id;
        public string name;
        public string description;
        public EndingConditions conditions;
        public CutsceneData cutscene;
        public EndingRewards rewards;
    }

    [System.Serializable]
    public class EndingConditions
    {
        public bool bossDefeated;
        public bool godBlessingUsed;
        public string specialItemUsed;
        public int minGodFavor;
    }

    [System.Serializable]
    public class CutsceneData
    {
        public List<string> dialogues;
        public string cg;
        public string bgm;
    }

    [System.Serializable]
    public class EndingRewards
    {
        public string title;
        public string titleEffect;
        public List<string> unlocks;
        public List<string> achievements;
    }
}
```

---

## 4. 신 시스템 (God System)

### 4.1 신 시스템 설계

DCSS의 신 시스템을 참고하여, **3명의 신**을 제공합니다. 각 신은 고유한 축복과 요구사항을 가집니다.

| 신 | 속성 | 호감도 획득 방법 | 축복 효과 | 금기 |
|------|------|------|------|------|
| **전쟁의 신 (Ares)** | 전투 | 적 처치, 보스 클리어 | 공격력 증가, 치명타 강화 | 도망 (-10 호감도) |
| **마법의 신 (Hecate)** | 마법 | 마법 스킬 사용, 비전서 수집 | 마나 재생, 스킬 레벨 보너스 | 물리 무기 사용 (-5 호감도) |
| **죽음의 신 (Thanatos)** | 생존 | 사망 직전 생존, 언데드 처치 | 부활, 체력 회복 | 회복 아이템 사용 (-3 호감도) |

### 4.2 호감도 시스템

- **호감도 범위**: 0 ~ 100
- **레벨 구분**:
  - 0-20: 인지 (Recognition) - 성소 활성화
  - 21-50: 우호 (Friendly) - 1단계 축복
  - 51-80: 헌신 (Devoted) - 2단계 축복
  - 81-100: 숭배 (Exalted) - 3단계 축복 + 궁극 축복

### 4.3 JSON 스키마 (god_schema.json)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "GodSchema",
  "type": "object",
  "required": ["version", "gods"],
  "properties": {
    "version": { "type": "string" },
    "gods": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "name", "attribute", "favorActions", "taboos", "blessings"],
        "properties": {
          "id": { "type": "string" },
          "name": { "type": "string" },
          "attribute": { "enum": ["combat", "magic", "survival"] },
          "description": { "type": "string" },
          "favorActions": {
            "type": "array",
            "items": {
              "type": "object",
              "properties": {
                "action": { "type": "string" },
                "favorGain": { "type": "integer" }
              }
            }
          },
          "taboos": {
            "type": "array",
            "items": {
              "type": "object",
              "properties": {
                "action": { "type": "string" },
                "favorLoss": { "type": "integer" }
              }
            }
          },
          "blessings": {
            "type": "array",
            "items": {
              "type": "object",
              "properties": {
                "id": { "type": "string" },
                "tier": { "type": "integer", "minimum": 1, "maximum": 3 },
                "requiredFavor": { "type": "integer" },
                "effects": { "type": "array", "items": { "type": "string" } }
              }
            }
          }
        }
      }
    }
  }
}
```

### 4.4 JSON 데이터 (gods.json)

```json
{
  "version": "1.0",
  "gods": [
    {
      "id": "god_of_war",
      "name": "아레스 (Ares)",
      "attribute": "combat",
      "description": "전쟁과 살육의 신. 강력한 전사를 선호한다.",
      "iconPath": "Icons/god_ares",
      "favorActions": [
        { "action": "kill_enemy", "favorGain": 2, "description": "적 처치" },
        { "action": "kill_boss", "favorGain": 20, "description": "보스 처치" },
        { "action": "deal_critical", "favorGain": 1, "description": "치명타 발동" },
        { "action": "clear_room_no_damage", "favorGain": 10, "description": "무피해 룸 클리어" }
      ],
      "taboos": [
        { "action": "flee_combat", "favorLoss": -10, "description": "전투 중 도망" },
        { "action": "use_stealth", "favorLoss": -5, "description": "은신 사용" }
      ],
      "blessings": [
        {
          "id": "war_blessing_tier1",
          "tier": 1,
          "name": "전사의 힘",
          "requiredFavor": 21,
          "cost": 0,
          "effects": [
            "attack_damage +15%",
            "critical_chance +5%"
          ],
          "description": "아레스의 축복이 공격력을 강화한다."
        },
        {
          "id": "war_blessing_tier2",
          "tier": 2,
          "name": "살육의 광기",
          "requiredFavor": 51,
          "cost": 100,
          "effects": [
            "attack_damage +30%",
            "critical_chance +10%",
            "kill_streak_bonus: 연속 처치 시 추가 데미지 (+5% per kill, max 50%)"
          ],
          "description": "적을 처치할수록 강해진다."
        },
        {
          "id": "war_blessing_tier3",
          "tier": 3,
          "name": "전쟁신의 화신",
          "requiredFavor": 81,
          "cost": 300,
          "effects": [
            "attack_damage +50%",
            "critical_chance +20%",
            "berserker_mode: 체력 30% 이하 시 공격속도 +100%, 무적 3초"
          ],
          "description": "아레스의 힘이 완전히 깃든다.",
          "ultimate": true
        }
      ]
    },
    {
      "id": "god_of_magic",
      "name": "헤카테 (Hecate)",
      "attribute": "magic",
      "description": "마법과 지식의 여신. 마법 사용자를 돕는다.",
      "iconPath": "Icons/god_hecate",
      "favorActions": [
        { "action": "cast_spell", "favorGain": 1, "description": "마법 사용" },
        { "action": "discover_spell_book", "favorGain": 15, "description": "비전서 발견" },
        { "action": "kill_with_magic", "favorGain": 3, "description": "마법으로 적 처치" }
      ],
      "taboos": [
        { "action": "use_melee_weapon", "favorLoss": -5, "description": "물리 무기 사용" },
        { "action": "destroy_book", "favorLoss": -20, "description": "비전서 파괴" }
      ],
      "blessings": [
        {
          "id": "magic_blessing_tier1",
          "tier": 1,
          "name": "마나의 흐름",
          "requiredFavor": 21,
          "cost": 0,
          "effects": [
            "mana_regen +50%",
            "spell_cost -10%"
          ]
        },
        {
          "id": "magic_blessing_tier2",
          "tier": 2,
          "name": "비전 숙달",
          "requiredFavor": 51,
          "cost": 100,
          "effects": [
            "magic_skills +3 levels",
            "mana_regen +100%",
            "spell_cooldown -20%"
          ]
        },
        {
          "id": "magic_blessing_tier3",
          "tier": 3,
          "name": "마법여신의 은총",
          "requiredFavor": 81,
          "cost": 300,
          "effects": [
            "magic_skills +5 levels",
            "mana_regen +200%",
            "arcane_overflow: 마나 사용 시 10% 확률로 비용 0"
          ],
          "ultimate": true
        }
      ]
    },
    {
      "id": "god_of_death",
      "name": "타나토스 (Thanatos)",
      "attribute": "survival",
      "description": "죽음과 재생의 신. 죽음에서 돌아온 자를 선호한다.",
      "iconPath": "Icons/god_thanatos",
      "favorActions": [
        { "action": "survive_near_death", "favorGain": 10, "description": "사망 직전 생존 (HP 10% 이하)" },
        { "action": "kill_undead", "favorGain": 3, "description": "언데드 처치" },
        { "action": "resurrect", "favorGain": 25, "description": "부활" }
      ],
      "taboos": [
        { "action": "use_healing_potion", "favorLoss": -3, "description": "회복 포션 사용" },
        { "action": "use_resurrection_item", "favorLoss": -10, "description": "부활 아이템 사용 (신 축복 제외)" }
      ],
      "blessings": [
        {
          "id": "death_blessing_tier1",
          "tier": 1,
          "name": "죽음의 예감",
          "requiredFavor": 21,
          "cost": 0,
          "effects": [
            "health_regen +30%",
            "death_save: 치명타 피격 시 10% 확률로 체력 1 유지"
          ]
        },
        {
          "id": "death_blessing_tier2",
          "tier": 2,
          "name": "불사의 혼",
          "requiredFavor": 51,
          "cost": 100,
          "effects": [
            "health_regen +60%",
            "death_save 30%",
            "second_wind: 체력 0 도달 시 체력 50% 회복 (1회)"
          ]
        },
        {
          "id": "death_blessing_tier3",
          "tier": 3,
          "name": "영혼 불멸",
          "requiredFavor": 81,
          "cost": 300,
          "effects": [
            "resurrection_permanent: 사망 시 자동 부활 (체력 100%, 1회)",
            "undead_command: 처치한 적 20% 확률로 아군 언데드로 부활"
          ],
          "ultimate": true
        }
      ]
    }
  ]
}
```

### 4.5 C# 구현 (GodManager.cs)

```csharp
namespace Game.Systems.God
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Core.Events;
    using Game.DataJson.Loaders;

    /// <summary>
    /// 신 시스템 관리 (호감도, 축복)
    /// </summary>
    public class GodManager : MonoBehaviour
    {
        public static GodManager Instance { get; private set; }

        [Header("God Selection")]
        [SerializeField] private string selectedGodId = "";
        [SerializeField] private int currentFavor = 0;

        [Header("Debug")]
        [SerializeField] private bool logFavorChanges = true;

        private List<GodData> allGods;
        private GodData currentGod;
        private List<Blessing> activeBlessings = new List<Blessing>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            allGods = JsonDataLoader.LoadGodData();

            GameEvents.OnEnemyDied += OnEnemyKilled;
            GameEvents.OnBossDefeated += OnBossKilled;
            GameEvents.OnPlayerNearDeath += OnNearDeath;
        }

        private void OnDestroy()
        {
            GameEvents.OnEnemyDied -= OnEnemyKilled;
            GameEvents.OnBossDefeated -= OnBossKilled;
            GameEvents.OnPlayerNearDeath -= OnNearDeath;
        }

        /// <summary>
        /// 신 선택 (성소에서 호출)
        /// </summary>
        public void SelectGod(string godId)
        {
            currentGod = allGods.Find(g => g.id == godId);
            selectedGodId = godId;
            currentFavor = 0;

            if (logFavorChanges)
            {
                Debug.Log($"[GodManager] Selected god: {currentGod.name}");
            }

            GameEvents.TriggerGodSelected(godId);
        }

        /// <summary>
        /// 호감도 추가
        /// </summary>
        public void AddFavor(string actionId, int amount)
        {
            if (currentGod == null) return;

            currentFavor = Mathf.Clamp(currentFavor + amount, 0, 100);

            if (logFavorChanges)
            {
                Debug.Log($"[GodManager] Favor: {currentFavor}/100 (+{amount} from {actionId})");
            }

            GameEvents.TriggerGodFavorChanged(selectedGodId, currentFavor);

            // 자동 축복 해제 확인
            CheckAutoUnlockBlessings();
        }

        /// <summary>
        /// 호감도 감소 (금기 위반)
        /// </summary>
        public void LoseFavor(string tabooId, int amount)
        {
            if (currentGod == null) return;

            currentFavor = Mathf.Clamp(currentFavor + amount, 0, 100); // amount는 음수

            if (logFavorChanges)
            {
                Debug.LogWarning($"[GodManager] Taboo violated: {tabooId}, Favor: {currentFavor}/100 ({amount})");
            }

            GameEvents.TriggerGodTabooViolated(selectedGodId, tabooId);
        }

        /// <summary>
        /// 축복 활성화
        /// </summary>
        public bool ActivateBlessing(string blessingId)
        {
            if (currentGod == null) return false;

            var blessing = currentGod.blessings.Find(b => b.id == blessingId);
            if (blessing == null) return false;

            // 호감도 확인
            if (currentFavor < blessing.requiredFavor)
            {
                Debug.LogWarning($"[GodManager] Not enough favor for {blessing.name}. Required: {blessing.requiredFavor}, Current: {currentFavor}");
                return false;
            }

            // 골드 소모 확인
            var playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats.GetGold() < blessing.cost)
            {
                Debug.LogWarning($"[GodManager] Not enough gold for {blessing.name}. Required: {blessing.cost}");
                return false;
            }

            // 축복 적용
            playerStats.AddGold(-blessing.cost);
            activeBlessings.Add(blessing);
            ApplyBlessingEffects(blessing);

            if (logFavorChanges)
            {
                Debug.Log($"[GodManager] Blessing activated: {blessing.name}");
            }

            GameEvents.TriggerGodBlessingActivated(blessingId);
            return true;
        }

        private void ApplyBlessingEffects(Blessing blessing)
        {
            foreach (var effect in blessing.effects)
            {
                // Effect parsing: "attack_damage +15%"
                string[] parts = effect.Split(' ');
                string statName = parts[0];
                string modifier = parts[1];

                // PlayerStats나 SkillManager에 적용
                // 예: PlayerStats.ApplyBuff(statName, modifier);
            }
        }

        private void CheckAutoUnlockBlessings()
        {
            // 호감도 달성 시 자동으로 Tier 1 축복 해제 (무료)
            if (currentFavor >= 21 && activeBlessings.Count == 0)
            {
                var tier1Blessing = currentGod.blessings.Find(b => b.tier == 1);
                if (tier1Blessing != null && tier1Blessing.cost == 0)
                {
                    ActivateBlessing(tier1Blessing.id);
                }
            }
        }

        // Event handlers
        private void OnEnemyKilled(GameObject enemy)
        {
            if (currentGod == null) return;

            var favorAction = currentGod.favorActions.Find(a => a.action == "kill_enemy");
            if (favorAction != null)
            {
                AddFavor("kill_enemy", favorAction.favorGain);
            }
        }

        private void OnBossKilled(string bossId)
        {
            if (currentGod == null) return;

            var favorAction = currentGod.favorActions.Find(a => a.action == "kill_boss");
            if (favorAction != null)
            {
                AddFavor("kill_boss", favorAction.favorGain);
            }
        }

        private void OnNearDeath(float healthPercent)
        {
            if (currentGod == null) return;
            if (currentGod.id != "god_of_death") return;

            if (healthPercent <= 0.1f)
            {
                var favorAction = currentGod.favorActions.Find(a => a.action == "survive_near_death");
                if (favorAction != null)
                {
                    AddFavor("survive_near_death", favorAction.favorGain);
                }
            }
        }

        public int GetCurrentFavor() => currentFavor;
        public GodData GetCurrentGod() => currentGod;
        public List<Blessing> GetActiveBlessings() => activeBlessings;
    }

    // Data structures
    [System.Serializable]
    public class GodData
    {
        public string id;
        public string name;
        public string attribute;
        public string description;
        public List<FavorAction> favorActions;
        public List<Taboo> taboos;
        public List<Blessing> blessings;
    }

    [System.Serializable]
    public class FavorAction
    {
        public string action;
        public int favorGain;
    }

    [System.Serializable]
    public class Taboo
    {
        public string action;
        public int favorLoss;
    }

    [System.Serializable]
    public class Blessing
    {
        public string id;
        public int tier;
        public string name;
        public int requiredFavor;
        public int cost;
        public List<string> effects;
    }
}
```

---

## 5. 지식 시스템 (Morgue File)

### 5.1 시스템 개요

DCSS의 Morgue File 시스템을 참고하여, **플레이어의 모든 정보를 기록**합니다.

**기록 내용**:
- 런 통계 (사망 시간, 처치 수, 획득 아이템)
- 몬스터 도감 (조우한 몬스터, 처치 횟수)
- 아이템 도감 (획득한 아이템, 사용 횟수)
- 스킬 성장 기록
- 주요 이벤트 타임라인

### 5.2 JSON 스키마 (knowledge_schema.json)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "KnowledgeSchema",
  "type": "object",
  "required": ["version", "runHistory", "monsterKnowledge", "itemKnowledge"],
  "properties": {
    "version": { "type": "string" },
    "runHistory": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "runId": { "type": "string" },
          "timestamp": { "type": "string" },
          "species": { "type": "string" },
          "job": { "type": "string" },
          "deathReason": { "type": "string" },
          "floorReached": { "type": "integer" },
          "totalKills": { "type": "integer" },
          "playTime": { "type": "number" }
        }
      }
    },
    "monsterKnowledge": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "monsterId": { "type": "string" },
          "firstEncounter": { "type": "string" },
          "encounterCount": { "type": "integer" },
          "killCount": { "type": "integer" },
          "deathCount": { "type": "integer" }
        }
      }
    },
    "itemKnowledge": {
      "type": "array",
      "items": {
        "type": "object"},
        "properties": {
          "itemId": { "type": "string" },
          "firstFound": { "type": "string" },
          "foundCount": { "type": "integer" },
          "usedCount": { "type": "integer" }
        }
      }
    }
  }
}
```

### 5.3 C# 구현 (MorgueFileGenerator.cs)

```csharp
namespace Game.Systems.Knowledge
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// DCSS 스타일 Morgue File 생성 (사망/클리어 시 기록)
    /// </summary>
    public class MorgueFileGenerator : MonoBehaviour
    {
        public static MorgueFileGenerator Instance { get; private set; }

        [Header("File Settings")]
        [SerializeField] private string morgueDirectory = "MorgueFiles";

        private RunData currentRun;
        private float runStartTime;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            currentRun = new RunData
            {
                runId = Guid.NewGuid().ToString(),
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                species = PlayerDataManager.Instance.GetSpeciesId(),
                job = PlayerDataManager.Instance.GetJobId()
            };

            runStartTime = Time.time;

            GameEvents.OnPlayerDeath += OnPlayerDeath;
            GameEvents.OnGameEnding += OnGameEnding;
        }

        private void OnDestroy()
        {
            GameEvents.OnPlayerDeath -= OnPlayerDeath;
            GameEvents.OnGameEnding -= OnGameEnding;
        }

        private void OnPlayerDeath()
        {
            currentRun.deathReason = "사망: " + GetDeathReason();
            currentRun.playTime = Time.time - runStartTime;
            currentRun.success = false;

            GenerateMorgueFile();
        }

        private void OnGameEnding(string endingId)
        {
            currentRun.deathReason = "클리어: " + endingId;
            currentRun.playTime = Time.time - runStartTime;
            currentRun.success = true;

            GenerateMorgueFile();
        }

        /// <summary>
        /// Morgue 파일 생성 및 저장
        /// </summary>
        private void GenerateMorgueFile()
        {
            string fileName = $"morgue_{currentRun.timestamp.Replace(":", "-").Replace(" ", "_")}.txt";
            string filePath = Path.Combine(Application.persistentDataPath, morgueDirectory, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            string content = BuildMorgueContent();
            File.WriteAllText(filePath, content);

            Debug.Log($"[MorgueFile] Saved to: {filePath}");
        }

        private string BuildMorgueContent()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("=== Labyrinth Morgue File ===");
            sb.AppendLine($"Run ID: {currentRun.runId}");
            sb.AppendLine($"Timestamp: {currentRun.timestamp}");
            sb.AppendLine($"Species: {currentRun.species}");
            sb.AppendLine($"Job: {currentRun.job}");
            sb.AppendLine($"Result: {currentRun.deathReason}");
            sb.AppendLine($"Floor Reached: {currentRun.floorReached}");
            sb.AppendLine($"Play Time: {FormatTime(currentRun.playTime)}");
            sb.AppendLine();

            sb.AppendLine("=== Statistics ===");
            sb.AppendLine($"Total Kills: {currentRun.totalKills}");
            sb.AppendLine($"Bosses Defeated: {currentRun.bossesKilled}");
            sb.AppendLine($"Gold Collected: {currentRun.goldCollected}");
            sb.AppendLine($"Items Found: {currentRun.itemsFound}");
            sb.AppendLine();

            sb.AppendLine("=== Skills (Final) ===");
            foreach (var skill in currentRun.finalSkills)
            {
                sb.AppendLine($"  {skill.Key}: Level {skill.Value}");
            }
            sb.AppendLine();

            sb.AppendLine("=== Equipment (Final) ===");
            foreach (var item in currentRun.finalEquipment)
            {
                sb.AppendLine($"  {item}");
            }
            sb.AppendLine();

            sb.AppendLine("=== Event Timeline ===");
            foreach (var evt in currentRun.eventTimeline)
            {
                sb.AppendLine($"  [{evt.timestamp}] {evt.description}");
            }

            return sb.ToString();
        }

        private string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes}m {secs}s";
        }

        private string GetDeathReason()
        {
            // 최근 피격 정보 반환
            return "Unknown"; // 실제로는 GameEvents에서 마지막 공격자 추적
        }

        public void RecordEvent(string description)
        {
            currentRun.eventTimeline.Add(new RunEvent
            {
                timestamp = FormatTime(Time.time - runStartTime),
                description = description
            });
        }
    }

    [System.Serializable]
    public class RunData
    {
        public string runId;
        public string timestamp;
        public string species;
        public string job;
        public string deathReason;
        public int floorReached;
        public float playTime;
        public bool success;
        public int totalKills;
        public int bossesKilled;
        public int goldCollected;
        public int itemsFound;
        public Dictionary<string, int> finalSkills = new Dictionary<string, int>();
        public List<string> finalEquipment = new List<string>();
        public List<RunEvent> eventTimeline = new List<RunEvent>();
    }

    [System.Serializable]
    public class RunEvent
    {
        public string timestamp;
        public string description;
    }
}
```

---

## 6. 업적 시스템

### 6.1 업적 카테고리

**총 30+ 업적**을 5개 카테고리로 분류합니다.

| 카테고리 | 업적 예시 | 조건 |
|---------|---------|------|
| **진행도** | 첫 승리, 모든 엔딩 클리어 | 엔딩 달성 |
| **전투** | 100 킬, 무피해 보스 클리어 | 전투 관련 |
| **수집** | 모든 룬 획득, 전설 아이템 획득 | 아이템 수집 |
| **신** | 3신 모두 호감도 100 달성 | 신 시스템 |
| **챌린지** | 1시간 내 클리어, 장비 없이 클리어 | 난이도 도전 |

### 6.2 JSON 데이터 (achievements.json)

```json
{
  "version": "1.0",
  "achievements": [
    {
      "id": "achievement_first_victory",
      "name": "첫 승리",
      "description": "미로를 탈출하다",
      "category": "progression",
      "condition": {
        "type": "game_clear",
        "count": 1
      },
      "reward": {
        "title": "탈출자",
        "icon": "achievement_first_victory"
      }
    },
    {
      "id": "achievement_true_freedom",
      "name": "진정한 자유",
      "description": "신의 도움 없이 엔딩 A 달성",
      "category": "progression",
      "condition": {
        "type": "ending_reached",
        "endingId": "ending_a_freedom"
      },
      "reward": {
        "title": "자유의 사도",
        "icon": "achievement_freedom"
      }
    },
    {
      "id": "achievement_godless_victory",
      "name": "무신론자",
      "description": "어떤 신도 선택하지 않고 클리어",
      "category": "challenge",
      "condition": {
        "type": "game_clear",
        "additionalCondition": "no_god_selected"
      },
      "reward": {
        "title": "무신론자",
        "icon": "achievement_godless"
      }
    },
    {
      "id": "achievement_100_kills",
      "name": "학살자",
      "description": "총 100마리 처치",
      "category": "combat",
      "condition": {
        "type": "total_kills",
        "count": 100
      },
      "reward": {
        "icon": "achievement_kills"
      }
    },
    {
      "id": "achievement_no_damage_boss",
      "name": "완벽한 전투",
      "description": "보스를 무피해로 처치",
      "category": "combat",
      "condition": {
        "type": "boss_kill_no_damage",
        "count": 1
      },
      "reward": {
        "icon": "achievement_perfect"
      }
    },
    {
      "id": "achievement_all_runes",
      "name": "룬 수집가",
      "description": "모든 룬 획득",
      "category": "collection",
      "condition": {
        "type": "collect_all_runes",
        "runeCount": 4
      },
      "reward": {
        "icon": "achievement_runes"
      }
    },
    {
      "id": "achievement_max_favor_all_gods",
      "name": "신들의 총애",
      "description": "3신 모두 호감도 100 달성 (런 누적)",
      "category": "god",
      "condition": {
        "type": "max_favor_all_gods",
        "godCount": 3
      },
      "reward": {
        "title": "신들의 총애",
        "icon": "achievement_gods"
      }
    },
    {
      "id": "achievement_speedrun",
      "name": "시간 정복자",
      "description": "30분 내 클리어",
      "category": "challenge",
      "condition": {
        "type": "clear_time",
        "maxSeconds": 1800
      },
      "reward": {
        "title": "시간 정복자",
        "icon": "achievement_speedrun"
      }
    }
  ]
}
```

### 6.3 C# 구현 (AchievementManager.cs)

```csharp
namespace Game.Systems.Achievement
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Core.Events;
    using Game.DataJson.Loaders;

    /// <summary>
    /// 업적 시스템 관리
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Progress")]
        [SerializeField] private List<string> unlockedAchievements = new List<string>();

        private List<AchievementData> allAchievements;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            allAchievements = JsonDataLoader.LoadAchievementData();
            LoadProgress();

            GameEvents.OnEnemyDied += CheckKillAchievements;
            GameEvents.OnBossDefeated += CheckBossAchievements;
            GameEvents.OnGameEnding += CheckEndingAchievements;
        }

        public void UnlockAchievement(string achievementId)
        {
            if (unlockedAchievements.Contains(achievementId)) return;

            unlockedAchievements.Add(achievementId);
            SaveProgress();

            var achievement = allAchievements.Find(a => a.id == achievementId);
            if (achievement != null)
            {
                Debug.Log($"[Achievement] Unlocked: {achievement.name}");
                GameEvents.TriggerAchievementUnlocked(achievementId);
                ShowAchievementPopup(achievement);
            }
        }

        private void CheckKillAchievements(GameObject enemy)
        {
            int totalKills = PlayerDataManager.Instance.GetTotalKills();
            if (totalKills >= 100)
            {
                UnlockAchievement("achievement_100_kills");
            }
        }

        private void CheckBossAchievements(string bossId)
        {
            // 무피해 보스 처치 확인
            bool noDamageTaken = /* 체크 */;
            if (noDamageTaken)
            {
                UnlockAchievement("achievement_no_damage_boss");
            }
        }

        private void CheckEndingAchievements(string endingId)
        {
            UnlockAchievement($"achievement_{endingId}");

            // 스피드런 확인
            float playTime = MorgueFileGenerator.Instance.currentRun.playTime;
            if (playTime < 1800f)
            {
                UnlockAchievement("achievement_speedrun");
            }
        }

        private void ShowAchievementPopup(AchievementData achievement)
        {
            // UI 팝업 표시
        }

        private void SaveProgress()
        {
            // PlayerPrefs 또는 JSON 파일로 저장
        }

        private void LoadProgress()
        {
            // 저장된 업적 목록 로드
        }

        public List<AchievementData> GetAllAchievements() => allAchievements;
        public bool IsUnlocked(string achievementId) => unlockedAchievements.Contains(achievementId);
    }

    [System.Serializable]
    public class AchievementData
    {
        public string id;
        public string name;
        public string description;
        public string category;
        public AchievementCondition condition;
        public AchievementReward reward;
    }

    [System.Serializable]
    public class AchievementCondition
    {
        public string type;
        public int count;
        public string additionalCondition;
    }

    [System.Serializable]
    public class AchievementReward
    {
        public string title;
        public string icon;
    }
}
```

---

## 7. 스토리 완성

### 7.1 스토리 개요

**Labyrinth의 스토리** (GAME_DESIGN.md 기반 확장):

```
과거, 고대 문명이 "시간의 수호자"를 봉인하기 위해 미로를 건설했다.
수호자는 시간을 조종하여 세계를 영원한 순환에 가두려 했다.

플레이어는 미로에 갇힌 영혼으로, 탈출하려면 수호자를 처치해야 한다.

3가지 선택:
1. 신의 힘을 빌린다 (엔딩 B) - 신의 대리인이 되어 미로 수호
2. 스스로 극복한다 (엔딩 A) - 자유를 얻어 미로 파괴
3. 수호자의 힘을 흡수한다 (엔딩 C) - 새로운 수호자가 되어 타임루프
```

### 7.2 종족별 스토리 변화

각 종족은 **엔딩 에필로그**가 다릅니다 (endings.json의 `endingVariations` 참조).

| 종족 | 엔딩 A 에필로그 | 엔딩 B 에필로그 | 엔딩 C 에필로그 |
|------|------|------|------|
| **인간** | 고향으로 돌아가 영웅이 됨 | 신전을 세우고 성직자가 됨 | 영원히 미로를 관리하는 수호자 |
| **드워프** | 산맥으로 돌아가 전설을 남김 | 전쟁의 신을 섬기는 전사단 창설 | 미로 깊숙이 새 왕국 건설 |
| **엘프** | 숲으로 돌아가 평화를 되찾음 | 마법의 신을 섬기는 현자 | 시간 마법을 연구하는 대마법사 |
| **고양이인간** | 자유를 찾아 영원히 떠돌음 | 죽음의 신과 계약, 9개 목숨 획득 | 시간을 넘나드는 방랑자 |
| **하프오크** | 부족으로 돌아가 족장이 됨 | 전쟁의 신 군단 지휘관 | 미로의 괴물을 통솔하는 군주 |

### 7.3 구현

```csharp
// EndingCutscene.cs에서 종족별 에필로그 로드
public void PlayEnding(EndingData ending)
{
    string speciesId = PlayerDataManager.Instance.GetSpeciesId();
    string epilogue = GetSpeciesEpilogue(ending.id, speciesId);

    // 기본 엔딩 다이얼로그 재생
    foreach (var dialogue in ending.cutscene.dialogues)
    {
        ShowDialogue(dialogue);
    }

    // 종족별 에필로그 추가
    ShowDialogue(epilogue);

    // CG 표시
    ShowCG(ending.cutscene.cg);

    // BGM 재생
    PlayBGM(ending.cutscene.bgm);
}
```

---

## 8. 종족/직업 확장

### 8.1 Phase 5 종족 추가

Phase 4에서 5종족이었던 것을 **8종족**으로 확장합니다.

| 종족 | 특성 | 시작 능력치 | 추천 직업 |
|------|------|------|------|
| **드래곤본 (Dragonborn)** (NEW) | 화염 면역, 브레스 공격 | HP 120, 방어력 +2 | 전사, 스펠블레이드 |
| **반신 (Demigod)** (NEW) | 모든 스탯 소폭 증가 | HP 100, Mana 100, 모든 스킬 +1 | 모든 직업 |
| **시간 여행자 (Time Traveler)** (NEW) | 시간 조작 스킬 | HP 80, 시간 되돌리기 (1회) | 도적, 마법사 |

### 8.2 Phase 5 직업 추가

7개 직업을 **10개 직업**으로 확장합니다.

| 직업 | 설명 | 시작 장비 | 시작 스킬 |
|------|------|------|------|
| **암살자 (Assassin)** (NEW) | 은신 + 치명타 | 단검 +1, 연막탄 x3 | 은신 Lv5, 치명타 Lv5 |
| **소환사 (Summoner)** (NEW) | 몬스터 소환 | 소환 지팡이, 마나 포션 x2 | 소환 Lv7 |
| **성기사 (Paladin)** (NEW) | 신 축복 + 방어 | 메이스 +1, 대형 방패 | 방어 Lv5, 신앙 Lv3 |

### 8.3 JSON 업데이트

```json
// species.json에 추가
{
  "id": "dragonborn",
  "displayName": "드래곤본",
  "description": "고대 용의 후손. 화염에 면역이며 강력한 브레스 공격을 사용한다.",
  "stats": {
    "maxHealth": 120,
    "maxMana": 60,
    "moveSpeed": 2.5
  },
  "racialAbilities": [
    {
      "id": "fire_breath",
      "name": "화염 브레스",
      "cooldown": 10,
      "damage": 50,
      "effect": "전방 원뿔 범위 화염 공격"
    },
    {
      "id": "fire_immunity",
      "name": "화염 면역",
      "effect": "화염 피해 100% 감소"
    }
  ]
}
```

---

## 9. Definition of Done

Phase 5가 완료되었다고 판단하는 기준:

- [ ] **최종 층 (5층) 구현 완료**
  - 61x61 그리드 생성
  - 4개 날개 (북, 동, 남, 서) 구조
  - 중앙 보스 룸 원형 구조
  - 4개 미니보스 배치 및 처치 가능
  - Vault 룸 2개 이상 생성
  - 성소 3개 배치

- [ ] **최종 보스 AI 구현 완료**
  - 5단계 페이즈 전환 (체력 80/60/40/20/10%)
  - 각 페이즈별 고유 패턴 작동
  - Phase 1: 과거 회귀 (위치 기록, 순간이동)
  - Phase 2: 시간 정지 (플레이어 프리즈)
  - Phase 3: 미래 예지 (공격 예고, 회피)
  - Phase 4: 영원 순환 (재생 1회, 복제)
  - Phase 5: 시간의 끝 (균열, 체력 1 감소, 카운트다운 즉사)
  - 보스 처치 시 엔딩 평가 시작

- [ ] **멀티 엔딩 시스템 구현 완료**
  - 3개 엔딩 (A: 자유, B: 신 대리인, C: 시간 계승자) 조건 평가
  - 엔딩별 컷신 재생 (다이얼로그, CG, BGM)
  - 종족별 에필로그 변화 적용
  - 엔딩 보상 (타이틀, 언락, 업적) 지급

- [ ] **신 시스템 구현 완료**
  - 3신 (전쟁, 마법, 죽음) 선택 가능
  - 호감도 0-100 시스템 작동
  - 행동에 따른 호감도 증감
  - 금기 위반 시 호감도 감소
  - 3단계 축복 (Tier 1/2/3) 활성화
  - 궁극 축복 (호감도 81+) 적용

- [ ] **지식 시스템 구현 완료**
  - Morgue File 생성 (사망/클리어 시)
  - 런 통계 기록 (사망 시간, 처치 수, 아이템, 플레이 타임)
  - 몬스터 도감 (조우 기록, 처치 횟수)
  - 아이템 도감 (획득 기록, 사용 횟수)
  - 이벤트 타임라인 기록

- [ ] **업적 시스템 구현 완료**
  - 30+ 업적 정의 (JSON)
  - 5개 카테고리 (진행도, 전투, 수집, 신, 챌린지)
  - 업적 조건 평가 자동화
  - 업적 해제 시 UI 팝업 표시
  - 업적 보상 (타이틀, 아이콘) 지급
  - 업적 진행도 저장/로드

- [ ] **스토리 완성**
  - 종족별 엔딩 에필로그 작성 (8종족 x 3엔딩 = 24개)
  - 주요 이벤트 다이얼로그 작성 (5층 진입, 미니보스, 최종보스)
  - 엔딩 CG 플레이스홀더 설정

- [ ] **종족/직업 확장 완료**
  - 8종족 구현 (기존 5 + 신규 3)
  - 10직업 구현 (기존 7 + 신규 3)
  - 신규 종족 특성 작동 (드래곤본 화염 브레스 등)
  - 신규 직업 시작 장비/스킬 적용

- [ ] **JSON 데이터 완성**
  - final_floor.json
  - endings.json
  - gods.json
  - blessings.json
  - achievements.json
  - monsters_final.json (5층 전용 몬스터)

- [ ] **통합 테스트 통과**
  - 5층 진입 가능 (룬 2개 이상 획득 조건)
  - 미니보스 4개 처치 후 최종보스 진입
  - 최종보스 5단계 페이즈 모두 정상 작동
  - 3개 엔딩 모두 달성 가능
  - 신 시스템 호감도 100 달성 가능
  - Morgue File 정상 생성
  - 업적 30개 이상 해제 가능

---

## 10. 성능 및 리스크 체크리스트

### 10.1 성능 최적화

- [ ] **5층 메쉬 최적화**
  - 61x61 그리드 = 3,721 타일 → Static Batching 필수
  - Tilemap 사용 검토 (GameObject보다 경량)
  - 플레이어 시야 밖 타일 컬링

- [ ] **최종 보스 페이즈 전환 최적화**
  - 페이즈 전환 시 Object Pooling (환영, 투사체)
  - 파티클 이펙트 Max Particles 제한 (1000개 이하)
  - 코루틴 대신 Update 기반 타이머 (GC 최소화)

- [ ] **Morgue File I/O 최적화**
  - 파일 저장 비동기 처리 (`async/await`)
  - 저장 크기 제한 (최대 10MB)
  - 오래된 파일 자동 삭제 (30개 이상 시)

- [ ] **업적 조건 평가 최적화**
  - 매 프레임 체크 방지 → 이벤트 기반
  - 조건 캐싱 (이미 해제된 업적 재평가 안 함)

### 10.2 리스크 관리

- [ ] **5층 생성 시간**
  - 리스크: 61x61 생성 시 1초 이상 소요 가능
  - 대응: 코루틴으로 분할 생성 + 로딩 화면

- [ ] **최종 보스 난이도 밸런스**
  - 리스크: 5단계 페이즈가 너무 어렵거나 쉬울 수 있음
  - 대응: 각 페이즈별 데미지/체력 조정 가능하도록 JSON 파라미터화

- [ ] **멀티 엔딩 조건 충돌**
  - 리스크: 여러 엔딩 조건이 동시에 만족될 수 있음
  - 대응: 우선순위 시스템 (C > B > A)

- [ ] **신 시스템 밸런스**
  - 리스크: 특정 신이 너무 강력할 수 있음
  - 대응: Phase 6에서 밸런스 패치 예정

- [ ] **Morgue File 저장 실패**
  - 리스크: 권한 없는 디렉터리 접근, 디스크 용량 부족
  - 대응: try-catch + 로그 기록, 대체 경로 (PlayerPrefs JSON)

### 10.3 테스트 전략

- [ ] **5층 구조 테스트**
  - 시드 고정 (예: 12345) → 항상 동일한 5층 생성 확인
  - 4개 날개 모두 미니보스 룸 도달 가능 확인

- [ ] **최종 보스 페이즈 테스트**
  - 치트 키로 보스 체력 단계별 감소 → 페이즈 전환 확인
  - 각 페이즈 패턴 로그 출력 → 타이밍 검증

- [ ] **엔딩 분기 테스트**
  - 엔딩 A: 신 선택 안 함 → 보스 처치
  - 엔딩 B: 신 호감도 100 → 축복 사용 → 보스 처치
  - 엔딩 C: 영원의 모래시계 획득 → 보스에게 사용

- [ ] **신 호감도 테스트**
  - 적 10마리 처치 → 전쟁의 신 호감도 +20 확인
  - 도망 1회 → 전쟁의 신 호감도 -10 확인

- [ ] **업적 테스트**
  - 각 업적 조건 강제 트리거 → 해제 확인
  - 업적 저장/로드 → 재시작 후에도 유지 확인

---

## 11. Phase 6 로드맵

Phase 5 완료 후 Phase 6에서 다룰 내용:

### 11.1 콘텐츠 완성

- **종족 10개로 확장** (현재 8 → 목표 10)
- **직업 10개 유지** (다양성 확보)
- **몬스터 50+ 종류** (브랜치별 15종 x 3 + 5층 5종)
- **아이템 300+ 개** (장비 100, 소비품 100, 아티팩트 50, 기타 50)
- **전설 아이템 20개** (각 브랜치/신/엔딩 보상)

### 11.2 밸런스 튜닝

- **스킬 레벨 곡선 조정** (0-27 레벨 XP 밸런스)
- **브랜치 난이도 균형** (각 브랜치가 비슷한 난이도)
- **신 축복 밸런스** (3신이 비슷한 강도)
- **보스 체력/공격력 조정** (플레이테스트 기반)

### 11.3 온라인 리더보드

- **클리어 타임 순위** (스피드런 리더보드)
- **최단 층수 클리어** (챌린지 모드)
- **총 플레이 통계** (전체 플레이어 합산 킬 수 등)

### 11.4 고급 통계

- **종족/직업별 승률** (메타 분석)
- **스킬 사용 빈도** (인기 빌드 분석)
- **아이템 픽률** (밸런스 참고)

### 11.5 Steam 출시 준비

- **Steam SDK 통합**
- **업적 → Steam 업적 연동**
- **클라우드 세이브**
- **스크린샷 시스템**
- **트레이딩 카드 (선택)**

### 11.6 현지화 (Localization)

- **한글 (기본)**
- **영어 추가** (글로벌 출시)
- **UI 텍스트 번역 시스템** (CSV 기반)

---

## 마무리

Phase 5는 게임의 **엔드 콘텐츠와 엔딩**을 완성하는 단계입니다. 최종 층, 최종 보스, 멀티 엔딩, 신 시스템, 지식 시스템, 업적 시스템이 모두 통합되어, 플레이어에게 **다양한 플레이 경험과 리플레이 가치**를 제공합니다.

**DoD 10개 항목**을 모두 체크하고, **성능/리스크 체크리스트**를 통과하면 Phase 5 완료입니다.

다음 Phase 6에서는 **콘텐츠 확장, 밸런스 튜닝, Steam 출시 준비**를 진행합니다.

---

**End of Phase 5 Request Document**
