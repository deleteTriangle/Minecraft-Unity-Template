public class Skeleton : Enemy
{
    public override void Init(EntityConfig config)
    {
        base.Init(config);
    }

    public override void Attack(IDamageable target)
    {
        target.TakeDamage(AttackDamage * 2);
    }
}
