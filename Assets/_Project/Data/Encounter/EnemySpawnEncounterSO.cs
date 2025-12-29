namespace Game.Data.Encounter
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/Encounter/EnemySpawn")]
    public class EnemySpawnEncounterSO : EncounterDefinitionSO
    {
        [Header("Enemy Settings")]
        [Tooltip("스폰할 적 타입 (EnemyDataSO)")]
        public ScriptableObject enemyType; // EnemyDataSO 참조 (순환 의존 방지)

        [Tooltip("최소 스폰 개수")]
        public int minCount = 1;

        [Tooltip("최대 스폰 개수")]
        public int maxCount = 3;

        [Tooltip("스폰 반경")]
        public float spawnRadius = 3f;

        public override string GetTypeName() => "Enemy Spawn";
    }
}
