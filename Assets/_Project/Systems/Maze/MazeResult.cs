namespace Game.Systems.Maze
{
    using UnityEngine;
    using System.Collections.Generic;

    public class MazeResult
    {
        public int Seed { get; }
        public int Width { get; }
        public int Height { get; }

        public HashSet<Vector2Int> FloorCells { get; }
        public HashSet<Vector2Int> WallCells { get; }

        public Vector2Int Start { get; set; }
        public Vector2Int Exit { get; set; }

        public List<MazeNode> Junctions { get; }
        public List<MazeNode> Corners { get; }
        public List<MazeNode> DeadEnds { get; }

        public RectInt Bounds { get; }

        public MazeResult(int seed, int width, int height)
        {
            Seed = seed;
            Width = width;
            Height = height;

            FloorCells = new HashSet<Vector2Int>();
            WallCells = new HashSet<Vector2Int>();

            Junctions = new List<MazeNode>();
            Corners = new List<MazeNode>();
            DeadEnds = new List<MazeNode>();

            Bounds = new RectInt(0, 0, width, height);
        }

        public bool IsFloor(Vector2Int cell) => FloorCells.Contains(cell);
        public bool IsWall(Vector2Int cell) => WallCells.Contains(cell);
        public bool InBounds(Vector2Int cell) => Bounds.Contains(cell);

        public override string ToString()
        {
            return $"Maze (Seed: {Seed}, Size: {Width}x{Height}, " +
                   $"Floor: {FloorCells.Count}, Junctions: {Junctions.Count}, " +
                   $"Corners: {Corners.Count}, DeadEnds: {DeadEnds.Count})";
        }
    }
}
