namespace Game.Systems.Dungeon
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using Game.Data.Dungeon;
    using Game.Core.Events;

    /// <summary>
    /// 던전 랜덤 생성기
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DungeonConfigSO config;
        [SerializeField] private TilemapPainter painter;

        [Header("Runtime Info")]
        [SerializeField] private bool autoGenerate = false;
        [SerializeField] private int? seedOverride;

        private DungeonResult currentDungeon;

        private void Start()
        {
            if (autoGenerate)
            {
                Generate(seedOverride);
            }
        }

        /// <summary>
        /// 던전 생성 메인 메서드
        /// </summary>
        public DungeonResult Generate(int? seedOverride = null)
        {
            if (config == null)
            {
                Debug.LogError("DungeonConfigSO is not assigned!");
                return null;
            }

            int seed = seedOverride ?? (config.useFixedSeed ? config.fixedSeed : UnityEngine.Random.Range(0, int.MaxValue));
            System.Random random = new System.Random(seed);

            Debug.Log($"[DungeonGenerator] Generating dungeon with seed: {seed}");

            DungeonResult result = new DungeonResult(seed);

            // 1. 방 생성
            if (!GenerateRooms(result, random))
            {
                Debug.LogError("[DungeonGenerator] Failed to generate rooms after max retries");
                return null;
            }

            // 2. 방 연결
            GenerateCorridors(result, random);

            // 3. 시작/출구 설정
            AssignSpecialRooms(result);

            // 4. Tilemap 페인팅
            if (painter != null)
            {
                painter.PaintDungeon(result);
            }
            else
            {
                Debug.LogWarning("[DungeonGenerator] TilemapPainter is not assigned!");
            }

            currentDungeon = result;
            GameEvents.TriggerDungeonGenerated(result);

            Debug.Log($"[DungeonGenerator] {result}");
            return result;
        }

        /// <summary>
        /// 방 배치 (겹침 방지)
        /// </summary>
        private bool GenerateRooms(DungeonResult result, System.Random random)
        {
            int roomCount = random.Next(config.minRooms, config.maxRooms + 1);
            int attempts = 0;
            int maxAttempts = config.maxRetries * roomCount;

            Debug.Log($"[DungeonGenerator] Attempting to generate {roomCount} rooms");

            while (result.Rooms.Count < roomCount && attempts < maxAttempts)
            {
                attempts++;

                // 랜덤 크기
                int width = random.Next(config.minRoomSize.x, config.maxRoomSize.x + 1);
                int height = random.Next(config.minRoomSize.y, config.maxRoomSize.y + 1);

                // 랜덤 위치 (그리드 기반)
                int gridRange = 20;
                int x = random.Next(-gridRange, gridRange) * (config.maxRoomSize.x + config.gridSpacing);
                int y = random.Next(-gridRange, gridRange) * (config.maxRoomSize.y + config.gridSpacing);

                RectInt bounds = new RectInt(x, y, width, height);
                Room newRoom = new Room(result.Rooms.Count, bounds);

                // 겹침 검사
                bool overlaps = false;
                foreach (var existingRoom in result.Rooms)
                {
                    if (newRoom.Intersects(existingRoom, config.gridSpacing))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    result.Rooms.Add(newRoom);
                    Debug.Log($"[DungeonGenerator] Created {newRoom}");
                }
            }

            bool success = result.Rooms.Count >= config.minRooms;
            Debug.Log($"[DungeonGenerator] Room generation {(success ? "succeeded" : "failed")} with {result.Rooms.Count} rooms (attempts: {attempts})");
            return success;
        }

        /// <summary>
        /// 방 연결 (순차 연결)
        /// </summary>
        private void GenerateCorridors(DungeonResult result, System.Random random)
        {
            Debug.Log($"[DungeonGenerator] Connecting rooms...");

            for (int i = 0; i < result.Rooms.Count - 1; i++)
            {
                Room roomA = result.Rooms[i];
                Room roomB = result.Rooms[i + 1];

                List<Vector2Int> tiles = CreateLShapedCorridor(roomA.Center, roomB.Center, random);
                Corridor corridor = new Corridor(roomA, roomB, tiles);
                result.Corridors.Add(corridor);

                Debug.Log($"[DungeonGenerator] {corridor}");
            }
        }

        /// <summary>
        /// L자 복도 생성 (수평 → 수직 또는 수직 → 수평)
        /// </summary>
        private List<Vector2Int> CreateLShapedCorridor(Vector2Int start, Vector2Int end, System.Random random)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();

            // 수평 우선 또는 수직 우선 (랜덤)
            bool horizontalFirst = random.Next(2) == 0;

            if (horizontalFirst)
            {
                // 수평 → 수직
                for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                {
                    tiles.Add(new Vector2Int(x, start.y));
                }
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    tiles.Add(new Vector2Int(end.x, y));
                }
            }
            else
            {
                // 수직 → 수평
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    tiles.Add(new Vector2Int(start.x, y));
                }
                for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                {
                    tiles.Add(new Vector2Int(x, end.y));
                }
            }

            return tiles;
        }

        /// <summary>
        /// 시작방/출구방 지정
        /// </summary>
        private void AssignSpecialRooms(DungeonResult result)
        {
            if (result.Rooms.Count == 0) return;

            result.StartRoom = result.Rooms[0];
            result.StartRoom.IsStartRoom = true;

            // 출구는 시작방과 가장 먼 방
            Room farthest = result.Rooms[0];
            float maxDist = 0;
            foreach (var room in result.Rooms)
            {
                float dist = Vector2Int.Distance(result.StartRoom.Center, room.Center);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthest = room;
                }
            }

            result.ExitRoom = farthest;
            result.ExitRoom.IsExitRoom = true;

            Debug.Log($"[DungeonGenerator] Start: Room {result.StartRoom.Id}, Exit: Room {result.ExitRoom.Id}");
        }

        public DungeonResult GetCurrentDungeon() => currentDungeon;

        private void OnDrawGizmos()
        {
            if (painter != null)
            {
                painter.OnDrawGizmos();
            }
        }
    }
}
