public interface IDamageable
{
    Health Health { get; }
    void TakeDamage(int damage);
}