namespace Game.Data.Dungeon
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    [CreateAssetMenu(menuName = "Game/Dungeon/TilePalette")]
    public class TilePaletteSO : ScriptableObject
    {
        [Header("Floor Tiles")]
        [Tooltip("바닥 타일 배열 (랜덤 선택)")]
        public TileBase[] floorTiles;

        [Header("Wall Tiles")]
        [Tooltip("기본 벽 타일")]
        public TileBase wallTile;

        [Tooltip("상단 벽 타일 (옵션)")]
        public TileBase wallTopTile;

        [Tooltip("코너 벽 타일 (옵션)")]
        public TileBase wallCornerTile;

        /// <summary>
        /// 랜덤 바닥 타일 반환
        /// </summary>
        public TileBase GetRandomFloorTile(System.Random random)
        {
            if (floorTiles == null || floorTiles.Length == 0)
            {
                Debug.LogWarning("No floor tiles assigned!");
                return null;
            }
            return floorTiles[random.Next(floorTiles.Length)];
        }

        /// <summary>
        /// 기본 바닥 타일 반환
        /// </summary>
        public TileBase GetDefaultFloorTile()
        {
            return floorTiles != null && floorTiles.Length > 0 ? floorTiles[0] : null;
        }
    }
}
