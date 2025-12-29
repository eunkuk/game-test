namespace Game.Data.Item
{
    using UnityEngine;

    public enum ItemType
    {
        Consumable,     // 소모품
        Equipment,      // 장비
        KeyItem         // 중요 아이템
    }

    [CreateAssetMenu(menuName = "Game/Item/ItemData")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("아이템 이름")]
        public string itemName = "Item";

        [Tooltip("아이템 타입")]
        public ItemType itemType;

        [TextArea(2, 4)]
        [Tooltip("아이템 설명")]
        public string description;

        [Tooltip("아이템 아이콘")]
        public Sprite icon;

        [Header("Effects")]
        [Tooltip("체력 회복량")]
        public int healthRestore = 0;

        [Tooltip("공격력 증가 (장비)")]
        public int attackBonus = 0;

        [Tooltip("방어력 증가 (장비)")]
        public int defenseBonus = 0;

        [Tooltip("이동 속도 배율 (장비)")]
        public float speedMultiplier = 1f;

        [Header("Stack")]
        [Tooltip("스택 가능 여부")]
        public bool stackable = true;

        [Tooltip("최대 스택 개수")]
        public int maxStack = 99;
    }
}
