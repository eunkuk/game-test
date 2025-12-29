namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using Game.Core.Events;

    /// <summary>
    /// Fog-of-War 시스템 메인 (이벤트 수신 + 렌더링 관리)
    /// </summary>
    public class FogOfWarSystem : MonoBehaviour
    {
        [Header("Grid Settings")]
        [Tooltip("그리드 폭")]
        [SerializeField] private int gridWidth = 100;

        [Tooltip("그리드 높이")]
        [SerializeField] private int gridHeight = 100;

        [Tooltip("그리드 오프셋 (월드 좌표 기준)")]
        [SerializeField] private Vector2Int gridOffset = new Vector2Int(-50, -50);

        [Header("References")]
        [SerializeField] private FogRenderer fogRenderer;

        [Header("Runtime")]
        [SerializeField] private bool isInitialized = false;

        [Header("Debug")]
        [SerializeField] private bool logUpdates = false;

        private FogOfWarGrid grid;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            GameEvents.OnVisionUpdated += OnVisionUpdated;
        }

        private void OnDisable()
        {
            GameEvents.OnVisionUpdated -= OnVisionUpdated;
        }

        /// <summary>
        /// 시스템 초기화
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            grid = new FogOfWarGrid(gridWidth, gridHeight, gridOffset);

            if (fogRenderer == null)
            {
                Debug.LogWarning("[FogOfWarSystem] FogRenderer is not assigned!");
            }
            else
            {
                fogRenderer.Initialize(grid);
            }

            isInitialized = true;
            Debug.Log($"[FogOfWarSystem] Initialized: {grid}");
        }

        /// <summary>
        /// FOV 업데이트 이벤트 수신
        /// </summary>
        private void OnVisionUpdated(Vector2[] viewPoints)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[FogOfWarSystem] Not initialized!");
                return;
            }

            grid.UpdateVisibility(viewPoints);

            if (fogRenderer != null)
            {
                fogRenderer.Render(grid);
            }

            if (logUpdates)
            {
                Debug.Log($"[FogOfWarSystem] Vision updated: {grid}");
            }
        }

        /// <summary>
        /// Fog 리셋 (디버그/재시작용)
        /// </summary>
        public void ResetFog()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[FogOfWarSystem] Not initialized!");
                return;
            }

            grid.Reset();

            if (fogRenderer != null)
            {
                fogRenderer.Clear();
            }

            Debug.Log("[FogOfWarSystem] Fog reset");
        }

        /// <summary>
        /// 특정 영역 공개 (디버그/치트용)
        /// </summary>
        public void RevealArea(Vector2 worldPos, int radius)
        {
            if (!isInitialized) return;

            Vector2Int cell = grid.WorldToCell(worldPos);
            grid.RevealArea(cell, radius);

            if (fogRenderer != null)
            {
                fogRenderer.Render(grid);
            }

            Debug.Log($"[FogOfWarSystem] Revealed area at {worldPos} (radius: {radius})");
        }

        /// <summary>
        /// 탐색 진행률 반환
        /// </summary>
        public float GetExplorationProgress()
        {
            return isInitialized ? grid.GetExplorationProgress() : 0f;
        }

        public FogOfWarGrid GetGrid() => grid;

        private void OnDrawGizmos()
        {
            if (!isInitialized || grid == null) return;

            // 그리드 범위 표시
            Gizmos.color = Color.magenta;
            Vector3 center = new Vector3(
                gridOffset.x + gridWidth / 2f,
                gridOffset.y + gridHeight / 2f,
                0
            );
            Vector3 size = new Vector3(gridWidth, gridHeight, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
