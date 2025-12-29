namespace Game.Core.Random
{
    using System;

    /// <summary>
    /// Seed 기반 랜덤 생성기 (재현 가능)
    /// </summary>
    public class SeededRandom
    {
        private System.Random random;
        public int Seed { get; private set; }

        public SeededRandom(int seed)
        {
            Seed = seed;
            random = new System.Random(seed);
        }

        public int Next() => random.Next();
        public int Next(int maxValue) => random.Next(maxValue);
        public int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);
        public double NextDouble() => random.NextDouble();
        public float NextFloat() => (float)random.NextDouble();
        public float Range(float min, float max) => min + (float)random.NextDouble() * (max - min);

        /// <summary>
        /// 확률 체크 (0~1 범위)
        /// </summary>
        public bool Chance(float probability) => NextFloat() < probability;

        /// <summary>
        /// Seed 재설정
        /// </summary>
        public void Reseed(int newSeed)
        {
            Seed = newSeed;
            random = new System.Random(newSeed);
        }
    }
}
