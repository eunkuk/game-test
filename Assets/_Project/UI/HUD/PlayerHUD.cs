namespace Game.UI.HUD
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using Game.Core.Events;

    /// <summary>
    /// 플레이어 HUD (체력, 골드 표시)
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Slider healthSlider;

        [Header("Gold")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float messageDuration = 3f;

        private int currentHealth;
        private int maxHealth = 100;
        private int gold;
        private float messageTimer;

        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged += OnHealthChanged;
            GameEvents.OnPlayerGoldChanged += OnGoldChanged;
            GameEvents.OnEncounterResolved += OnEncounterResolved;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged -= OnHealthChanged;
            GameEvents.OnPlayerGoldChanged -= OnGoldChanged;
            GameEvents.OnEncounterResolved -= OnEncounterResolved;
        }

        private void Update()
        {
            // 메시지 타이머
            if (messageTimer > 0)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0 && messageText != null)
                {
                    messageText.text = "";
                }
            }
        }

        private void OnHealthChanged(int newHealth)
        {
            currentHealth = newHealth;
            UpdateHealthUI();
        }

        private void OnGoldChanged(int newGold)
        {
            gold = newGold;
            UpdateGoldUI();
        }

        private void OnEncounterResolved(Game.Systems.Encounter.EncounterResult result)
        {
            if (!string.IsNullOrEmpty(result.Message))
            {
                ShowMessage(result.Message);
            }
        }

        private void UpdateHealthUI()
        {
            if (healthText != null)
            {
                healthText.text = $"HP: {currentHealth}/{maxHealth}";
            }

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
        }

        private void UpdateGoldUI()
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {gold}";
            }
        }

        public void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageTimer = messageDuration;
            }
        }

        public void SetMaxHealth(int max)
        {
            maxHealth = max;
            UpdateHealthUI();
        }
    }
}
