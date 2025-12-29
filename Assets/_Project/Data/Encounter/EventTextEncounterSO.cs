namespace Game.Data.Encounter
{
    using UnityEngine;

    /// <summary>
    /// 이벤트 선택지 데이터
    /// </summary>
    [System.Serializable]
    public class EventChoice
    {
        [Tooltip("선택지 텍스트 (버튼에 표시)")]
        public string choiceText;

        [TextArea(2, 3)]
        [Tooltip("선택 결과 텍스트")]
        public string resultText;

        [Tooltip("골드 보상 (음수 가능)")]
        public int goldReward;

        [Tooltip("체력 변화 (음수면 피해)")]
        public int healthChange;

        [Tooltip("아이템 보상 (옵션)")]
        public ScriptableObject itemReward;
    }

    [CreateAssetMenu(menuName = "Game/Encounter/EventText")]
    public class EventTextEncounterSO : EncounterDefinitionSO
    {
        [Header("Event Settings")]
        [TextArea(3, 5)]
        [Tooltip("이벤트 텍스트 (상황 설명)")]
        public string eventText;

        [Header("Choices")]
        [Tooltip("선택지 A")]
        public EventChoice choiceA;

        [Tooltip("선택지 B")]
        public EventChoice choiceB;

        public override string GetTypeName() => "Event";

        /// <summary>
        /// 선택지 인덱스로 EventChoice 반환
        /// </summary>
        public EventChoice GetChoice(int index)
        {
            return index == 0 ? choiceA : choiceB;
        }
    }
}
