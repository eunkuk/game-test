namespace Game.Gameplay.Player
{
    using UnityEngine;
    using Game.Core.Events;

    /// <summary>
    /// 플레이어 스탯 관리 (체력, 골드 등)
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;

        [Header("Gold")]
        [SerializeField] private int gold = 0;

        [Header("Debug")]
        [SerializeField] private bool logChanges = true;

        private void Start()
        {
            currentHealth = maxHealth;
            GameEvents.TriggerPlayerHealthChanged(currentHealth);
            GameEvents.TriggerPlayerGoldChanged(gold);
        }

        private void OnEnable()
        {
            GameEvents.OnEncounterResolved += OnEncounterResolved;
        }

        private void OnDisable()
        {
            GameEvents.OnEncounterResolved -= OnEncounterResolved;
        }

        /// <summary>
        /// Encounter 결과 적용 (GameEvents는 object 타입 사용)
        /// </summary>
        private void OnEncounterResolved(object obj)
        {
            // GameEvents가 object 타입을 사용하므로 캐스팅 필요
            var result = obj as Game.Systems.Encounter.EncounterResult;
            if (result == null) return;

            if (result.HealthChange != 0)
            {
                ChangeHealth(result.HealthChange);
            }

            if (result.GoldReward != 0)
            {
                AddGold(result.GoldReward);
            }
        }

        /// <summary>
        /// 체력 변경
        /// </summary>
        public void ChangeHealth(int amount)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

            if (logChanges)
            {
                Debug.Log($"[PlayerStats] Health changed by {amount}, Current: {currentHealth}/{maxHealth}");
            }

            GameEvents.TriggerPlayerHealthChanged(currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(int amount)
        {
            gold += amount;

            if (logChanges)
            {
                Debug.Log($"[PlayerStats] Gold changed by {amount}, Current: {gold}");
            }

            GameEvents.TriggerPlayerGoldChanged(gold);
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void Die()
        {
            Debug.Log("[PlayerStats] Player died!");
            GameEvents.TriggerPlayerDeath();

            // TODO: Game Over 처리
        }

        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public int GetGold() => gold;
        public bool IsAlive() => currentHealth > 0;
    }
}
