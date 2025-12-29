namespace Game.Systems.Maze
{
    using System;
    using UnityEngine;

    [Serializable]
    public class MazeConfig
    {
        [Header("Size")]
        public int width = 41;
        public int height = 41;

        [Header("Algorithm")]
        [Range(0f, 1f)]
        public float deadEndRemovalRate = 0.3f;

        [Header("Seed")]
        public bool useFixedSeed = false;
        public int fixedSeed = 12345;

        [Header("Spawn Points")]
        public int minEnemySpawnPoints = 5;
        public int maxEnemySpawnPoints = 15;
        public int minEventPoints = 3;
        public int maxEventPoints = 8;

        public void Validate()
        {
            if (width % 2 == 0) width++;
            if (height % 2 == 0) height++;
            if (width < 11) width = 11;
            if (height < 11) height = 11;
        }
    }
}
