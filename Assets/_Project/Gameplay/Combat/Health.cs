namespace Game.Gameplay.Combat
{
    using UnityEngine;
    using System;
    using Game.Core.Interfaces;
    using Game.Core.Events;

    /// <summary>
    /// 체력 관리 컴포넌트
    /// Player, Enemy 모두 사용
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] private int maxHealth = 100;

        [Header("Runtime")]
        [SerializeField] private int currentHealth;

        public event Action OnDeath;
        public event Action<int, int> OnHealthChanged; // (current, max)

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void SetMaxHealth(int value)
        {
            maxHealth = value;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead()) return;

            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);

            Debug.Log($"[Health] {gameObject.name}이(가) {amount} 데미지를 받았습니다. 체력: {currentHealth}/{maxHealth}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            GameEvents.TriggerHealthChanged(gameObject, currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead()) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            GameEvents.TriggerHealthChanged(gameObject, currentHealth, maxHealth);
        }

        private void Die()
        {
            Debug.Log($"[Health] {gameObject.name}이(가) 사망했습니다");

            OnDeath?.Invoke();
            GameEvents.TriggerDeath(gameObject);
        }

        public bool IsDead() => currentHealth <= 0;

        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public float GetHealthPercent() => (float)currentHealth / maxHealth;
    }
}
