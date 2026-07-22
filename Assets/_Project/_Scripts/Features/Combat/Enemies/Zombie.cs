using UnityEngine;

public class Zombie : Enemy
{
    public void Init(EnemyConfig config)
    {
        base.Init(config);
    }

    public override void Attack(IDamageable target)
    {
        target.TakeDamage(AttackDamage);
        Debug.Log($"Zombie attacks for {AttackDamage} damage");
    }
}