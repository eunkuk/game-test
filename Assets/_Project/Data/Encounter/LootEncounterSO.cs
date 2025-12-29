namespace Game.Data.Encounter
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/Encounter/Loot")]
    public class LootEncounterSO : EncounterDefinitionSO
    {
        [Header("Loot Settings")]
        [Tooltip("골드 획득량")]
        public int goldAmount = 50;

        [Tooltip("아이템 보상 (옵션)")]
        public ScriptableObject itemReward; // ItemDataSO 참조 (순환 의존 방지)

        [Tooltip("체력 회복량 (옵션)")]
        public int healthRestore = 0;

        public override string GetTypeName() => "Loot";
    }
}
