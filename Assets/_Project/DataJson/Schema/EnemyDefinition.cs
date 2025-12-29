namespace Game.DataJson.Schema
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class MonstersData
    {
        public string version;
        public List<EnemyDefinition> monsters;
    }

    [Serializable]
    public class EnemyDefinition
    {
        public string id;
        public string displayName;
        public string archetype; // "Melee", "Ranged", "Caster"
        public string prefabPath;
        public EnemyStats stats;
        public EnemyAI ai;
        public EnemyLoot loot;
    }

    [Serializable]
    public class EnemyStats
    {
        public int maxHealth;
        public float moveSpeed;
        public int attackDamage;
        public float attackRange;
        public float detectionRange;
        public float attackCooldown;
    }

    [Serializable]
    public class EnemyAI
    {
        public string behavior; // "Patrol", "Aggressive", "Defensive"
        public float aggroRadius;
        public float chaseSpeed;
        public float giveUpTime;
    }

    [Serializable]
    public class EnemyLoot
    {
        public int goldMin;
        public int goldMax;
        public float dropChance;
        public List<string> itemPool;
    }
}
