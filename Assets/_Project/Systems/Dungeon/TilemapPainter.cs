namespace Game.Systems.Dungeon
{
    using UnityEngine;
    using UnityEngine.Tilemaps;
    using Game.Data.Dungeon;
    using System.Collections.Generic;

    /// <summary>
    /// 던전을 Tilemap에 페인팅
    /// </summary>
    public class TilemapPainter : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap floorTilemap;
        [SerializeField] private Tilemap wallTilemap;

        [Header("Palette")]
        [SerializeField] private TilePaletteSO palette;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool drawRoomBounds = true;
        [SerializeField] private bool drawCorridorLines = true;

        private DungeonResult currentDungeon;

        /// <summary>
        /// 던전을 Tilemap에 페인팅
        /// </summary>
        public void PaintDungeon(DungeonResult dungeon)
        {
            if (floorTilemap == null || wallTilemap == null)
            {
                Debug.LogError("[TilemapPainter] Tilemaps are not assigned!");
                return;
            }

            if (palette == null)
            {
                Debug.LogError("[TilemapPainter] TilePaletteSO is not assigned!");
                return;
            }

            currentDungeon = dungeon;

            // 초기화
            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();

            System.Random random = new System.Random(dungeon.Seed);

            Debug.Log("[TilemapPainter] Painting dungeon...");

            // 방 페인팅
            foreach (var room in dungeon.Rooms)
            {
                PaintRoom(room, random);
            }

            // 복도 페인팅
            foreach (var corridor in dungeon.Corridors)
            {
                PaintCorridor(corridor, random);
            }

            // 벽 생성 (바닥 타일 주변)
            GenerateWalls();

            Debug.Log("[TilemapPainter] Painting complete");
        }

        /// <summary>
        /// 방 페인팅
        /// </summary>
        private void PaintRoom(Room room, System.Random random)
        {
            for (int x = room.Bounds.xMin; x < room.Bounds.xMax; x++)
            {
                for (int y = room.Bounds.yMin; y < room.Bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    floorTilemap.SetTile(pos, palette.GetRandomFloorTile(random));
                }
            }
        }

        /// <summary>
        /// 복도 페인팅 (폭 고려)
        /// </summary>
        private void PaintCorridor(Corridor corridor, System.Random random)
        {
            HashSet<Vector2Int> paintedTiles = new HashSet<Vector2Int>();

            foreach (var tile in corridor.Tiles)
            {
                // 복도 폭만큼 확장 (기본 1칸씩 확장)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector2Int expandedTile = new Vector2Int(tile.x + dx, tile.y + dy);
                        if (!paintedTiles.Contains(expandedTile))
                        {
                            paintedTiles.Add(expandedTile);
                            Vector3Int pos = new Vector3Int(expandedTile.x, expandedTile.y, 0);

                            // 이미 바닥이 있으면 스킵
                            if (floorTilemap.GetTile(pos) == null)
                            {
                                floorTilemap.SetTile(pos, palette.GetRandomFloorTile(random));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 벽 생성 (바닥 타일 주변)
        /// </summary>
        private void GenerateWalls()
        {
            BoundsInt bounds = floorTilemap.cellBounds;

            // 범위를 조금 확장해서 검사
            for (int x = bounds.xMin - 1; x <= bounds.xMax + 1; x++)
            {
                for (int y = bounds.yMin - 1; y <= bounds.yMax + 1; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);

                    // 바닥 타일이 없으면서 인접에 바닥이 있으면 벽
                    if (floorTilemap.GetTile(pos) == null && HasAdjacentFloor(pos))
                    {
                        wallTilemap.SetTile(pos, palette.wallTile);
                    }
                }
            }
        }

        /// <summary>
        /// 인접한 타일 중 바닥이 있는지 확인
        /// </summary>
        private bool HasAdjacentFloor(Vector3Int pos)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector3Int neighbor = pos + new Vector3Int(dx, dy, 0);
                    if (floorTilemap.GetTile(neighbor) != null)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gizmos 그리기 (디버그용)
        /// </summary>
        public void OnDrawGizmos()
        {
            if (!drawGizmos || currentDungeon == null) return;

            // 방 경계 그리기
            if (drawRoomBounds)
            {
                foreach (var room in currentDungeon.Rooms)
                {
                    Gizmos.color = room.IsStartRoom ? Color.green :
                                   (room.IsExitRoom ? Color.red : Color.blue);

                    Vector3 center = new Vector3(room.Center.x, room.Center.y, 0);
                    Vector3 size = new Vector3(room.Bounds.width, room.Bounds.height, 0);
                    Gizmos.DrawWireCube(center, size);

                    // 방 ID 표시 (SceneView에서만 보임)
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(center, $"R{room.Id}");
                    #endif
                }
            }

            // 연결선 그리기
            if (drawCorridorLines)
            {
                Gizmos.color = Color.yellow;
                foreach (var corridor in currentDungeon.Corridors)
                {
                    Vector3 startPos = new Vector3(corridor.RoomA.Center.x, corridor.RoomA.Center.y, 0);
                    Vector3 endPos = new Vector3(corridor.RoomB.Center.x, corridor.RoomB.Center.y, 0);
                    Gizmos.DrawLine(startPos, endPos);
                }
            }
        }

        /// <summary>
        /// 던전 초기화
        /// </summary>
        public void Clear()
        {
            if (floorTilemap != null) floorTilemap.ClearAllTiles();
            if (wallTilemap != null) wallTilemap.ClearAllTiles();
            currentDungeon = null;
        }
    }
}
