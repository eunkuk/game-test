namespace Game.Systems.Dungeon
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// 방들을 연결하는 복도
    /// </summary>
    public class Corridor
    {
        public Room RoomA { get; }
        public Room RoomB { get; }
        public List<Vector2Int> Tiles { get; }

        public Corridor(Room roomA, Room roomB, List<Vector2Int> tiles)
        {
            RoomA = roomA;
            RoomB = roomB;
            Tiles = tiles;
        }

        public override string ToString()
        {
            return $"Corridor connecting Room {RoomA.Id} to Room {RoomB.Id} ({Tiles.Count} tiles)";
        }
    }
}
