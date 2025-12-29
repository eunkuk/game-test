namespace Game.Systems.Encounter
{
    using UnityEngine;
    using Game.Data.Encounter;
    using Game.Core.Events;

    /// <summary>
    /// Encounter 실행기 (EncounterDefinitionSO → EncounterResult)
    /// </summary>
    public class EncounterResolver : MonoBehaviour
    {
        [Header("Prefabs (Init용 임시)")]
        [SerializeField] private GameObject enemyPrefab;

        [Header("Debug")]
        [SerializeField] private bool logResults = true;

        /// <summary>
        /// Encounter 실행 메인 메서드
        /// </summary>
        public EncounterResult Resolve(EncounterDefinitionSO definition, EncounterContext context)
        {
            if (definition == null)
            {
                Debug.LogWarning("[EncounterResolver] Encounter definition is null!");
                return new EncounterResult { Message = "Nothing happens." };
            }

            EncounterResult result = new EncounterResult();

            // 타입별 실행
            switch (definition)
            {
                case EnemySpawnEncounterSO enemySpawn:
                    ResolveEnemySpawn(enemySpawn, context, result);
                    break;

                case TrapEncounterSO trap:
                    ResolveTrap(trap, context, result);
                    break;

                case LootEncounterSO loot:
                    ResolveLoot(loot, context, result);
                    break;

                case EventTextEncounterSO eventText:
                    // UI를 통해 처리, 여기서는 이벤트 발행만
                    ResolveEventText(eventText, context, result);
                    break;

                default:
                    result.Message = $"Unknown encounter type: {definition.GetType().Name}";
                    Debug.LogWarning($"[EncounterResolver] {result.Message}");
                    break;
            }

            if (logResults)
            {
                Debug.Log($"[EncounterResolver] {result}");
            }

            GameEvents.TriggerEncounterResolved(result);
            return result;
        }

        /// <summary>
        /// 적 스폰 Encounter 실행
        /// </summary>
        private void ResolveEnemySpawn(EnemySpawnEncounterSO data, EncounterContext context, EncounterResult result)
        {
            if (enemyPrefab == null)
            {
                result.Message = "Enemy prefab not assigned!";
                Debug.LogWarning($"[EncounterResolver] {result.Message}");
                return;
            }

            System.Random random = new System.Random(context.Seed + context.Room.Id);
            int count = random.Next(data.minCount, data.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i;
                Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.up * data.spawnRadius;
                Vector3 spawnPos = context.SpawnCenter + offset;

                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.name = $"Enemy_{context.Room.Id}_{i}";
                result.SpawnedEnemies.Add(enemy);
            }

            result.Message = $"Spawned {count} enemies!";
        }

        /// <summary>
        /// 함정 Encounter 실행
        /// </summary>
        private void ResolveTrap(TrapEncounterSO data, EncounterContext context, EncounterResult result)
        {
            switch (data.trapType)
            {
                case TrapType.Damage:
                    result.HealthChange = -data.damageAmount;
                    result.Message = $"Trap! Took {data.damageAmount} damage.";
                    break;

                case TrapType.Slow:
                    result.Message = $"Slow trap! Movement slowed for {data.duration}s.";
                    // TODO: 플레이어에게 슬로우 디버프 적용
                    break;

                case TrapType.Poison:
                    result.Message = $"Poison trap! Taking {data.effectStrength} damage per second for {data.duration}s.";
                    // TODO: 플레이어에게 독 디버프 적용
                    break;
            }

            // 플레이어 체력 변경 이벤트 발행 (PlayerStats에서 처리)
            if (result.HealthChange != 0)
            {
                // TODO: PlayerStats 연동
            }
        }

        /// <summary>
        /// 보상 Encounter 실행
        /// </summary>
        private void ResolveLoot(LootEncounterSO data, EncounterContext context, EncounterResult result)
        {
            result.GoldReward = data.goldAmount;
            result.HealthChange = data.healthRestore;

            if (data.goldAmount > 0 && data.healthRestore > 0)
            {
                result.Message = $"Found {data.goldAmount} gold and restored {data.healthRestore} health!";
            }
            else if (data.goldAmount > 0)
            {
                result.Message = $"Found {data.goldAmount} gold!";
            }
            else if (data.healthRestore > 0)
            {
                result.Message = $"Restored {data.healthRestore} health!";
            }
            else
            {
                result.Message = "Found an empty chest.";
            }

            // TODO: 아이템 보상 처리 (data.itemReward)
        }

        /// <summary>
        /// 이벤트 텍스트 Encounter 실행 (UI 연동 필요)
        /// </summary>
        private void ResolveEventText(EventTextEncounterSO data, EncounterContext context, EncounterResult result)
        {
            result.Message = data.eventText;

            // UI 이벤트 발행 (EventChoicePanel에서 처리)
            GameEvents.TriggerEventEncounter(data, context, result);
        }

        /// <summary>
        /// 이벤트 선택지 결과 적용 (UI에서 호출)
        /// </summary>
        public void ApplyEventChoice(EventChoice choice, EncounterResult result)
        {
            result.GoldReward = choice.goldReward;
            result.HealthChange = choice.healthChange;
            result.Message = choice.resultText;

            if (logResults)
            {
                Debug.Log($"[EncounterResolver] Event choice applied: {result.Message}");
            }

            GameEvents.TriggerEncounterResolved(result);
        }
    }
}
