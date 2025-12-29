namespace Game.Systems.Encounter
{
    using UnityEngine;
    using Game.Data.Encounter;
    using Game.Systems.Dungeon;
    using Game.Core.Events;

    /// <summary>
    /// 방 진입 시 Encounter 트리거 (1회만 발동)
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomTrigger2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EncounterTableSO encounterTable;
        [SerializeField] private EncounterResolver resolver;

        [Header("Runtime")]
        [SerializeField] private Room room;
        [SerializeField] private int dungeonSeed;
        [SerializeField] private bool hasTriggered = false;

        [Header("Debug")]
        [SerializeField] private bool logTriggers = true;

        private BoxCollider2D boxCollider;

        /// <summary>
        /// RoomTrigger 초기화
        /// </summary>
        public void Initialize(Room room, EncounterTableSO table, EncounterResolver resolver, int seed)
        {
            this.room = room;
            this.encounterTable = table;
            this.resolver = resolver;
            this.dungeonSeed = seed;

            // Collider 설정
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(room.Bounds.width, room.Bounds.height);
            transform.position = new Vector3(room.Center.x, room.Center.y, 0);

            hasTriggered = false;

            if (logTriggers)
            {
                Debug.Log($"[RoomTrigger2D] Initialized for {room}");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered) return;
            if (!other.CompareTag("Player")) return;

            TriggerEncounter(other.gameObject);
        }

        /// <summary>
        /// Encounter 트리거 실행
        /// </summary>
        private void TriggerEncounter(GameObject player)
        {
            hasTriggered = true;
            room.IsVisited = true;

            if (logTriggers)
            {
                Debug.Log($"[RoomTrigger2D] Player entered {room}");
            }

            // 방 진입 이벤트 발행
            GameEvents.TriggerEnterRoom(room);

            // 특수 방은 Encounter 스킵
            if (room.IsStartRoom)
            {
                if (logTriggers)
                {
                    Debug.Log($"[RoomTrigger2D] Start room, skipping encounter");
                }
                return;
            }

            // Encounter 롤
            if (encounterTable == null)
            {
                Debug.LogWarning($"[RoomTrigger2D] No encounter table assigned for Room {room.Id}");
                return;
            }

            System.Random random = new System.Random(dungeonSeed + room.Id);
            EncounterDefinitionSO encounter = encounterTable.Roll(random);

            if (encounter == null)
            {
                if (logTriggers)
                {
                    Debug.Log($"[RoomTrigger2D] No encounter rolled for Room {room.Id}");
                }
                return;
            }

            // Encounter 실행
            if (resolver == null)
            {
                Debug.LogError($"[RoomTrigger2D] EncounterResolver is not assigned!");
                return;
            }

            EncounterContext context = new EncounterContext(dungeonSeed, room, player);
            resolver.Resolve(encounter, context);
        }

        /// <summary>
        /// 트리거 리셋 (디버그용)
        /// </summary>
        public void ResetTrigger()
        {
            hasTriggered = false;
            room.IsVisited = false;
        }

        private void OnDrawGizmos()
        {
            if (room == null) return;

            Gizmos.color = hasTriggered ? Color.gray : Color.cyan;
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
            Gizmos.DrawCube(transform.position, new Vector3(room.Bounds.width, room.Bounds.height, 0.1f));
        }
    }
}
