namespace Game.Core.Events
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Game.Systems.Dungeon;
    using Game.Systems.Encounter;
    using Game.Data.Encounter;

    /// <summary>
    /// 시스템 간 결합도를 낮추기 위한 이벤트 버스
    /// </summary>
    public static class GameEvents
    {
        // Dungeon Events (Phase 1)
        public static event Action<Room> OnEnterRoom;
        public static event Action<Room> OnRoomCleared;
        public static event Action<DungeonResult> OnDungeonGenerated;

        // Maze Events (Phase 2)
        public static event Action<Game.Systems.Maze.MazeResult> OnMazeGenerated;

        // Encounter Events
        public static event Action<EncounterResult> OnEncounterResolved;
        public static event Action<EventTextEncounterSO, EncounterContext, EncounterResult> OnEventEncounterTriggered;
        public static event Action<Vector2Int, Game.DataJson.Schema.EnemyDefinition> OnEnemySpawned; // Phase 2
        public static event Action<GameObject> OnEnemyDetectedPlayer; // Phase 2
        public static event Action<CorridorTrigger> OnCorridorTriggerActivated; // Phase 2

        // Vision Events
        public static event Action<Vector2[]> OnVisionUpdated; // Phase 1 (Raycast)
        public static event Action<HashSet<Vector2Int>> OnVisionCellsUpdated; // Phase 2 (Shadowcasting)
        public static event Action<Vector2> OnPlayerFacingChanged;

        // Player Events
        public static event Action<int> OnPlayerHealthChanged;
        public static event Action<int> OnPlayerGoldChanged;
        public static event Action OnPlayerDeath;

        // Game Flow Events
        public static event Action OnGameStart;
        public static event Action OnGameOver;
        public static event Action OnLevelComplete;

        // Trigger methods (안전한 호출을 위한 래퍼)
        // Phase 1
        public static void TriggerEnterRoom(Room room) => OnEnterRoom?.Invoke(room);
        public static void TriggerRoomCleared(Room room) => OnRoomCleared?.Invoke(room);
        public static void TriggerDungeonGenerated(DungeonResult result) => OnDungeonGenerated?.Invoke(result);

        // Phase 2 - Maze
        public static void TriggerMazeGenerated(Game.Systems.Maze.MazeResult result) => OnMazeGenerated?.Invoke(result);

        // Encounter
        public static void TriggerEncounterResolved(EncounterResult result) => OnEncounterResolved?.Invoke(result);
        public static void TriggerEventEncounter(EventTextEncounterSO eventData, EncounterContext context, EncounterResult result)
            => OnEventEncounterTriggered?.Invoke(eventData, context, result);
        public static void TriggerEnemySpawned(Vector2Int position, Game.DataJson.Schema.EnemyDefinition definition)
            => OnEnemySpawned?.Invoke(position, definition);
        public static void TriggerCorridorTriggerActivated(CorridorTrigger trigger)
            => OnCorridorTriggerActivated?.Invoke(trigger);

        // Vision
        public static void TriggerVisionUpdated(Vector2[] points) => OnVisionUpdated?.Invoke(points);
        public static void TriggerVisionCellsUpdated(HashSet<Vector2Int> cells) => OnVisionCellsUpdated?.Invoke(cells);
        public static void TriggerPlayerFacingChanged(Vector2 direction) => OnPlayerFacingChanged?.Invoke(direction);

        // Player
        public static void TriggerPlayerHealthChanged(int newHealth) => OnPlayerHealthChanged?.Invoke(newHealth);
        public static void TriggerPlayerGoldChanged(int newGold) => OnPlayerGoldChanged?.Invoke(newGold);
        public static void TriggerPlayerDeath() => OnPlayerDeath?.Invoke();

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
            OnGameStart = null;
            OnGameOver = null;
            OnLevelComplete = null;
        }
    }
}
