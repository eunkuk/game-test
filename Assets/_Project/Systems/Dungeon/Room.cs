namespace Game.Systems.Dungeon
{
    using UnityEngine;

    /// <summary>
    /// 던전의 방 정보
    /// </summary>
    public class Room
    {
        public int Id { get; }
        public RectInt Bounds { get; }
        public Vector2Int Center => new Vector2Int(
            Bounds.x + Bounds.width / 2,
            Bounds.y + Bounds.height / 2
        );

        public bool IsStartRoom { get; set; }
        public bool IsExitRoom { get; set; }
        public bool IsVisited { get; set; }

        public Room(int id, RectInt bounds)
        {
            Id = id;
            Bounds = bounds;
        }

        /// <summary>
        /// 다른 방과 겹치는지 확인 (spacing 고려)
        /// </summary>
        public bool Intersects(Room other, int spacing = 0)
        {
            return !(Bounds.xMax + spacing < other.Bounds.xMin ||
                     Bounds.xMin - spacing > other.Bounds.xMax ||
                     Bounds.yMax + spacing < other.Bounds.yMin ||
                     Bounds.yMin - spacing > other.Bounds.yMax);
        }

        /// <summary>
        /// 방의 모든 타일 위치 반환
        /// </summary>
        public Vector2Int[] GetAllTiles()
        {
            int count = Bounds.width * Bounds.height;
            Vector2Int[] tiles = new Vector2Int[count];
            int index = 0;

            for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            {
                for (int y = Bounds.yMin; y < Bounds.yMax; y++)
                {
                    tiles[index++] = new Vector2Int(x, y);
                }
            }

            return tiles;
        }

        public override string ToString()
        {
            return $"Room {Id} at {Bounds} (Start: {IsStartRoom}, Exit: {IsExitRoom})";
        }
    }
}
