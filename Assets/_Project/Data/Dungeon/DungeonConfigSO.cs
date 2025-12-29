namespace Game.Data.Dungeon
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/Dungeon/Config")]
    public class DungeonConfigSO : ScriptableObject
    {
        [Header("Room Settings")]
        [Tooltip("최소 방 개수")]
        public int minRooms = 8;

        [Tooltip("최대 방 개수")]
        public int maxRooms = 12;

        [Tooltip("방 최소 크기")]
        public Vector2Int minRoomSize = new Vector2Int(5, 5);

        [Tooltip("방 최대 크기")]
        public Vector2Int maxRoomSize = new Vector2Int(12, 12);

        [Header("Corridor Settings")]
        [Tooltip("복도 폭")]
        public int corridorWidth = 2;

        [Header("Generation Settings")]
        [Tooltip("방 배치 실패 시 최대 재시도 횟수")]
        public int maxRetries = 10;

        [Tooltip("방 사이 최소 간격")]
        public int gridSpacing = 2;

        [Header("Seed")]
        [Tooltip("고정 Seed 사용 여부")]
        public bool useFixedSeed = false;

        [Tooltip("고정 Seed 값")]
        public int fixedSeed = 12345;
    }
}
