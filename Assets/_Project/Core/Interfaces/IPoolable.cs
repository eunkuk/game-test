namespace Game.Core.Interfaces
{
    /// <summary>
    /// Object Pool에서 관리 가능한 오브젝트 인터페이스
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Pool에서 꺼내질 때 호출
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Pool로 반환될 때 호출
        /// </summary>
        void OnDespawn();
    }
}
