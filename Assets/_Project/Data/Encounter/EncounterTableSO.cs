namespace Game.Data.Encounter
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// 가중치 기반 Encounter 엔트리
    /// </summary>
    [System.Serializable]
    public class WeightedEncounterEntry
    {
        [Tooltip("가중치 (높을수록 자주 나옴)")]
        public int weight = 10;

        [Tooltip("Encounter 정의")]
        public EncounterDefinitionSO encounter;
    }

    /// <summary>
    /// Encounter 테이블 (가중치 랜덤)
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Encounter/EncounterTable")]
    public class EncounterTableSO : ScriptableObject
    {
        [Tooltip("Encounter 엔트리 리스트")]
        public List<WeightedEncounterEntry> entries = new List<WeightedEncounterEntry>();

        /// <summary>
        /// 가중치 기반 랜덤 Encounter 선택
        /// </summary>
        public EncounterDefinitionSO Roll(System.Random random)
        {
            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning($"EncounterTable '{name}' has no entries!");
                return null;
            }

            int totalWeight = 0;
            foreach (var entry in entries)
            {
                if (entry.encounter != null)
                    totalWeight += entry.weight;
            }

            if (totalWeight <= 0)
            {
                Debug.LogWarning($"EncounterTable '{name}' has zero total weight!");
                return null;
            }

            int roll = random.Next(totalWeight);
            int cumulative = 0;

            foreach (var entry in entries)
            {
                if (entry.encounter == null) continue;

                cumulative += entry.weight;
                if (roll < cumulative)
                    return entry.encounter;
            }

            // Fallback (should not reach here)
            return entries[0].encounter;
        }

        /// <summary>
        /// 가중치 합계 반환 (디버그용)
        /// </summary>
        public int GetTotalWeight()
        {
            int total = 0;
            foreach (var entry in entries)
            {
                if (entry.encounter != null)
                    total += entry.weight;
            }
            return total;
        }
    }
}
