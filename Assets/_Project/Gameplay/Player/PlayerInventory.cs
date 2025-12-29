namespace Game.Gameplay.Player
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// 플레이어 인벤토리 (Init용 간단 구현)
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Items")]
        [SerializeField] private List<ScriptableObject> items = new List<ScriptableObject>();

        [Header("Debug")]
        [SerializeField] private bool logChanges = true;

        /// <summary>
        /// 아이템 추가
        /// </summary>
        public void AddItem(ScriptableObject item)
        {
            if (item == null) return;

            items.Add(item);

            if (logChanges)
            {
                Debug.Log($"[PlayerInventory] Added item: {item.name}");
            }
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        public bool RemoveItem(ScriptableObject item)
        {
            bool removed = items.Remove(item);

            if (removed && logChanges)
            {
                Debug.Log($"[PlayerInventory] Removed item: {item.name}");
            }

            return removed;
        }

        /// <summary>
        /// 아이템 보유 여부
        /// </summary>
        public bool HasItem(ScriptableObject item)
        {
            return items.Contains(item);
        }

        public List<ScriptableObject> GetItems() => new List<ScriptableObject>(items);
        public int GetItemCount() => items.Count;
    }
}
