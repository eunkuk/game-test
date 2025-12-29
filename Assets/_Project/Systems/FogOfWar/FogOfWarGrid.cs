namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// Fog-of-War 그리드 데이터 (visited/visible 관리)
    /// </summary>
    public class FogOfWarGrid
    {
        private int width;
        private int height;
        private Vector2Int offset;

        private HashSet<Vector2Int> visitedCells;
        private HashSet<Vector2Int> visibleCells;

        public int Width => width;
        public int Height => height;
        public Vector2Int Offset => offset;

        public FogOfWarGrid(int width, int height, Vector2Int offset)
        {
            this.width = width;
            this.height = height;
            this.offset = offset;

            visitedCells = new HashSet<Vector2Int>();
            visibleCells = new HashSet<Vector2Int>();
        }

        /// <summary>
        /// 시야 업데이트 (FOV 결과 반영)
        /// </summary>
        public void UpdateVisibility(Vector2[] viewPoints)
        {
            visibleCells.Clear();

            if (viewPoints == null) return;

            foreach (var worldPoint in viewPoints)
            {
                Vector2Int cell = WorldToCell(worldPoint);
                visibleCells.Add(cell);
                visitedCells.Add(cell); // 한 번 본 셀은 visited로 승격

                // 주변 셀도 visible로 처리 (부드러운 효과)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector2Int neighbor = cell + new Vector2Int(dx, dy);
                        visibleCells.Add(neighbor);
                        visitedCells.Add(neighbor);
                    }
                }
            }
        }

        /// <summary>
        /// 현재 시야에 보이는지
        /// </summary>
        public bool IsVisible(Vector2Int cell) => visibleCells.Contains(cell);

        /// <summary>
        /// 탐색한 적 있는지
        /// </summary>
        public bool IsVisited(Vector2Int cell) => visitedCells.Contains(cell);

        /// <summary>
        /// 미탐색 영역인지
        /// </summary>
        public bool IsUnexplored(Vector2Int cell) => !visitedCells.Contains(cell);

        /// <summary>
        /// 월드 좌표 → 셀 좌표 변환
        /// </summary>
        public Vector2Int WorldToCell(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x) - offset.x,
                Mathf.FloorToInt(worldPos.y) - offset.y
            );
        }

        /// <summary>
        /// 셀 좌표 → 월드 좌표 변환 (셀 중심)
        /// </summary>
        public Vector2 CellToWorld(Vector2Int cell)
        {
            return new Vector2(
                cell.x + offset.x + 0.5f,
                cell.y + offset.y + 0.5f
            );
        }

        /// <summary>
        /// 모든 Fog 리셋
        /// </summary>
        public void Reset()
        {
            visitedCells.Clear();
            visibleCells.Clear();
        }

        /// <summary>
        /// 특정 영역의 Fog 제거 (디버그/치트용)
        /// </summary>
        public void RevealArea(Vector2Int center, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        Vector2Int cell = center + new Vector2Int(x, y);
                        visitedCells.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// 탐색 진행률 (0~1)
        /// </summary>
        public float GetExplorationProgress()
        {
            int totalCells = width * height;
            return totalCells > 0 ? (float)visitedCells.Count / totalCells : 0f;
        }

        public override string ToString()
        {
            return $"FogOfWarGrid ({width}x{height}, Visited: {visitedCells.Count}, Visible: {visibleCells.Count})";
        }
    }
}
