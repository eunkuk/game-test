namespace Game.DataJson.Registry
{
    using UnityEngine;
    using System.Collections.Generic;
    using Game.DataJson.Schema;

    public class EnemyRegistry : MonoBehaviour
    {
        private static EnemyRegistry instance;
        public static EnemyRegistry Instance => instance;

        private Dictionary<string, EnemyDefinition> registry = new Dictionary<string, EnemyDefinition>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Register(MonstersData data)
        {
            registry.Clear();

            foreach (var monster in data.monsters)
            {
                registry[monster.id] = monster;
            }

            Debug.Log($"[EnemyRegistry] Registered {registry.Count} enemies");
        }

        public EnemyDefinition Get(string id)
        {
            if (registry.TryGetValue(id, out EnemyDefinition def))
            {
                return def;
            }

            Debug.LogWarning($"[EnemyRegistry] Enemy not found: {id}");
            return null;
        }

        public bool Has(string id) => registry.ContainsKey(id);

        public List<EnemyDefinition> GetAll() => new List<EnemyDefinition>(registry.Values);

        public List<string> GetAllIds() => new List<string>(registry.Keys);
    }
}
