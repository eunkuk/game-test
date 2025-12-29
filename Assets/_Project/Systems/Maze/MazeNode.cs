namespace Game.Systems.Maze
{
    using UnityEngine;

    public enum MazeNodeType
    {
        Junction,   // 교차로 (3방향 이상)
        Corner,     // 코너 (2방향, 직각)
        DeadEnd,    // 막다른 길
        Corridor    // 일반 복도
    }

    public class MazeNode
    {
        public Vector2Int Position { get; }
        public MazeNodeType Type { get; }
        public int ConnectionCount { get; }

        public MazeNode(Vector2Int position, MazeNodeType type, int connectionCount)
        {
            Position = position;
            Type = type;
            ConnectionCount = connectionCount;
        }

        public override string ToString()
        {
            return $"{Type} at {Position} ({ConnectionCount} connections)";
        }
    }
}
