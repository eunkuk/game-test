namespace Game.UI.Encounter
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using Game.Core.Events;
    using Game.Data.Encounter;
    using Game.Systems.Encounter;

    /// <summary>
    /// 이벤트 선택지 UI 패널
    /// </summary>
    public class EventChoicePanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI eventText;
        [SerializeField] private Button choiceAButton;
        [SerializeField] private Button choiceBButton;
        [SerializeField] private TextMeshProUGUI choiceAText;
        [SerializeField] private TextMeshProUGUI choiceBText;

        [Header("References")]
        [SerializeField] private Game.Systems.Encounter.EncounterResolver resolver;

        private EventTextEncounterSO currentEvent;
        private EncounterContext currentContext;
        private EncounterResult currentResult;

        private void Awake()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            if (choiceAButton != null)
            {
                choiceAButton.onClick.AddListener(() => OnChoiceSelected(0));
            }

            if (choiceBButton != null)
            {
                choiceBButton.onClick.AddListener(() => OnChoiceSelected(1));
            }
        }

        private void OnEnable()
        {
            GameEvents.OnEventEncounterTriggered += OnEventEncounterTriggered;
        }

        private void OnDisable()
        {
            GameEvents.OnEventEncounterTriggered -= OnEventEncounterTriggered;
        }

        /// <summary>
        /// 이벤트 Encounter 트리거 수신
        /// </summary>
        private void OnEventEncounterTriggered(EventTextEncounterSO eventData, EncounterContext context, EncounterResult result)
        {
            currentEvent = eventData;
            currentContext = context;
            currentResult = result;

            ShowPanel(eventData);
        }

        /// <summary>
        /// 패널 표시
        /// </summary>
        private void ShowPanel(EventTextEncounterSO eventData)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (eventText != null)
            {
                eventText.text = eventData.eventText;
            }

            if (choiceAText != null)
            {
                choiceAText.text = eventData.choiceA.choiceText;
            }

            if (choiceBText != null)
            {
                choiceBText.text = eventData.choiceB.choiceText;
            }

            // 시간 정지 (옵션)
            // Time.timeScale = 0;
        }

        /// <summary>
        /// 선택지 선택
        /// </summary>
        private void OnChoiceSelected(int choiceIndex)
        {
            if (currentEvent == null)
            {
                Debug.LogWarning("[EventChoicePanel] No event data!");
                return;
            }

            EventChoice choice = currentEvent.GetChoice(choiceIndex);

            // Resolver를 통해 결과 적용
            if (resolver != null)
            {
                resolver.ApplyEventChoice(choice, currentResult);
            }

            // 결과 메시지 표시 (간단한 방법)
            Debug.Log($"[EventChoicePanel] Selected: {choice.choiceText} → {choice.resultText}");

            HidePanel();
        }

        /// <summary>
        /// 패널 숨기기
        /// </summary>
        private void HidePanel()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            // 시간 재개
            // Time.timeScale = 1;

            currentEvent = null;
            currentContext = null;
            currentResult = null;
        }
    }
}
