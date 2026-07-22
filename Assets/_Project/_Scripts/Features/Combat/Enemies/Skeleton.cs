public class Skeleton : Enemy
{
    public void Init(EntityConfig config)
    {
        base.Init(config);
    }

    public override void Attack(IDamageable target)
    {
        target.TakeDamage(AttackDamage * 2);
    }
}