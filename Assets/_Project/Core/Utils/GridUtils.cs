namespace Game.Core.Utils
{
    using UnityEngine;

    /// <summary>
    /// 그리드 관련 유틸리티 함수
    /// </summary>
    public static class GridUtils
    {
        /// <summary>
        /// 월드 좌표를 그리드 셀 좌표로 변환
        /// </summary>
        public static Vector2Int WorldToCell(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x),
                Mathf.FloorToInt(worldPos.y)
            );
        }

        /// <summary>
        /// 그리드 셀 좌표를 월드 좌표로 변환 (셀 중심)
        /// </summary>
        public static Vector2 CellToWorld(Vector2Int cellPos)
        {
            return new Vector2(cellPos.x + 0.5f, cellPos.y + 0.5f);
        }

        /// <summary>
        /// 맨하탄 거리 계산
        /// </summary>
        public static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// 체비셰프 거리 계산 (대각선 포함)
        /// </summary>
        public static int ChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        /// <summary>
        /// 두 RectInt가 겹치는지 확인 (spacing 고려)
        /// </summary>
        public static bool Overlaps(RectInt a, RectInt b, int spacing = 0)
        {
            return !(a.xMax + spacing < b.xMin ||
                     a.xMin - spacing > b.xMax ||
                     a.yMax + spacing < b.yMin ||
                     a.yMin - spacing > b.yMax);
        }

        /// <summary>
        /// RectInt의 중심점 반환
        /// </summary>
        public static Vector2Int GetCenter(RectInt rect)
        {
            return new Vector2Int(
                rect.x + rect.width / 2,
                rect.y + rect.height / 2
            );
        }

        /// <summary>
        /// 방향 벡터를 8방향 중 하나로 정규화
        /// </summary>
        public static Vector2Int GetCardinalDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
                return Vector2Int.zero;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 8방향으로 스냅
            if (angle >= -22.5f && angle < 22.5f) return Vector2Int.right;
            if (angle >= 22.5f && angle < 67.5f) return new Vector2Int(1, 1);
            if (angle >= 67.5f && angle < 112.5f) return Vector2Int.up;
            if (angle >= 112.5f && angle < 157.5f) return new Vector2Int(-1, 1);
            if (angle >= 157.5f || angle < -157.5f) return Vector2Int.left;
            if (angle >= -157.5f && angle < -112.5f) return new Vector2Int(-1, -1);
            if (angle >= -112.5f && angle < -67.5f) return Vector2Int.down;
            return new Vector2Int(1, -1);
        }
    }
}
