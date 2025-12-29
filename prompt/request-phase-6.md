# Phase 6 Implementation Request - 콘텐츠 완성 및 출시 준비

> **작성일**: 2025-12-30
> **대상**: Claude Code
> **목적**: Phase 6 (콘텐츠 완성, 밸런스, Steam 출시) 구현 상세 가이드

---

## 📋 목차

0. [아키텍처 개요](#0-아키텍처-개요)
1. [종족/직업 완성 (10종족 10직업)](#1-종족직업-완성-10종족-10직업)
2. [몬스터 콘텐츠 확장 (50+)](#2-몬스터-콘텐츠-확장-50)
3. [아이템 콘텐츠 확장 (300+)](#3-아이템-콘텐츠-확장-300)
4. [밸런스 튜닝 시스템](#4-밸런스-튜닝-시스템)
5. [온라인 리더보드](#5-온라인-리더보드)
6. [고급 통계 시스템](#6-고급-통계-시스템)
7. [Steam SDK 통합](#7-steam-sdk-통합)
8. [현지화 시스템 (한글/영어)](#8-현지화-시스템-한글영어)
9. [Definition of Done](#9-definition-of-done)
10. [성능 및 출시 체크리스트](#10-성능-및-출시-체크리스트)

---

## 0. 아키텍처 개요

### Phase 5 → Phase 6 전환

```
Phase 5 완료 상태:
├── 5층 "영원의 전당" 구현
├── 최종 보스 (5단계 페이즈)
├── 멀티 엔딩 (3개)
├── 신 시스템 (3신)
├── 지식 시스템 (Morgue File)
├── 업적 시스템 (30+)
└── 종족 8개, 직업 10개

Phase 6 추가 요구사항:
├── 종족 10개 완성
├── 몬스터 50+ 종류
├── 아이템 300+ 개
├── 밸런스 튜닝 시스템
├── 온라인 리더보드
├── 고급 통계 (메타 분석)
├── Steam SDK 통합
└── 현지화 (한글/영어)
```

### 아키텍처 다이어그램

```
Assets/_Project/
├── Systems/
│   ├── Balance/
│   │   ├── BalanceConfigManager.cs (밸런스 파라미터 관리)
│   │   ├── DifficultyScaler.cs (난이도 조정)
│   │   └── PlaytestAnalyzer.cs (플레이테스트 데이터 분석)
│   ├── Online/
│   │   ├── LeaderboardManager.cs (리더보드)
│   │   ├── CloudSaveManager.cs (클라우드 세이브)
│   │   └── OnlineStatsCollector.cs (통계 수집)
│   ├── Localization/
│   │   ├── LocalizationManager.cs (현지화 관리)
│   │   ├── LanguageLoader.cs (언어 파일 로드)
│   │   └── TextLocalizer.cs (텍스트 현지화 컴포넌트)
│   └── Steam/
│       ├── SteamManager.cs (Steam SDK 래퍼)
│       ├── SteamAchievements.cs (Steam 업적 연동)
│       └── SteamStats.cs (Steam 통계 연동)
│
├── DataJson/
│   ├── Schemas/
│   │   ├── species_full_schema.json (10종족)
│   │   ├── job_full_schema.json (10직업)
│   │   ├── monster_full_schema.json (50+)
│   │   ├── item_full_schema.json (300+)
│   │   ├── balance_schema.json
│   │   └── localization_schema.json
│   └── Loaders/
│       ├── FullContentLoader.cs (전체 콘텐츠 로드)
│       └── LocalizationLoader.cs (번역 로드)
│
└── UI/
    ├── LeaderboardPanel.cs (리더보드 UI)
    ├── StatsPanel.cs (통계 UI)
    └── LanguageSelector.cs (언어 선택 UI)

StreamingAssets/GameData/
├── species_full.json (10종족 완전판)
├── jobs_full.json (10직업 완전판)
├── monsters/ (50+ 몬스터)
│   ├── monsters_common.json
│   ├── monsters_branch_bone.json
│   ├── monsters_branch_fire.json
│   ├── monsters_branch_poison.json
│   └── monsters_final.json
├── items/ (300+ 아이템)
│   ├── items_weapons.json (100)
│   ├── items_armor.json (50)
│   ├── items_consumables.json (100)
│   └── items_artifacts.json (50)
├── balance.json (밸런스 파라미터)
└── localization/
    ├── en.csv (영어)
    └── ko.csv (한글)

External/
└── Steamworks.NET/ (Steam SDK)
```

---

## 1. 종족/직업 완성 (10종족 10직업)

### 1.1 종족 2개 추가 (현재 8 → 목표 10)

Phase 5에서 8종족까지 완성했으므로, **2종족 추가**합니다.

| 종족 | 특성 | 시작 능력치 | 종족 스킬 | 추천 직업 |
|------|------|------|------|------|
| **뱀파이어 (Vampire)** | 생명력 흡수, 햇빛 약화 | HP 90, Mana 80 | 생명력 흡수 (공격 시 25% 회복), 야간 강화 (+20% 공격력) | 도적, 마법사 |
| **고블린 (Goblin)** | 작은 체구, 빠른 속도 | HP 70, 속도 +30% | 재빠른 회피 (+15% 회피율), 함정 설치 | 도적, 사냥꾼 |

### 1.2 JSON 데이터 (species_full.json)

```json
{
  "version": "2.0",
  "species": [
    {
      "id": "vampire",
      "displayName": "뱀파이어",
      "description": "불사의 존재. 생명력을 흡수하지만 햇빛에 약하다.",
      "stats": {
        "maxHealth": 90,
        "maxMana": 80,
        "moveSpeed": 2.5,
        "attackDamage": 15
      },
      "racialAbilities": [
        {
          "id": "lifesteal",
          "name": "생명력 흡수",
          "description": "모든 공격이 25%의 피해를 체력으로 회복",
          "passive": true,
          "effect": {
            "type": "lifesteal",
            "value": 0.25
          }
        },
        {
          "id": "night_power",
          "name": "야간 강화",
          "description": "어두운 지역에서 공격력 +20%",
          "passive": true,
          "effect": {
            "type": "conditional_buff",
            "condition": "in_dark_area",
            "stat": "attack_damage",
            "value": 0.2
          }
        }
      ],
      "weaknesses": [
        {
          "id": "sunlight_weakness",
          "description": "밝은 지역에서 체력 재생 -50%"
        }
      ],
      "startingSkills": {
        "combat_melee": 3,
        "survival_stealth": 5
      }
    },
    {
      "id": "goblin",
      "displayName": "고블린",
      "description": "작고 빠른 종족. 함정을 다루는 데 능숙하다.",
      "stats": {
        "maxHealth": 70,
        "maxMana": 60,
        "moveSpeed": 3.5,
        "attackDamage": 12
      },
      "racialAbilities": [
        {
          "id": "quick_dodge",
          "name": "재빠른 회피",
          "description": "회피율 +15%",
          "passive": true,
          "effect": {
            "type": "dodge_chance",
            "value": 0.15
          }
        },
        {
          "id": "trap_mastery",
          "name": "함정 설치",
          "description": "함정 설치 가능 (쿨다운 30초)",
          "cooldown": 30,
          "effect": {
            "type": "spawn_trap",
            "trapType": "spike_trap",
            "damage": 30
          }
        }
      ],
      "startingSkills": {
        "combat_ranged": 5,
        "misc_traps": 7
      }
    }
  ]
}
```

### 1.3 직업 밸런스 조정

10개 직업이 모두 **비슷한 승률**을 가지도록 시작 장비/스킬 조정.

**밸런스 목표**:
- 모든 직업 승률 40-60% (플레이테스트 후)
- 초보자 친화 직업: 전사, 사냥꾼 (승률 55%+)
- 고난이도 직업: 암살자, 버서커 (승률 45%)

```json
// jobs_full.json - 밸런스 조정 예시
{
  "id": "fighter",
  "buffs": [
    "starting_health +20 (기존 대비)",
    "starting_armor +1"
  ],
  "reason": "플레이테스트에서 초반 생존률 낮음"
},
{
  "id": "berserker",
  "nerfs": [
    "berserker_rage duration 10초 → 8초"
  ],
  "reason": "플레이테스트에서 무적 시간이 너무 길어 밸런스 붕괴"
}
```

---

## 2. 몬스터 콘텐츠 확장 (50+)

### 2.1 몬스터 분포

**총 50+ 몬스터**를 다음과 같이 분배합니다:

| 구역 | 몬스터 수 | 난이도 | 특징 |
|------|------|------|------|
| **1층 (망각의 회랑)** | 10종 | Easy | 기본 몬스터, 튜토리얼 |
| **2층 (뼈의 미로)** | 15종 | Normal | 언데드 테마 |
| **3층 (화염의 심연)** | 15종 | Normal | 화염 테마 |
| **4층 (맹독의 정원)** | 15종 | Normal | 독 테마 |
| **5층 (영원의 전당)** | 10종 | Hard | 시간 테마 |
| **보스/특수** | 5종 | Boss | 각 층 보스 |

### 2.2 신규 몬스터 예시 (50종 중 10종)

#### 2층 - 뼈의 미로 (언데드 테마)
```json
{
  "id": "bone_knight",
  "displayName": "뼈의 기사",
  "archetype": "Melee",
  "stats": {
    "maxHealth": 100,
    "moveSpeed": 2.0,
    "attackDamage": 20
  },
  "abilities": [
    {
      "id": "shield_bash",
      "name": "방패 강타",
      "cooldown": 8,
      "damage": 30,
      "effect": "스턴 2초"
    }
  ],
  "loot": {
    "gold": { "min": 20, "max": 40 },
    "items": [
      { "id": "bone_sword", "chance": 0.1 },
      { "id": "health_potion_small", "chance": 0.3 }
    ]
  }
},
{
  "id": "ghost_mage",
  "displayName": "유령 마법사",
  "archetype": "Ranged",
  "stats": {
    "maxHealth": 60,
    "moveSpeed": 1.5,
    "attackDamage": 25
  },
  "abilities": [
    {
      "id": "ethereal_bolt",
      "name": "유령 화살",
      "range": 8,
      "damage": 25,
      "effect": "관통 (벽 통과)"
    },
    {
      "id": "phase_shift",
      "name": "위상 이동",
      "cooldown": 15,
      "effect": "2초간 무적 + 벽 통과"
    }
  ]
}
```

#### 3층 - 화염의 심연
```json
{
  "id": "fire_elemental",
  "displayName": "화염 정령",
  "archetype": "Magic",
  "stats": {
    "maxHealth": 80,
    "moveSpeed": 2.5,
    "attackDamage": 30
  },
  "abilities": [
    {
      "id": "fire_burst",
      "name": "화염 폭발",
      "cooldown": 10,
      "damage": 40,
      "aoe": 3,
      "effect": "화상 5초 (초당 5 피해)"
    }
  ],
  "immunities": ["fire"],
  "weaknesses": ["ice"]
},
{
  "id": "lava_golem",
  "displayName": "용암 골렘",
  "archetype": "Tank",
  "stats": {
    "maxHealth": 200,
    "moveSpeed": 1.0,
    "attackDamage": 35,
    "armor": 10
  },
  "abilities": [
    {
      "id": "molten_armor",
      "name": "용암 갑옷",
      "passive": true,
      "effect": "근접 공격자에게 10 화염 피해 반사"
    }
  ]
}
```

#### 4층 - 맹독의 정원
```json
{
  "id": "poison_spider",
  "displayName": "맹독 거미",
  "archetype": "Melee",
  "stats": {
    "maxHealth": 70,
    "moveSpeed": 3.0,
    "attackDamage": 15
  },
  "abilities": [
    {
      "id": "venom_bite",
      "name": "맹독 이빨",
      "damage": 15,
      "effect": "중독 10초 (초당 3 피해)"
    },
    {
      "id": "web_trap",
      "name": "거미줄",
      "cooldown": 12,
      "effect": "플레이어 이동속도 -50% (5초)"
    }
  ]
},
{
  "id": "toxic_plant",
  "displayName": "맹독 식물",
  "archetype": "Stationary",
  "stats": {
    "maxHealth": 50,
    "moveSpeed": 0,
    "attackDamage": 0
  },
  "abilities": [
    {
      "id": "poison_gas",
      "name": "독가스 방출",
      "cooldown": 5,
      "aoe": 5,
      "damage": 20,
      "effect": "독 구름 생성 (10초 지속)"
    }
  ]
}
```

#### 5층 - 영원의 전당 (시간 테마)
```json
{
  "id": "time_phantom",
  "displayName": "시간의 환영",
  "archetype": "Magic",
  "stats": {
    "maxHealth": 100,
    "moveSpeed": 2.0,
    "attackDamage": 30
  },
  "abilities": [
    {
      "id": "temporal_echo",
      "name": "시간 잔향",
      "cooldown": 15,
      "effect": "3초 전 위치에 환영 생성 (체력 50%, 10초 지속)"
    },
    {
      "id": "time_slow",
      "name": "시간 감속",
      "cooldown": 20,
      "effect": "플레이어 이동속도 -70% (3초)"
    }
  ]
},
{
  "id": "eternity_guardian",
  "displayName": "영원의 수호병",
  "archetype": "Tank",
  "stats": {
    "maxHealth": 250,
    "moveSpeed": 1.5,
    "attackDamage": 40,
    "armor": 15
  },
  "abilities": [
    {
      "id": "immortal_shield",
      "name": "불멸의 방패",
      "passive": true,
      "effect": "체력 0 도달 시 1회 부활 (체력 50%)"
    }
  ]
}
```

### 2.3 몬스터 JSON 파일 구조

```
StreamingAssets/GameData/monsters/
├── monsters_common.json (1층, 10종)
├── monsters_branch_bone.json (2층, 15종)
├── monsters_branch_fire.json (3층, 15종)
├── monsters_branch_poison.json (4층, 15종)
├── monsters_final.json (5층, 10종)
└── monsters_bosses.json (보스 5종)
```

---

## 3. 아이템 콘텐츠 확장 (300+)

### 3.1 아이템 분포

**총 300+ 아이템**:

| 카테고리 | 수량 | 설명 |
|---------|------|------|
| **무기** | 100 | 검, 도끼, 활, 지팡이 등 (Tier 1-9) |
| **방어구** | 50 | 갑옷, 투구, 장갑, 신발 (Tier 1-9) |
| **액세서리** | 50 | 반지, 목걸이 (특수 효과) |
| **소비 아이템** | 100 | 포션, 스크롤, 투척 무기 |
| **아티팩트** | 50 | 고유 전설 아이템 |

### 3.2 무기 Tier 시스템

**Tier 1-9** 무기, 각 Tier마다 스탯 증가:

```json
{
  "weaponTiers": [
    { "tier": 1, "baseDamage": 10, "displayName": "낡은" },
    { "tier": 2, "baseDamage": 15, "displayName": "평범한" },
    { "tier": 3, "baseDamage": 20, "displayName": "튼튼한" },
    { "tier": 4, "baseDamage": 30, "displayName": "우수한" },
    { "tier": 5, "baseDamage": 40, "displayName": "훌륭한" },
    { "tier": 6, "baseDamage": 55, "displayName": "희귀한" },
    { "tier": 7, "baseDamage": 70, "displayName": "영웅의" },
    { "tier": 8, "baseDamage": 90, "displayName": "전설의" },
    { "tier": 9, "baseDamage": 120, "displayName": "신화의" }
  ],
  "weaponTypes": [
    {
      "type": "sword",
      "displayName": "검",
      "attackSpeed": 1.0,
      "range": 1.5
    },
    {
      "type": "axe",
      "displayName": "도끼",
      "attackSpeed": 0.8,
      "range": 1.5,
      "bonusDamage": 1.2
    },
    {
      "type": "bow",
      "displayName": "활",
      "attackSpeed": 0.7,
      "range": 8.0,
      "projectile": true
    },
    {
      "type": "staff",
      "displayName": "지팡이",
      "attackSpeed": 1.2,
      "range": 6.0,
      "magicDamage": true
    }
  ]
}
```

**무기 조합 예시**:
- Tier 1 + Sword = "낡은 검" (피해 10)
- Tier 5 + Axe = "훌륭한 도끼" (피해 40 * 1.2 = 48)
- Tier 9 + Staff = "신화의 지팡이" (마법 피해 120)

**총 무기 수**: 9 Tier x 4 Type = 36종 (기본) + 인챈트/특수 속성 추가 시 100+

### 3.3 인챈트 시스템

모든 장비는 **+0 ~ +9 인챈트** 가능:

```json
{
  "enchantmentLevels": [
    { "level": 0, "statBonus": 0, "displayName": "" },
    { "level": 1, "statBonus": 0.05, "displayName": "+1" },
    { "level": 2, "statBonus": 0.10, "displayName": "+2" },
    { "level": 3, "statBonus": 0.15, "displayName": "+3" },
    { "level": 4, "statBonus": 0.20, "displayName": "+4" },
    { "level": 5, "statBonus": 0.30, "displayName": "+5" },
    { "level": 6, "statBonus": 0.40, "displayName": "+6" },
    { "level": 7, "statBonus": 0.50, "displayName": "+7" },
    { "level": 8, "statBonus": 0.70, "displayName": "+8" },
    { "level": 9, "statBonus": 1.00, "displayName": "+9" }
  ]
}
```

**예시**:
- "훌륭한 검 +5" = 기본 피해 40 * (1 + 0.30) = 52
- "신화의 지팡이 +9" = 기본 피해 120 * (1 + 1.00) = 240

### 3.4 아티팩트 (고유 전설 아이템)

**50개 아티팩트**, 각 아티팩트는 **고유 효과** 보유:

```json
{
  "artifacts": [
    {
      "id": "excalibur",
      "displayName": "엑스칼리버",
      "type": "weapon_sword",
      "tier": 9,
      "baseDamage": 150,
      "uniqueEffect": {
        "name": "성스러운 빛",
        "description": "공격 시 10% 확률로 적을 즉사 (보스 제외)"
      },
      "lore": "전설의 왕이 휘둘렀던 성검",
      "dropLocation": "final_boss",
      "dropChance": 0.05
    },
    {
      "id": "ring_of_time",
      "displayName": "시간의 반지",
      "type": "accessory_ring",
      "uniqueEffect": {
        "name": "시간 되돌리기",
        "description": "사망 시 1회 5초 전으로 되돌림 (런 당 1회)"
      },
      "lore": "시간의 수호자가 착용했던 반지",
      "dropLocation": "time_keeper_boss",
      "dropChance": 0.1
    },
    {
      "id": "cloak_of_shadows",
      "displayName": "그림자 망토",
      "type": "armor_cloak",
      "tier": 7,
      "defense": 30,
      "uniqueEffect": {
        "name": "완전 은신",
        "description": "스페이스바로 5초간 완전 투명 (쿨다운 60초)"
      },
      "dropLocation": "vault_poison_branch"
    }
  ]
}
```

### 3.5 소비 아이템 확장

**100개 소비 아이템**:

| 카테고리 | 수량 | 예시 |
|---------|------|------|
| **포션** | 40 | 체력 회복 (소/중/대), 마나 회복, 해독제, 투명화 |
| **스크롤** | 30 | 순간이동, 식별, 마법 강화, 저주 해제 |
| **투척 무기** | 20 | 화염병, 독 병, 빙결 병, 섬광탄 |
| **음식** | 10 | 빵, 고기, 버섯 (버프 효과) |

```json
{
  "consumables": [
    {
      "id": "potion_health_large",
      "displayName": "대형 체력 포션",
      "type": "potion",
      "effect": {
        "type": "heal",
        "value": 100
      },
      "stackSize": 5
    },
    {
      "id": "scroll_teleport",
      "displayName": "순간이동 스크롤",
      "type": "scroll",
      "effect": {
        "type": "teleport",
        "range": "random_room"
      },
      "stackSize": 3
    },
    {
      "id": "firebomb",
      "displayName": "화염병",
      "type": "throwing",
      "effect": {
        "type": "aoe_damage",
        "damage": 80,
        "radius": 3,
        "element": "fire"
      },
      "stackSize": 10
    }
  ]
}
```

---

## 4. 밸런스 튜닝 시스템

### 4.1 밸런스 파라미터 JSON

모든 밸런스 수치를 **JSON으로 외부화**하여 런타임 조정 가능:

```json
{
  "version": "1.0",
  "balanceConfig": {
    "player": {
      "baseHealthRegen": 1.0,
      "baseManaRegen": 2.0,
      "dodgeChanceBase": 0.05,
      "criticalChanceBase": 0.1
    },
    "enemies": {
      "healthScaling": {
        "floor1": 1.0,
        "floor2": 1.5,
        "floor3": 1.8,
        "floor4": 2.0,
        "floor5": 2.5
      },
      "damageScaling": {
        "floor1": 1.0,
        "floor2": 1.3,
        "floor3": 1.6,
        "floor4": 1.9,
        "floor5": 2.3
      }
    },
    "skills": {
      "xpCurve": [
        { "level": 1, "xpRequired": 100 },
        { "level": 2, "xpRequired": 150 },
        { "level": 5, "xpRequired": 400 },
        { "level": 10, "xpRequired": 1200 },
        { "level": 15, "xpRequired": 3000 },
        { "level": 20, "xpRequired": 6000 },
        { "level": 27, "xpRequired": 15000 }
      ],
      "xpGainPerAction": {
        "melee_attack": 5,
        "spell_cast": 8,
        "dodge_success": 3,
        "enemy_kill": 50
      }
    },
    "loot": {
      "goldDropMultiplier": 1.0,
      "itemDropChance": {
        "common": 0.6,
        "uncommon": 0.25,
        "rare": 0.1,
        "legendary": 0.03,
        "artifact": 0.02
      }
    },
    "difficulty": {
      "easyMode": {
        "playerDamageMultiplier": 1.2,
        "enemyDamageMultiplier": 0.8,
        "enemyHealthMultiplier": 0.7
      },
      "normalMode": {
        "playerDamageMultiplier": 1.0,
        "enemyDamageMultiplier": 1.0,
        "enemyHealthMultiplier": 1.0
      },
      "hardMode": {
        "playerDamageMultiplier": 0.9,
        "enemyDamageMultiplier": 1.3,
        "enemyHealthMultiplier": 1.5
      }
    }
  }
}
```

### 4.2 C# 구현 (BalanceConfigManager.cs)

```csharp
namespace Game.Systems.Balance
{
    using UnityEngine;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.IO;

    /// <summary>
    /// 밸런스 파라미터 관리 (JSON 기반 런타임 조정)
    /// </summary>
    public class BalanceConfigManager : MonoBehaviour
    {
        public static BalanceConfigManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private BalanceConfig config;

        [Header("Difficulty")]
        [SerializeField] private DifficultyMode currentDifficulty = DifficultyMode.Normal;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            LoadBalanceConfig();
        }

        private void LoadBalanceConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "GameData", "balance.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<BalanceConfigData>(json);
                config = data.balanceConfig;
                Debug.Log("[BalanceConfig] Loaded from JSON");
            }
            else
            {
                Debug.LogWarning("[BalanceConfig] File not found, using defaults");
            }
        }

        /// <summary>
        /// 플레이어 스탯에 난이도 배율 적용
        /// </summary>
        public float GetPlayerDamageMultiplier()
        {
            return currentDifficulty switch
            {
                DifficultyMode.Easy => config.difficulty.easyMode.playerDamageMultiplier,
                DifficultyMode.Normal => config.difficulty.normalMode.playerDamageMultiplier,
                DifficultyMode.Hard => config.difficulty.hardMode.playerDamageMultiplier,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 층별 적 체력 스케일링
        /// </summary>
        public float GetEnemyHealthScaling(int floor)
        {
            float baseScaling = floor switch
            {
                1 => config.enemies.healthScaling.floor1,
                2 => config.enemies.healthScaling.floor2,
                3 => config.enemies.healthScaling.floor3,
                4 => config.enemies.healthScaling.floor4,
                5 => config.enemies.healthScaling.floor5,
                _ => 1.0f
            };

            // 난이도 배율 추가
            float difficultyMultiplier = currentDifficulty switch
            {
                DifficultyMode.Easy => config.difficulty.easyMode.enemyHealthMultiplier,
                DifficultyMode.Normal => config.difficulty.normalMode.enemyHealthMultiplier,
                DifficultyMode.Hard => config.difficulty.hardMode.enemyHealthMultiplier,
                _ => 1.0f
            };

            return baseScaling * difficultyMultiplier;
        }

        /// <summary>
        /// 스킬 레벨업 필요 XP
        /// </summary>
        public int GetSkillXPRequired(int level)
        {
            var xpData = config.skills.xpCurve.Find(x => x.level == level);
            return xpData != null ? xpData.xpRequired : 10000;
        }

        /// <summary>
        /// 아이템 드롭 확률 (레어도별)
        /// </summary>
        public float GetItemDropChance(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => config.loot.itemDropChance.common,
                ItemRarity.Uncommon => config.loot.itemDropChance.uncommon,
                ItemRarity.Rare => config.loot.itemDropChance.rare,
                ItemRarity.Legendary => config.loot.itemDropChance.legendary,
                ItemRarity.Artifact => config.loot.itemDropChance.artifact,
                _ => 0.1f
            };
        }

        public void SetDifficulty(DifficultyMode mode)
        {
            currentDifficulty = mode;
            Debug.Log($"[BalanceConfig] Difficulty set to {mode}");
        }

        public BalanceConfig GetConfig() => config;
    }

    // Data structures
    [System.Serializable]
    public class BalanceConfigData
    {
        public string version;
        public BalanceConfig balanceConfig;
    }

    [System.Serializable]
    public class BalanceConfig
    {
        public PlayerBalance player;
        public EnemyBalance enemies;
        public SkillBalance skills;
        public LootBalance loot;
        public DifficultySettings difficulty;
    }

    [System.Serializable]
    public class PlayerBalance
    {
        public float baseHealthRegen;
        public float baseManaRegen;
    }

    [System.Serializable]
    public class EnemyBalance
    {
        public HealthScaling healthScaling;
        public DamageScaling damageScaling;
    }

    [System.Serializable]
    public class HealthScaling
    {
        public float floor1, floor2, floor3, floor4, floor5;
    }

    [System.Serializable]
    public class DamageScaling
    {
        public float floor1, floor2, floor3, floor4, floor5;
    }

    [System.Serializable]
    public class SkillBalance
    {
        public List<XPCurve> xpCurve;
    }

    [System.Serializable]
    public class XPCurve
    {
        public int level;
        public int xpRequired;
    }

    [System.Serializable]
    public class LootBalance
    {
        public float goldDropMultiplier;
        public ItemDropChances itemDropChance;
    }

    [System.Serializable]
    public class ItemDropChances
    {
        public float common, uncommon, rare, legendary, artifact;
    }

    [System.Serializable]
    public class DifficultySettings
    {
        public DifficultyModifiers easyMode;
        public DifficultyModifiers normalMode;
        public DifficultyModifiers hardMode;
    }

    [System.Serializable]
    public class DifficultyModifiers
    {
        public float playerDamageMultiplier;
        public float enemyDamageMultiplier;
        public float enemyHealthMultiplier;
    }

    public enum DifficultyMode { Easy, Normal, Hard }
    public enum ItemRarity { Common, Uncommon, Rare, Legendary, Artifact }
}
```

### 4.3 플레이테스트 데이터 수집

밸런스 조정을 위한 **자동 데이터 수집**:

```csharp
namespace Game.Systems.Balance
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// 플레이테스트 데이터 수집 및 분석
    /// </summary>
    public class PlaytestAnalyzer : MonoBehaviour
    {
        [Header("Collected Data")]
        [SerializeField] private List<RunAnalysis> runData = new List<RunAnalysis>();

        public void RecordRun(RunAnalysis analysis)
        {
            runData.Add(analysis);
            SaveToCSV();
        }

        private void SaveToCSV()
        {
            // CSV 파일로 저장 (Excel 분석용)
            // 예: species, job, winRate, avgPlayTime, avgKills
        }

        public void AnalyzeBalance()
        {
            // 종족별 승률
            var speciesWinRate = CalculateSpeciesWinRate();
            // 직업별 승률
            var jobWinRate = CalculateJobWinRate();
            // 스킬별 사용 빈도
            var skillUsage = CalculateSkillUsage();

            Debug.Log($"[PlaytestAnalyzer] Species win rates: {string.Join(", ", speciesWinRate)}");
        }

        private Dictionary<string, float> CalculateSpeciesWinRate()
        {
            var winRates = new Dictionary<string, float>();
            // 구현
            return winRates;
        }
    }

    [System.Serializable]
    public class RunAnalysis
    {
        public string species;
        public string job;
        public bool victory;
        public float playTime;
        public int totalKills;
        public int floorReached;
    }
}
```

---

## 5. 온라인 리더보드

### 5.1 리더보드 카테고리

| 카테고리 | 설명 | 정렬 기준 |
|---------|------|---------|
| **스피드런** | 최단 클리어 시간 | 플레이 타임 (오름차순) |
| **최고 킬 수** | 가장 많은 적 처치 | 킬 수 (내림차순) |
| **최소 층수 클리어** | 최소 층수로 클리어 (챌린지) | 층 수 (오름차순) |
| **일일 챌린지** | 동일 시드 하루 챌린지 | 점수 (내림차순) |

### 5.2 백엔드 서비스 (간단한 REST API)

**옵션 1**: Firebase Realtime Database (무료 tier)
**옵션 2**: Custom REST API (Node.js + MongoDB)
**옵션 3**: Steam Leaderboards (Steam SDK 사용)

### 5.3 C# 구현 (LeaderboardManager.cs)

```csharp
namespace Game.Systems.Online
{
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// 온라인 리더보드 시스템 (REST API 기반)
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        [Header("API Settings")]
        [SerializeField] private string apiBaseURL = "https://labyrinth-api.com";

        [Header("Current Leaderboard")]
        [SerializeField] private List<LeaderboardEntry> currentLeaderboard = new List<LeaderboardEntry>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// 리더보드 제출
        /// </summary>
        public void SubmitScore(LeaderboardCategory category, int score, string playerName)
        {
            StartCoroutine(SubmitScoreCoroutine(category, score, playerName));
        }

        private IEnumerator SubmitScoreCoroutine(LeaderboardCategory category, int score, string playerName)
        {
            string url = $"{apiBaseURL}/leaderboard/submit";

            var data = new SubmitScoreRequest
            {
                category = category.ToString(),
                playerName = playerName,
                score = score,
                timestamp = System.DateTime.UtcNow.ToString("o")
            };

            string json = JsonConvert.SerializeObject(data);

            using (UnityWebRequest request = UnityWebRequest.Post(url, json, "application/json"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[Leaderboard] Score submitted successfully");
                }
                else
                {
                    Debug.LogError($"[Leaderboard] Submit failed: {request.error}");
                }
            }
        }

        /// <summary>
        /// 리더보드 조회 (Top 100)
        /// </summary>
        public void FetchLeaderboard(LeaderboardCategory category)
        {
            StartCoroutine(FetchLeaderboardCoroutine(category));
        }

        private IEnumerator FetchLeaderboardCoroutine(LeaderboardCategory category)
        {
            string url = $"{apiBaseURL}/leaderboard?category={category}&limit=100";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    var response = JsonConvert.DeserializeObject<LeaderboardResponse>(json);
                    currentLeaderboard = response.entries;

                    Debug.Log($"[Leaderboard] Fetched {currentLeaderboard.Count} entries");
                    GameEvents.TriggerLeaderboardUpdated(currentLeaderboard);
                }
                else
                {
                    Debug.LogError($"[Leaderboard] Fetch failed: {request.error}");
                }
            }
        }

        public List<LeaderboardEntry> GetCurrentLeaderboard() => currentLeaderboard;
    }

    // Data structures
    public enum LeaderboardCategory
    {
        Speedrun,
        HighestKills,
        MinFloors,
        DailyChallenge
    }

    [System.Serializable]
    public class SubmitScoreRequest
    {
        public string category;
        public string playerName;
        public int score;
        public string timestamp;
    }

    [System.Serializable]
    public class LeaderboardResponse
    {
        public List<LeaderboardEntry> entries;
    }

    [System.Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string playerName;
        public int score;
        public string timestamp;
    }
}
```

### 5.4 UI 구현 (LeaderboardPanel.cs)

```csharp
namespace Game.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;
    using Game.Systems.Online;

    public class LeaderboardPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform entryContainer;
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private Dropdown categoryDropdown;

        private void Start()
        {
            categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            GameEvents.OnLeaderboardUpdated += DisplayLeaderboard;

            // 초기 로드
            LeaderboardManager.Instance.FetchLeaderboard(LeaderboardCategory.Speedrun);
        }

        private void OnCategoryChanged(int index)
        {
            LeaderboardCategory category = (LeaderboardCategory)index;
            LeaderboardManager.Instance.FetchLeaderboard(category);
        }

        private void DisplayLeaderboard(List<LeaderboardEntry> entries)
        {
            // 기존 항목 제거
            foreach (Transform child in entryContainer)
            {
                Destroy(child.gameObject);
            }

            // 새 항목 생성
            foreach (var entry in entries)
            {
                GameObject entryObj = Instantiate(entryPrefab, entryContainer);
                var textComponents = entryObj.GetComponentsInChildren<Text>();

                textComponents[0].text = entry.rank.ToString();
                textComponents[1].text = entry.playerName;
                textComponents[2].text = entry.score.ToString();
            }
        }
    }
}
```

---

## 6. 고급 통계 시스템

### 6.1 수집할 통계

- **종족별 승률** (메타 분석)
- **직업별 승률** (밸런스 참고)
- **스킬별 사용 빈도** (인기 빌드 분석)
- **아이템 픽률** (드롭 밸런스 참고)
- **층별 사망률** (난이도 곡선 분석)
- **평균 플레이 타임** (게임 길이 조정)

### 6.2 C# 구현 (OnlineStatsCollector.cs)

```csharp
namespace Game.Systems.Online
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.Networking;
    using Newtonsoft.Json;

    /// <summary>
    /// 익명 통계 수집 (개인정보 없음, 집계용)
    /// </summary>
    public class OnlineStatsCollector : MonoBehaviour
    {
        [Header("API Settings")]
        [SerializeField] private string statsAPIURL = "https://labyrinth-api.com/stats";

        [Header("Privacy")]
        [SerializeField] private bool enableStatsCollection = true;

        public void SubmitRunStats(RunStatsData stats)
        {
            if (!enableStatsCollection) return;

            StartCoroutine(SubmitStatsCoroutine(stats));
        }

        private IEnumerator SubmitStatsCoroutine(RunStatsData stats)
        {
            string json = JsonConvert.SerializeObject(stats);

            using (UnityWebRequest request = UnityWebRequest.Post(statsAPIURL, json, "application/json"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[Stats] Anonymous stats submitted");
                }
                else
                {
                    Debug.LogWarning($"[Stats] Submit failed: {request.error}");
                }
            }
        }
    }

    [System.Serializable]
    public class RunStatsData
    {
        public string species;
        public string job;
        public bool victory;
        public int floorReached;
        public float playTime;
        public int totalKills;
        public List<string> skillsUsed;
        public List<string> itemsEquipped;
    }
}
```

### 6.3 통계 시각화 (관리자용 대시보드)

**옵션**: Metabase, Grafana, 또는 Custom Web Dashboard

```sql
-- 종족별 승률 쿼리 (PostgreSQL 예시)
SELECT
    species,
    COUNT(*) as total_runs,
    SUM(CASE WHEN victory = true THEN 1 ELSE 0 END) as victories,
    ROUND(100.0 * SUM(CASE WHEN victory = true THEN 1 ELSE 0 END) / COUNT(*), 2) as win_rate
FROM run_stats
GROUP BY species
ORDER BY win_rate DESC;
```

---

## 7. Steam SDK 통합

### 7.1 Steamworks.NET 설치

**패키지**: [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET)

```bash
# Unity Package Manager로 설치
Add package from git URL: https://github.com/rlabrecque/Steamworks.NET.git#upm
```

### 7.2 C# 구현 (SteamManager.cs)

```csharp
namespace Game.Systems.Steam
{
    using UnityEngine;
    using Steamworks;

    /// <summary>
    /// Steam SDK 초기화 및 관리
    /// </summary>
    public class SteamManager : MonoBehaviour
    {
        public static SteamManager Instance { get; private set; }

        [Header("Steam Settings")]
        [SerializeField] private uint appId = 480; // 실제 App ID로 교체

        private bool steamInitialized = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeSteam();
        }

        private void InitializeSteam()
        {
            try
            {
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(appId)))
                {
                    Application.Quit();
                    return;
                }

                steamInitialized = SteamAPI.Init();

                if (steamInitialized)
                {
                    string playerName = SteamFriends.GetPersonaName();
                    Debug.Log($"[Steam] Initialized. Welcome, {playerName}!");
                }
                else
                {
                    Debug.LogError("[Steam] Initialization failed");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Steam] Exception: {e.Message}");
            }
        }

        private void Update()
        {
            if (steamInitialized)
            {
                SteamAPI.RunCallbacks();
            }
        }

        private void OnApplicationQuit()
        {
            if (steamInitialized)
            {
                SteamAPI.Shutdown();
            }
        }

        public bool IsSteamInitialized() => steamInitialized;
        public string GetPlayerName() => steamInitialized ? SteamFriends.GetPersonaName() : "Player";
    }
}
```

### 7.3 Steam 업적 연동 (SteamAchievements.cs)

```csharp
namespace Game.Systems.Steam
{
    using UnityEngine;
    using Steamworks;
    using Game.Systems.Achievement;

    /// <summary>
    /// Steam 업적 연동
    /// </summary>
    public class SteamAchievements : MonoBehaviour
    {
        private void Start()
        {
            GameEvents.OnAchievementUnlocked += UnlockSteamAchievement;
        }

        private void UnlockSteamAchievement(string achievementId)
        {
            if (!SteamManager.Instance.IsSteamInitialized()) return;

            // Labyrinth 업적 ID → Steam 업적 ID 매핑
            string steamAchievementId = MapToSteamAchievement(achievementId);

            if (string.IsNullOrEmpty(steamAchievementId)) return;

            bool success = SteamUserStats.SetAchievement(steamAchievementId);
            if (success)
            {
                SteamUserStats.StoreStats();
                Debug.Log($"[Steam] Achievement unlocked: {steamAchievementId}");
            }
        }

        private string MapToSteamAchievement(string labyrinthId)
        {
            // 매핑 테이블
            return labyrinthId switch
            {
                "achievement_first_victory" => "ACH_FIRST_WIN",
                "achievement_true_freedom" => "ACH_ENDING_A",
                "achievement_godless_victory" => "ACH_NO_GOD",
                _ => ""
            };
        }
    }
}
```

### 7.4 Steam 클라우드 세이브 (CloudSaveManager.cs)

```csharp
namespace Game.Systems.Steam
{
    using UnityEngine;
    using Steamworks;
    using System.Text;

    /// <summary>
    /// Steam Cloud 세이브 시스템
    /// </summary>
    public class CloudSaveManager : MonoBehaviour
    {
        private const string SAVE_FILE_NAME = "save_data.json";

        public void SaveToCloud(string jsonData)
        {
            if (!SteamManager.Instance.IsSteamInitialized()) return;

            byte[] data = Encoding.UTF8.GetBytes(jsonData);

            bool success = SteamRemoteStorage.FileWrite(SAVE_FILE_NAME, data, data.Length);

            if (success)
            {
                Debug.Log("[CloudSave] Saved to Steam Cloud");
            }
            else
            {
                Debug.LogError("[CloudSave] Failed to save");
            }
        }

        public string LoadFromCloud()
        {
            if (!SteamManager.Instance.IsSteamInitialized()) return "";

            if (!SteamRemoteStorage.FileExists(SAVE_FILE_NAME))
            {
                Debug.LogWarning("[CloudSave] No cloud save found");
                return "";
            }

            int fileSize = SteamRemoteStorage.GetFileSize(SAVE_FILE_NAME);
            byte[] data = new byte[fileSize];

            int bytesRead = SteamRemoteStorage.FileRead(SAVE_FILE_NAME, data, fileSize);

            if (bytesRead > 0)
            {
                string jsonData = Encoding.UTF8.GetString(data);
                Debug.Log("[CloudSave] Loaded from Steam Cloud");
                return jsonData;
            }

            return "";
        }
    }
}
```

---

## 8. 현지화 시스템 (한글/영어)

### 8.1 번역 파일 구조 (CSV)

```csv
# ko.csv (한글)
KEY,VALUE
ui_title_main_menu,메인 메뉴
ui_button_start,게임 시작
ui_button_quit,종료
species_human,인간
species_dwarf,드워프
job_fighter,전사
item_sword_tier1,낡은 검
monster_goblin,고블린

# en.csv (영어)
KEY,VALUE
ui_title_main_menu,Main Menu
ui_button_start,Start Game
ui_button_quit,Quit
species_human,Human
species_dwarf,Dwarf
job_fighter,Fighter
item_sword_tier1,Rusty Sword
monster_goblin,Goblin
```

### 8.2 C# 구현 (LocalizationManager.cs)

```csharp
namespace Game.Systems.Localization
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// 현지화 시스템 (CSV 기반)
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private SystemLanguage currentLanguage = SystemLanguage.Korean;

        private Dictionary<string, string> localizedText = new Dictionary<string, string>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            LoadLocalization(currentLanguage);
        }

        /// <summary>
        /// 언어 파일 로드
        /// </summary>
        public void LoadLocalization(SystemLanguage language)
        {
            currentLanguage = language;
            localizedText.Clear();

            string fileName = language == SystemLanguage.English ? "en.csv" : "ko.csv";
            string path = Path.Combine(Application.streamingAssetsPath, "GameData", "localization", fileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"[Localization] File not found: {path}");
                return;
            }

            string[] lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string line = lines[i];
                string[] parts = line.Split(',');

                if (parts.Length >= 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    localizedText[key] = value;
                }
            }

            Debug.Log($"[Localization] Loaded {localizedText.Count} entries for {language}");
            GameEvents.TriggerLanguageChanged(language);
        }

        /// <summary>
        /// 번역 텍스트 가져오기
        /// </summary>
        public string GetText(string key)
        {
            if (localizedText.TryGetValue(key, out string value))
            {
                return value;
            }

            Debug.LogWarning($"[Localization] Missing key: {key}");
            return $"[{key}]";
        }

        public void SetLanguage(SystemLanguage language)
        {
            LoadLocalization(language);
        }

        public SystemLanguage GetCurrentLanguage() => currentLanguage;
    }
}
```

### 8.3 UI 텍스트 현지화 컴포넌트 (TextLocalizer.cs)

```csharp
namespace Game.Systems.Localization
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// UI Text를 자동으로 현지화
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextLocalizer : MonoBehaviour
    {
        [Header("Localization Key")]
        [SerializeField] private string localizationKey;

        private Text textComponent;

        private void Start()
        {
            textComponent = GetComponent<Text>();
            GameEvents.OnLanguageChanged += OnLanguageChanged;

            UpdateText();
        }

        private void OnDestroy()
        {
            GameEvents.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(SystemLanguage language)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey)) return;

            string localizedText = LocalizationManager.Instance.GetText(localizationKey);
            textComponent.text = localizedText;
        }

        // Inspector에서 키 변경 시 즉시 반영
        private void OnValidate()
        {
            if (Application.isPlaying && LocalizationManager.Instance != null)
            {
                UpdateText();
            }
        }
    }
}
```

### 8.4 언어 선택 UI (LanguageSelector.cs)

```csharp
namespace Game.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using Game.Systems.Localization;

    public class LanguageSelector : MonoBehaviour
    {
        [SerializeField] private Dropdown languageDropdown;

        private void Start()
        {
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            // 현재 언어로 초기화
            int currentIndex = LocalizationManager.Instance.GetCurrentLanguage() == SystemLanguage.English ? 1 : 0;
            languageDropdown.value = currentIndex;
        }

        private void OnLanguageChanged(int index)
        {
            SystemLanguage language = index == 0 ? SystemLanguage.Korean : SystemLanguage.English;
            LocalizationManager.Instance.SetLanguage(language);
        }
    }
}
```

---

## 9. Definition of Done

Phase 6가 완료되었다고 판단하는 기준:

- [ ] **종족 10개 완성**
  - 2종족 추가 (뱀파이어, 고블린)
  - 모든 종족 밸런스 조정 (승률 45-55%)
  - 종족별 특성 작동 확인

- [ ] **직업 10개 밸런스 완료**
  - 모든 직업 승률 40-60%
  - 초보자 친화 직업 (전사, 사냥꾼) 승률 55%+
  - 고난이도 직업 (암살자, 버서커) 승률 45%

- [ ] **몬스터 50+ 종류 추가**
  - 1층: 10종
  - 2-4층: 각 15종
  - 5층: 10종
  - 보스: 5종
  - 모든 몬스터 JSON 작성 완료
  - 몬스터 스탯 밸런스 조정 (층별 난이도 곡선)

- [ ] **아이템 300+ 개 추가**
  - 무기 100개 (9 Tier x 4 Type + 인챈트)
  - 방어구 50개
  - 액세서리 50개
  - 소비 아이템 100개
  - 아티팩트 50개
  - 모든 아이템 JSON 작성 완료

- [ ] **밸런스 튜닝 시스템 구현**
  - balance.json 파일 작성 (플레이어/적/스킬/루트 파라미터)
  - BalanceConfigManager.cs 구현
  - 런타임 밸런스 조정 가능
  - 난이도 모드 (Easy/Normal/Hard) 작동

- [ ] **플레이테스트 데이터 수집**
  - PlaytestAnalyzer.cs 구현
  - 종족/직업별 승률 수집
  - CSV 파일 출력 (Excel 분석용)

- [ ] **온라인 리더보드 구현**
  - 4개 카테고리 (스피드런, 최고 킬, 최소 층, 일일 챌린지)
  - REST API 통신 (제출/조회)
  - LeaderboardPanel UI 작동
  - Top 100 표시

- [ ] **고급 통계 시스템 구현**
  - 익명 통계 수집 (OnlineStatsCollector.cs)
  - 종족/직업/스킬/아이템 사용 통계
  - 관리자 대시보드 (선택 사항)

- [ ] **Steam SDK 통합 완료**
  - Steamworks.NET 설치
  - SteamManager.cs 초기화
  - Steam 업적 연동 (30+ 업적)
  - Steam 클라우드 세이브 작동
  - Steam 리더보드 (선택 사항)

- [ ] **현지화 시스템 완료**
  - LocalizationManager.cs 구현
  - ko.csv, en.csv 파일 작성 (모든 UI 텍스트)
  - TextLocalizer 컴포넌트 모든 UI에 적용
  - 언어 전환 UI 작동
  - 게임 내 모든 텍스트 현지화 완료

- [ ] **최종 빌드 테스트**
  - Windows 빌드 정상 작동
  - 모든 시스템 통합 테스트 통과
  - 크래시 없이 1시간 플레이 가능
  - 메모리 누수 없음 (프로파일러 확인)

---

## 10. 성능 및 출시 체크리스트

### 10.1 성능 최적화

- [ ] **프레임레이트 안정성**
  - 목표: 60 FPS 유지 (1080p, GTX 1060 기준)
  - 최악의 경우 (5층, 적 20마리): 45 FPS 이상

- [ ] **메모리 사용량**
  - 목표: 2GB 이하 (Unity 포함)
  - GC.Alloc 최소화 (Object Pooling)

- [ ] **로딩 시간**
  - 게임 시작 → 메인 메뉴: 5초 이내
  - 층 생성: 1초 이내 (61x61 그리드 포함)

- [ ] **네트워크 최적화**
  - 리더보드 조회: 3초 이내
  - 통계 제출: 백그라운드 (비차단)

### 10.2 버그 체크리스트

- [ ] **치명적 버그 0개**
  - 크래시 유발 버그
  - 진행 불가 버그 (갇히기, 문 안열림 등)
  - 세이브 파일 손상

- [ ] **주요 버그 수정**
  - 밸런스 붕괴 버그 (무한 골드, 무적 등)
  - UI 깨짐
  - 사운드 누락

- [ ] **알려진 이슈 문서화**
  - KNOWN_ISSUES.md 작성
  - 해결 예정 이슈 목록

### 10.3 Steam 출시 체크리스트

- [ ] **Steam 앱 설정**
  - App ID 생성
  - Store 페이지 작성 (설명, 스크린샷, 트레일러)
  - 가격 설정
  - 지역별 출시 설정

- [ ] **빌드 업로드**
  - SteamPipe로 빌드 업로드
  - Depot 설정 (Windows/Mac/Linux)
  - 베타 브랜치 설정 (Early Access 시)

- [ ] **Steam 기능 테스트**
  - 업적 해제 작동
  - 클라우드 세이브 작동
  - 리더보드 작동 (Steam 리더보드 사용 시)
  - 스크린샷 시스템 작동

- [ ] **법적 준비**
  - 개인정보 처리방침 (Privacy Policy)
  - 이용약관 (Terms of Service)
  - EULA (End User License Agreement)

- [ ] **마케팅 준비**
  - 트레일러 제작
  - 스크린샷 10장 이상
  - 커뮤니티 허브 설정
  - Discord 서버 개설 (선택)

### 10.4 출시 전 최종 점검

- [ ] **플레이테스트 10회 이상**
  - 내부 테스터 5명 이상
  - 평균 플레이 타임 2시간 이상
  - 주요 버그 리포트 수집

- [ ] **밸런스 최종 조정**
  - 종족/직업 승률 편차 10% 이내
  - 스킬 사용 빈도 분석
  - 아이템 드롭률 조정

- [ ] **콘텐츠 검증**
  - 모든 엔딩 달성 가능 확인
  - 모든 업적 해제 가능 확인
  - 모든 아이템 획득 가능 확인

- [ ] **문서화**
  - README.md 최신화
  - CHANGELOG.md 작성
  - 플레이어 가이드 (Wiki 또는 PDF)

---

## 마무리

Phase 6는 **게임의 완성 및 출시 준비** 단계입니다. 모든 콘텐츠를 확장하고, 밸런스를 조정하며, Steam 출시를 위한 인프라를 구축합니다.

**DoD 10개 항목**을 모두 체크하고, **성능/출시 체크리스트**를 통과하면 **Labyrinth는 출시 준비 완료**입니다.

Phase 6 완료 후에는:
- **Early Access 출시** (Steam)
- **커뮤니티 피드백 수집**
- **패치 및 콘텐츠 업데이트**
- **정식 출시 (v1.0)**

---

**End of Phase 6 Request Document**
**End of Phase 4-5-6 Planning**
