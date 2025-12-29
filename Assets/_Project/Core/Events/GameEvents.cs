namespace Game.Core.Events
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 시스템 간 결합도를 낮추기 위한 이벤트 버스
    /// Core 모듈은 다른 모듈에 의존하지 않으므로, 모든 타입을 object로 처리
    /// 호출/구독 측에서 캐스팅 필요
    /// </summary>
    public static class GameEvents
    {
        // Dungeon Events (Phase 1) - object로 타입 제거
        public static event Action<object> OnEnterRoom; // Room
        public static event Action<object> OnRoomCleared; // Room
        public static event Action<object> OnDungeonGenerated; // DungeonResult

        // Maze Events (Phase 2)
        public static event Action<object> OnMazeGenerated; // MazeResult

        // Encounter Events - object로 타입 제거
        public static event Action<object> OnEncounterResolved; // EncounterResult
        public static event Action<object, object, object> OnEventEncounterTriggered; // EventTextEncounterSO, EncounterContext, EncounterResult
        public static event Action<Vector2Int, object> OnEnemySpawned; // Phase 2 - EnemyDefinition
        public static event Action<GameObject> OnEnemyDetectedPlayer; // Phase 2
        public static event Action<object> OnCorridorTriggerActivated; // Phase 2 - CorridorTrigger

        // Vision Events
        public static event Action<Vector2[]> OnVisionUpdated; // Phase 1 (Raycast)
        public static event Action<HashSet<Vector2Int>> OnVisionCellsUpdated; // Phase 2 (Shadowcasting)
        public static event Action<Vector2> OnPlayerFacingChanged;

        // Player Events
        public static event Action<int> OnPlayerHealthChanged;
        public static event Action<int> OnPlayerGoldChanged;
        public static event Action OnPlayerDeath;

        // Combat Events (Phase 3)
        public static event Action<GameObject, int, int> OnHealthChanged; // (entity, currentHealth, maxHealth)
        public static event Action<GameObject> OnDeath; // (entity)
        public static event Action<GameObject, GameObject, int> OnDamageDealt; // (attacker, target, damage)
        public static event Action<GameObject> OnEnemyDied; // (enemy)

        // Game Flow Events
        public static event Action OnGameStart;
        public static event Action OnGameOver;
        public static event Action OnLevelComplete;

        // Trigger methods (안전한 호출을 위한 래퍼)
        // object 타입 사용 - 호출 측에서 구체 타입을 object로 변환하여 전달

        // Phase 1 - Dungeon
        public static void TriggerEnterRoom(object room) => OnEnterRoom?.Invoke(room);
        public static void TriggerRoomCleared(object room) => OnRoomCleared?.Invoke(room);
        public static void TriggerDungeonGenerated(object result) => OnDungeonGenerated?.Invoke(result);

        // Phase 2 - Maze
        public static void TriggerMazeGenerated(object result) => OnMazeGenerated?.Invoke(result);

        // Encounter
        public static void TriggerEncounterResolved(object result) => OnEncounterResolved?.Invoke(result);
        public static void TriggerEventEncounter(object eventData, object context, object result)
            => OnEventEncounterTriggered?.Invoke(eventData, context, result);
        public static void TriggerEnemySpawned(Vector2Int position, object definition)
            => OnEnemySpawned?.Invoke(position, definition);
        public static void TriggerCorridorTriggerActivated(object trigger)
            => OnCorridorTriggerActivated?.Invoke(trigger);

        // Vision
        public static void TriggerVisionUpdated(Vector2[] points) => OnVisionUpdated?.Invoke(points);
        public static void TriggerVisionCellsUpdated(HashSet<Vector2Int> cells) => OnVisionCellsUpdated?.Invoke(cells);
        public static void TriggerPlayerFacingChanged(Vector2 direction) => OnPlayerFacingChanged?.Invoke(direction);

        // Player
        public static void TriggerPlayerHealthChanged(int newHealth) => OnPlayerHealthChanged?.Invoke(newHealth);
        public static void TriggerPlayerGoldChanged(int newGold) => OnPlayerGoldChanged?.Invoke(newGold);
        public static void TriggerPlayerDeath() => OnPlayerDeath?.Invoke();

        // Combat (Phase 3)
        public static void TriggerHealthChanged(GameObject entity, int currentHealth, int maxHealth)
            => OnHealthChanged?.Invoke(entity, currentHealth, maxHealth);
        public static void TriggerDeath(GameObject entity) => OnDeath?.Invoke(entity);
        public static void TriggerDamageDealt(GameObject attacker, GameObject target, int damage)
            => OnDamageDealt?.Invoke(attacker, target, damage);
        public static void TriggerEnemyDied(GameObject enemy) => OnEnemyDied?.Invoke(enemy);

        // Game Flow
        public static void TriggerGameStart() => OnGameStart?.Invoke();
        public static void TriggerGameOver() => OnGameOver?.Invoke();
        public static void TriggerLevelComplete() => OnLevelComplete?.Invoke();

        /// <summary>
        /// 모든 이벤트 구독 해제 (씬 전환 시 사용)
        /// </summary>
        public static void ClearAllEvents()
        {
            // Phase 1
            OnEnterRoom = null;
            OnRoomCleared = null;
            OnDungeonGenerated = null;

            // Phase 2
            OnMazeGenerated = null;
            OnEnemySpawned = null;
            OnEnemyDetectedPlayer = null;
            OnCorridorTriggerActivated = null;
            OnVisionCellsUpdated = null;

            // Common
            OnEncounterResolved = null;
            OnEventEncounterTriggered = null;
            OnVisionUpdated = null;
            OnPlayerFacingChanged = null;
            OnPlayerHealthChanged = null;
            OnPlayerGoldChanged = null;
            OnPlayerDeath = null;

            // Combat (Phase 3)
            OnHealthChanged = null;
            OnDeath = null;
            OnDamageDealt = null;
            OnEnemyDied = null;

            // Game Flow
            OnGameStart = null;
            OnGameOver = null;
            OnLevelComplete = null;
        }
    }
}
