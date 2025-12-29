namespace Game.Systems.FogOfWar
{
    using UnityEngine;
    using UnityEngine.Tilemaps;
    using System.Collections.Generic;

    /// <summary>
    /// Fog-of-War 렌더러 (Tilemap 기반)
    /// </summary>
    public class FogRenderer : MonoBehaviour
    {
        [Header("Tilemaps")]
        [Tooltip("Fog 타일맵 (Overlay 레이어)")]
        [SerializeField] private Tilemap fogTilemap;

        [Header("Tiles")]
        [Tooltip("미탐색 타일 (검은색, 알파 0.9)")]
        [SerializeField] private TileBase unexploredTile;

        [Tooltip("탐색 완료 타일 (회색, 알파 0.5)")]
        [SerializeField] private TileBase exploredTile;

        [Header("Debug")]
        [SerializeField] private bool logRenders = false;

        private HashSet<Vector2Int> renderedCells = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> lastVisibleCells = new HashSet<Vector2Int>();
        private bool isInitialized = false;

        /// <summary>
        /// 초기화 (FogOfWarSystem에서 호출)
        /// </summary>
        public void Initialize(FogOfWarGrid grid)
        {
            if (isInitialized) return;

            if (fogTilemap == null)
            {
                Debug.LogError("[FogRenderer] FogTilemap is not assigned!");
                return;
            }

            if (unexploredTile == null || exploredTile == null)
            {
                Debug.LogWarning("[FogRenderer] Fog tiles are not assigned!");
            }

            InitializeFog(grid);
            isInitialized = true;

            Debug.Log("[FogRenderer] Initialized");
        }

        /// <summary>
        /// Fog 초기화 (전체 영역을 미탐색으로 설정)
        /// </summary>
        private void InitializeFog(FogOfWarGrid grid)
        {
            if (unexploredTile == null) return;

            // 그리드 전체를 미탐색 타일로 채우기
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    Vector3Int tilePos = new Vector3Int(
                        cell.x + grid.Offset.x,
                        cell.y + grid.Offset.y,
                        0
                    );

                    fogTilemap.SetTile(tilePos, unexploredTile);
                    renderedCells.Add(cell);
                }
            }

            if (logRenders)
            {
                Debug.Log($"[FogRenderer] Initialized {renderedCells.Count} fog tiles");
            }
        }

        /// <summary>
        /// Fog 렌더링 (변경된 셀만 업데이트)
        /// </summary>
        public void Render(FogOfWarGrid grid)
        {
            if (!isInitialized || fogTilemap == null) return;

            int updatedCount = 0;

            // 모든 렌더링된 셀 검사 (최적화: 변경된 셀만)
            foreach (var cell in renderedCells)
            {
                Vector3Int tilePos = new Vector3Int(
                    cell.x + grid.Offset.x,
                    cell.y + grid.Offset.y,
                    0
                );

                if (grid.IsVisible(cell))
                {
                    // 현재 시야: Fog 제거 (FOV 메쉬가 밝게 표시)
                    if (!lastVisibleCells.Contains(cell))
                    {
                        fogTilemap.SetTile(tilePos, null);
                        updatedCount++;
                    }
                    lastVisibleCells.Add(cell);
                }
                else if (grid.IsVisited(cell))
                {
                    // 탐색 완료: 반투명 타일
                    if (lastVisibleCells.Contains(cell))
                    {
                        fogTilemap.SetTile(tilePos, exploredTile);
                        updatedCount++;
                        lastVisibleCells.Remove(cell);
                    }
                    else if (fogTilemap.GetTile(tilePos) == unexploredTile)
                    {
                        fogTilemap.SetTile(tilePos, exploredTile);
                        updatedCount++;
                    }
                }
                // 미탐색: 그대로 유지 (unexploredTile)
            }

            if (logRenders && updatedCount > 0)
            {
                Debug.Log($"[FogRenderer] Updated {updatedCount} tiles");
            }
        }

        /// <summary>
        /// Fog 초기화 (모든 타일 제거)
        /// </summary>
        public void Clear()
        {
            if (fogTilemap != null)
            {
                fogTilemap.ClearAllTiles();
            }

            renderedCells.Clear();
            lastVisibleCells.Clear();
            isInitialized = false;

            Debug.Log("[FogRenderer] Cleared");
        }

        /// <summary>
        /// 특정 셀 강제 공개 (디버그용)
        /// </summary>
        public void RevealCell(Vector2Int cell, Vector2Int gridOffset)
        {
            if (fogTilemap == null) return;

            Vector3Int tilePos = new Vector3Int(
                cell.x + gridOffset.x,
                cell.y + gridOffset.y,
                0
            );

            fogTilemap.SetTile(tilePos, null);
        }
    }
}
