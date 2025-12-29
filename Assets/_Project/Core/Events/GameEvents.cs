namespace Game.Core.Events
{
    using System;
    using UnityEngine;
    using Game.Systems.Dungeon;
    using Game.Systems.Encounter;
    using Game.Data.Encounter;

    /// <summary>
    /// 시스템 간 결합도를 낮추기 위한 이벤트 버스
    /// </summary>
    public static class GameEvents
    {
        // Dungeon Events
        public static event Action<Room> OnEnterRoom;
        public static event Action<Room> OnRoomCleared;
        public static event Action<DungeonResult> OnDungeonGenerated;

        // Encounter Events
        public static event Action<EncounterResult> OnEncounterResolved;
        public static event Action<EventTextEncounterSO, EncounterContext, EncounterResult> OnEventEncounterTriggered;

        // Vision Events
        public static event Action<Vector2[]> OnVisionUpdated;
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
        public static void TriggerEnterRoom(Room room) => OnEnterRoom?.Invoke(room);
        public static void TriggerRoomCleared(Room room) => OnRoomCleared?.Invoke(room);
        public static void TriggerDungeonGenerated(DungeonResult result) => OnDungeonGenerated?.Invoke(result);
        public static void TriggerEncounterResolved(EncounterResult result) => OnEncounterResolved?.Invoke(result);
        public static void TriggerEventEncounter(EventTextEncounterSO eventData, EncounterContext context, EncounterResult result)
            => OnEventEncounterTriggered?.Invoke(eventData, context, result);
        public static void TriggerVisionUpdated(Vector2[] points) => OnVisionUpdated?.Invoke(points);
        public static void TriggerPlayerFacingChanged(Vector2 direction) => OnPlayerFacingChanged?.Invoke(direction);
        public static void TriggerPlayerHealthChanged(int newHealth) => OnPlayerHealthChanged?.Invoke(newHealth);
        public static void TriggerPlayerGoldChanged(int newGold) => OnPlayerGoldChanged?.Invoke(newGold);
        public static void TriggerPlayerDeath() => OnPlayerDeath?.Invoke();
        public static void TriggerGameStart() => OnGameStart?.Invoke();
        public static void TriggerGameOver() => OnGameOver?.Invoke();
        public static void TriggerLevelComplete() => OnLevelComplete?.Invoke();

        /// <summary>
        /// 모든 이벤트 구독 해제 (씬 전환 시 사용)
        /// </summary>
        public static void ClearAllEvents()
        {
            OnEnterRoom = null;
            OnRoomCleared = null;
            OnDungeonGenerated = null;
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
