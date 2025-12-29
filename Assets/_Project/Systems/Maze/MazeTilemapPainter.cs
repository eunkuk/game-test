namespace Game.Systems.Maze
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class MazeTilemapPainter : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap floorTilemap;
        [SerializeField] private Tilemap wallTilemap;

        [Header("Tiles")]
        [SerializeField] private TileBase floorTile;
        [SerializeField] private TileBase wallTile;

        private MazeResult currentMaze;

        public void PaintMaze(MazeResult maze)
        {
            if (floorTilemap == null || wallTilemap == null)
            {
                Debug.LogError("[MazeTilemapPainter] Tilemaps not assigned!");
                return;
            }

            currentMaze = maze;

            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();

            Debug.Log("[MazeTilemapPainter] Painting maze...");

            foreach (var cell in maze.FloorCells)
            {
                Vector3Int pos = new Vector3Int(cell.x, cell.y, 0);
                floorTilemap.SetTile(pos, floorTile);
            }

            foreach (var cell in maze.WallCells)
            {
                Vector3Int pos = new Vector3Int(cell.x, cell.y, 0);

                if (HasAdjacentFloor(maze, cell))
                {
                    wallTilemap.SetTile(pos, wallTile);
                }
            }

            Debug.Log("[MazeTilemapPainter] Painting complete");
        }

        private bool HasAdjacentFloor(MazeResult maze, Vector2Int cell)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                if (maze.IsFloor(cell + dir))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
