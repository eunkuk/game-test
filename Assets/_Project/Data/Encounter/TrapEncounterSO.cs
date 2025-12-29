namespace Game.Data.Encounter
{
    using UnityEngine;

    public enum TrapType
    {
        Damage,     // 즉시 피해
        Slow,       // 이동 속도 감소
        Poison      // 지속 피해
    }

    [CreateAssetMenu(menuName = "Game/Encounter/Trap")]
    public class TrapEncounterSO : EncounterDefinitionSO
    {
        [Header("Trap Settings")]
        [Tooltip("함정 타입")]
        public TrapType trapType;

        [Tooltip("피해량 (Damage 타입일 경우)")]
        public int damageAmount = 10;

        [Tooltip("지속 시간 (Slow/Poison일 경우, 초 단위)")]
        public float duration = 5f;

        [Tooltip("효과 강도 (Slow: 0~1, Poison: DPS)")]
        public float effectStrength = 0.5f;

        public override string GetTypeName() => "Trap";
    }
}
