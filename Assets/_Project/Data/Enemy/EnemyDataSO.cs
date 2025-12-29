namespace Game.Data.Enemy
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/Enemy/EnemyData")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("적 이름")]
        public string enemyName = "Enemy";

        [Tooltip("적 프리팹")]
        public GameObject prefab;

        [Tooltip("적 스프라이트 (프리팹이 없을 경우)")]
        public Sprite sprite;

        [Header("Stats")]
        [Tooltip("최대 체력")]
        public int maxHealth = 50;

        [Tooltip("공격력")]
        public int attackDamage = 10;

        [Tooltip("이동 속도")]
        public float moveSpeed = 2f;

        [Tooltip("공격 범위")]
        public float attackRange = 1.5f;

        [Tooltip("탐지 범위")]
        public float detectionRange = 5f;

        [Header("Rewards")]
        [Tooltip("처치 시 골드")]
        public int goldReward = 10;

        [Tooltip("경험치 (확장용)")]
        public int expReward = 5;
    }
}
