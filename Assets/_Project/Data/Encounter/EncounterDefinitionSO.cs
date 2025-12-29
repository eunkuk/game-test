namespace Game.Data.Encounter
{
    using UnityEngine;

    /// <summary>
    /// 모든 Encounter의 추상 베이스 클래스
    /// </summary>
    public abstract class EncounterDefinitionSO : ScriptableObject
    {
        [Header("Base Info")]
        [TextArea(2, 4)]
        [Tooltip("Encounter 설명 (디자이너용)")]
        public string description;

        /// <summary>
        /// Encounter 타입 이름 반환 (디버그용)
        /// </summary>
        public abstract string GetTypeName();
    }
}
