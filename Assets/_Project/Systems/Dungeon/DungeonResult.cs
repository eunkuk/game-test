namespace Game.Systems.Dungeon
{
    using System.Collections.Generic;

    /// <summary>
    /// 던전 생성 결과
    /// </summary>
    public class DungeonResult
    {
        public List<Room> Rooms { get; }
        public List<Corridor> Corridors { get; }
        public Room StartRoom { get; set; }
        public Room ExitRoom { get; set; }
        public int Seed { get; }

        public DungeonResult(int seed)
        {
            Seed = seed;
            Rooms = new List<Room>();
            Corridors = new List<Corridor>();
        }

        /// <summary>
        /// ID로 방 찾기
        /// </summary>
        public Room GetRoomById(int id)
        {
            return Rooms.Find(r => r.Id == id);
        }

        public override string ToString()
        {
            return $"Dungeon (Seed: {Seed}, Rooms: {Rooms.Count}, Corridors: {Corridors.Count})";
        }
    }
}
