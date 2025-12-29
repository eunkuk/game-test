namespace Game.Systems.Vision
{
    using UnityEngine;
    using Game.Core.Events;

    /// <summary>
    /// 플레이어가 바라보는 방향 제공
    /// </summary>
    public class FacingProvider : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoUpdateFromMovement = true;

        [Header("Runtime")]
        [SerializeField] private Vector2 lastFacingDirection = Vector2.down;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = false;

        /// <summary>
        /// 방향 설정 (외부에서 호출 가능)
        /// </summary>
        public void SetFacingDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                lastFacingDirection = direction.normalized;
                GameEvents.TriggerPlayerFacingChanged(lastFacingDirection);
            }
        }

        /// <summary>
        /// 현재 방향 반환
        /// </summary>
        public Vector2 GetFacingDirection()
        {
            return lastFacingDirection;
        }

        /// <summary>
        /// 자동 업데이트 (PlayerController에서 호출 가능)
        /// </summary>
        public void UpdateFromMovement(Vector2 movementInput)
        {
            if (!autoUpdateFromMovement) return;
            SetFacingDirection(movementInput);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            Gizmos.color = Color.yellow;
            Vector3 origin = transform.position;
            Vector3 direction = new Vector3(lastFacingDirection.x, lastFacingDirection.y, 0);
            Gizmos.DrawRay(origin, direction * 2f);
            Gizmos.DrawSphere(origin + direction * 2f, 0.2f);
        }
    }
}
