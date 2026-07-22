using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IDamageable
{
    public event Action OnDied;

    public EntityConfig config;
    public Health Health { get; private set; }
    public int AttackDamage { get; private set; }
    public float AttackCooldown { get; private set; }
    public bool IsAlive => Health.CurrentHealth > 0;

    public virtual void Init(EntityConfig config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config), $"EntityConfig is null on {name}");
        this.config = config;
        
        Health = new Health(config.maxHealth);
        AttackDamage = config.attackDamage;
        AttackCooldown = config.attackCooldown;

        Health.OnDeath += HandleDeath;
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        Health.TakeDamage(damage);
    }

    protected virtual void HandleDeath()
    {
        OnDied?.Invoke();
    }

    private void OnDestroy()
    {
        if (Health != null)
            Health.OnDeath -= HandleDeath;
    }
}