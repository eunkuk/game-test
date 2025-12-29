namespace Game.Systems.Encounter
{
    using UnityEngine;
    using Game.Systems.Dungeon;

    /// <summary>
    /// Encounter 실행 컨텍스트 (실행 시 필요한 정보)
    /// </summary>
    public class EncounterContext
    {
        public int Seed { get; }
        public Room Room { get; }
        public GameObject Player { get; }
        public Vector3 SpawnCenter { get; }

        public EncounterContext(int seed, Room room, GameObject player)
        {
            Seed = seed;
            Room = room;
            Player = player;
            SpawnCenter = new Vector3(room.Center.x, room.Center.y, 0);
        }

        public override string ToString()
        {
            return $"EncounterContext (Seed: {Seed}, Room: {Room.Id}, Player: {Player.name})";
        }
    }
}
