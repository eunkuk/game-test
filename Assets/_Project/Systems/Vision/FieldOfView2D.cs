namespace Game.Systems.Vision
{
    using UnityEngine;
    using Game.Core.Events;

    /// <summary>
    /// 2D 전방 시야(FOV) 시스템 (레이캐스트 기반)
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class FieldOfView2D : MonoBehaviour
    {
        [Header("FOV Parameters")]
        [Tooltip("시야각 (도)")]
        [SerializeField] private float viewAngle = 90f;

        [Tooltip("시야 거리")]
        [SerializeField] private float viewDistance = 10f;

        [Tooltip("레이 개수 (해상도)")]
        [SerializeField] private int rayCount = 50;

        [Tooltip("업데이트 주기 (초)")]
        [SerializeField] private float updateRate = 0.1f;

        [Header("Occlusion")]
        [Tooltip("시야 차단 레이어 (Wall/VisionBlocker)")]
        [SerializeField] private LayerMask visionBlockerMask;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool drawRays = true;

        private MeshFilter meshFilter;
        private Mesh viewMesh;
        private FacingProvider facingProvider;

        private Vector2[] currentViewPoints;
        private float updateTimer;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            facingProvider = GetComponentInParent<FacingProvider>();

            if (facingProvider == null)
            {
                Debug.LogWarning("[FieldOfView2D] FacingProvider not found! Add it to parent GameObject.");
            }

            // 메쉬 초기화
            viewMesh = new Mesh { name = "View Mesh" };
            meshFilter.mesh = viewMesh;
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateRate)
            {
                updateTimer = 0;
                UpdateVision();
            }
        }

        /// <summary>
        /// 시야 업데이트 (레이캐스트 + 메쉬 생성)
        /// </summary>
        private void UpdateVision()
        {
            if (facingProvider == null) return;

            Vector2 origin = transform.position;
            Vector2 facingDir = facingProvider.GetFacingDirection();
            float facingAngle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;

            // 레이캐스트
            Vector3[] vertices = new Vector3[rayCount + 1];
            int[] triangles = new int[(rayCount - 1) * 3];

            vertices[0] = Vector3.zero; // 중심

            float angleStep = viewAngle / (rayCount - 1);
            float startAngle = facingAngle - viewAngle / 2f;

            currentViewPoints = new Vector2[rayCount];

            for (int i = 0; i < rayCount; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewDistance, visionBlockerMask);

                Vector2 point = hit.collider != null ? hit.point : origin + dir * viewDistance;
                currentViewPoints[i] = point;

                vertices[i + 1] = transform.InverseTransformPoint(point);

                if (i < rayCount - 1)
                {
                    int triIndex = i * 3;
                    triangles[triIndex] = 0;
                    triangles[triIndex + 1] = i + 1;
                    triangles[triIndex + 2] = i + 2;
                }
            }

            // 메쉬 업데이트
            viewMesh.Clear();
            viewMesh.vertices = vertices;
            viewMesh.triangles = triangles;
            viewMesh.RecalculateNormals();

            // 이벤트 발행 (Fog-of-War에서 사용)
            GameEvents.TriggerVisionUpdated(currentViewPoints);
        }

        /// <summary>
        /// 특정 월드 좌표가 시야에 보이는지 확인
        /// </summary>
        public bool IsVisible(Vector2 worldPos)
        {
            if (currentViewPoints == null) return false;

            Vector2 origin = transform.position;
            Vector2 toTarget = worldPos - origin;

            if (toTarget.magnitude > viewDistance) return false;

            // 시야각 내부인지 확인
            Vector2 facingDir = facingProvider.GetFacingDirection();
            float angle = Vector2.Angle(facingDir, toTarget);
            if (angle > viewAngle / 2f) return false;

            // 레이캐스트로 차단 여부 확인
            RaycastHit2D hit = Physics2D.Raycast(origin, toTarget.normalized, toTarget.magnitude, visionBlockerMask);
            return hit.collider == null;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || currentViewPoints == null) return;

            Vector2 origin = transform.position;

            // 레이 그리기
            if (drawRays)
            {
                Gizmos.color = Color.yellow;
                foreach (var point in currentViewPoints)
                {
                    Gizmos.DrawLine(origin, point);
                }
            }

            // 끝점 그리기
            Gizmos.color = Color.red;
            foreach (var point in currentViewPoints)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }

            // 시야 범위 (부채꼴 윤곽)
            Gizmos.color = Color.cyan;
            if (facingProvider != null)
            {
                Vector2 facingDir = facingProvider.GetFacingDirection();
                float facingAngle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;

                float leftAngle = (facingAngle - viewAngle / 2f) * Mathf.Deg2Rad;
                float rightAngle = (facingAngle + viewAngle / 2f) * Mathf.Deg2Rad;

                Vector2 leftDir = new Vector2(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle)) * viewDistance;
                Vector2 rightDir = new Vector2(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle)) * viewDistance;

                Gizmos.DrawLine(origin, origin + leftDir);
                Gizmos.DrawLine(origin, origin + rightDir);
            }
        }

        // 런타임에서 파라미터 변경 가능
        public void SetViewAngle(float angle) => viewAngle = angle;
        public void SetViewDistance(float distance) => viewDistance = distance;
        public void SetRayCount(int count) => rayCount = Mathf.Max(3, count);
        public void SetUpdateRate(float rate) => updateRate = Mathf.Max(0.01f, rate);
    }
}
