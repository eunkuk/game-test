namespace Game.Gameplay.Player
{
    using UnityEngine;
    using Game.Systems.Vision;

    /// <summary>
    /// 플레이어 이동 컨트롤러 (2D 탑다운)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("References")]
        [SerializeField] private FacingProvider facingProvider;

        private Rigidbody2D rb;
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // 2D 탑다운
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (facingProvider == null)
            {
                facingProvider = GetComponent<FacingProvider>();
            }
        }

        private void Update()
        {
            // 입력 받기
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;

            // 방향 업데이트 (FacingProvider)
            if (facingProvider != null && moveInput.sqrMagnitude > 0.01f)
            {
                facingProvider.SetFacingDirection(moveInput);
            }
        }

        private void FixedUpdate()
        {
            // 이동
            rb.velocity = moveInput * moveSpeed;
        }

        public void SetMoveSpeed(float speed) => moveSpeed = speed;
        public float GetMoveSpeed() => moveSpeed;
    }
}
