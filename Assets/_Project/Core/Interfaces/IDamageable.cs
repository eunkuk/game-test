namespace Game.Core.Interfaces
{
    /// <summary>
    /// 피해를 받을 수 있는 엔티티를 위한 인터페이스
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
        void Heal(int amount);
        bool IsDead();
    }
}
