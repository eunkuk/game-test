namespace Game.Systems.Encounter
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Encounter 실행 결과
    /// </summary>
    public class EncounterResult
    {
        public string Message { get; set; }
        public int GoldReward { get; set; }
        public int HealthChange { get; set; }
        public List<GameObject> SpawnedEnemies { get; set; }
        public GameObject ItemReward { get; set; }

        public EncounterResult()
        {
            SpawnedEnemies = new List<GameObject>();
        }

        public override string ToString()
        {
            return $"EncounterResult: {Message} (Gold: {GoldReward}, Health: {HealthChange}, Enemies: {SpawnedEnemies.Count})";
        }
    }
}
