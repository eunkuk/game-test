namespace Game.Systems.Maze
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.Core.Events;

    public class MazeGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MazeConfig config = new MazeConfig();

        [Header("References")]
        [SerializeField] private MazeTilemapPainter painter;

        [Header("Runtime")]
        [SerializeField] private bool autoGenerate = false;

        private MazeResult currentMaze;

        private void Start()
        {
            if (autoGenerate)
            {
                Generate(null);
            }
        }

        public MazeResult Generate(int? seedOverride = null)
        {
            config.Validate();

            int seed = seedOverride ?? (config.useFixedSeed ? config.fixedSeed : Random.Range(0, int.MaxValue));
            System.Random random = new System.Random(seed);

            Debug.Log($"[MazeGenerator] Generating maze with seed: {seed}");

            MazeResult result = new MazeResult(seed, config.width, config.height);

            GenerateMazeDFS(result, random);

            if (config.deadEndRemovalRate > 0)
            {
                RemoveDeadEnds(result, random);
            }

            AssignStartExit(result);
            AnalyzeNodes(result);

            if (painter != null)
            {
                painter.PaintMaze(result);
            }

            currentMaze = result;
            GameEvents.TriggerMazeGenerated(result);

            Debug.Log($"[MazeGenerator] {result}");
            return result;
        }

        private void GenerateMazeDFS(MazeResult result, System.Random random)
        {
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    result.WallCells.Add(new Vector2Int(x, y));
                }
            }

            Vector2Int start = new Vector2Int(
                random.Next(result.Width / 2) * 2 + 1,
                random.Next(result.Height / 2) * 2 + 1
            );

            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(start);

            Vector2Int[] directions = {
                Vector2Int.up * 2,
                Vector2Int.down * 2,
                Vector2Int.left * 2,
                Vector2Int.right * 2
            };

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();
                MakeCellFloor(result, current);

                List<Vector2Int> neighbors = new List<Vector2Int>();
                foreach (var dir in directions)
                {
                    Vector2Int next = current + dir;
                    if (result.InBounds(next) && result.IsWall(next))
                    {
                        neighbors.Add(next);
                    }
                }

                if (neighbors.Count > 0)
                {
                    Vector2Int chosen = neighbors[random.Next(neighbors.Count)];
                    Vector2Int between = current + (chosen - current) / 2;
                    MakeCellFloor(result, between);
                    MakeCellFloor(result, chosen);
                    stack.Push(chosen);
                }
                else
                {
                    stack.Pop();
                }
            }
        }

        private void MakeCellFloor(MazeResult result, Vector2Int cell)
        {
            if (result.WallCells.Remove(cell))
            {
                result.FloorCells.Add(cell);
            }
        }

        private void RemoveDeadEnds(MazeResult result, System.Random random)
        {
            List<Vector2Int> deadEnds = FindDeadEnds(result);
            int removeCount = Mathf.FloorToInt(deadEnds.Count * config.deadEndRemovalRate);

            for (int i = 0; i < removeCount && deadEnds.Count > 0; i++)
            {
                int index = random.Next(deadEnds.Count);
                Vector2Int deadEnd = deadEnds[index];
                deadEnds.RemoveAt(index);

                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = deadEnd + dir;
                    if (result.InBounds(neighbor) && result.IsWall(neighbor))
                    {
                        MakeCellFloor(result, neighbor);
                        break;
                    }
                }
            }
        }

        private List<Vector2Int> FindDeadEnds(MazeResult result)
        {
            List<Vector2Int> deadEnds = new List<Vector2Int>();
            foreach (var cell in result.FloorCells)
            {
                if (GetConnectionCount(result, cell) == 1)
                {
                    deadEnds.Add(cell);
                }
            }
            return deadEnds;
        }

        private int GetConnectionCount(MazeResult result, Vector2Int cell)
        {
            int count = 0;
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                Vector2Int neighbor = cell + dir;
                if (result.IsFloor(neighbor))
                {
                    count++;
                }
            }

            return count;
        }

        private void AssignStartExit(MazeResult result)
        {
            result.Start = FindNearestFloorTo(result, new Vector2Int(0, 0));
            result.Exit = FindFarthestFloorFrom(result, result.Start);
            Debug.Log($"[MazeGenerator] Start: {result.Start}, Exit: {result.Exit}");
        }

        private Vector2Int FindNearestFloorTo(MazeResult result, Vector2Int target)
        {
            Vector2Int nearest = Vector2Int.zero;
            float minDist = float.MaxValue;

            foreach (var cell in result.FloorCells)
            {
                float dist = Vector2Int.Distance(cell, target);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = cell;
                }
            }

            return nearest;
        }

        private Vector2Int FindFarthestFloorFrom(MazeResult result, Vector2Int start)
        {
            Vector2Int farthest = start;
            float maxDist = 0;

            foreach (var cell in result.FloorCells)
            {
                float dist = Vector2Int.Distance(cell, start);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthest = cell;
                }
            }

            return farthest;
        }

        private void AnalyzeNodes(MazeResult result)
        {
            foreach (var cell in result.FloorCells)
            {
                int connections = GetConnectionCount(result, cell);
                MazeNodeType type;

                if (connections >= 3)
                {
                    type = MazeNodeType.Junction;
                    result.Junctions.Add(new MazeNode(cell, type, connections));
                }
                else if (connections == 2 && IsCorner(result, cell))
                {
                    type = MazeNodeType.Corner;
                    result.Corners.Add(new MazeNode(cell, type, connections));
                }
                else if (connections == 1)
                {
                    type = MazeNodeType.DeadEnd;
                    result.DeadEnds.Add(new MazeNode(cell, type, connections));
                }
            }

            Debug.Log($"[MazeGenerator] Analyzed nodes: {result.Junctions.Count} junctions, " +
                      $"{result.Corners.Count} corners, {result.DeadEnds.Count} dead ends");
        }

        private bool IsCorner(MazeResult result, Vector2Int cell)
        {
            bool up = result.IsFloor(cell + Vector2Int.up);
            bool down = result.IsFloor(cell + Vector2Int.down);
            bool left = result.IsFloor(cell + Vector2Int.left);
            bool right = result.IsFloor(cell + Vector2Int.right);

            return (up && left) || (up && right) || (down && left) || (down && right);
        }

        public MazeResult GetCurrentMaze() => currentMaze;

        private void OnDrawGizmos()
        {
            if (currentMaze == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(currentMaze.Start.x, currentMaze.Start.y, 0), 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(currentMaze.Exit.x, currentMaze.Exit.y, 0), 0.5f);

            Gizmos.color = Color.yellow;
            foreach (var junction in currentMaze.Junctions)
            {
                Gizmos.DrawSphere(new Vector3(junction.Position.x, junction.Position.y, 0), 0.3f);
            }

            Gizmos.color = Color.cyan;
            foreach (var corner in currentMaze.Corners)
            {
                Gizmos.DrawWireSphere(new Vector3(corner.Position.x, corner.Position.y, 0), 0.2f);
            }
        }
    }
}
