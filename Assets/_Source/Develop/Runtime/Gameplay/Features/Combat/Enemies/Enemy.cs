using UnityEngine;

[RequireComponent(typeof(Knockback), typeof(Flash))]
public abstract class Enemy : Entity, ITickable
{
    public EnemyType EnemyType { get; private set; }
    
    public Knockback Knockback { get; private set; }
    public Flash Flash { get; private set; }

    private EnemyAI _ai;

    public override void Init(EntityConfig config)
    {
        base.Init(config);
        var enemyConfig = config as EnemyConfig;
        if (enemyConfig == null) throw new System.ArgumentException("EnemyConfig is required", nameof(config));
        EnemyType = enemyConfig.type;
        OnDied += OnEnemyDied;
        
        Knockback = GetComponent<Knockback>();
        Knockback.Init();

        Flash = GetComponent<Flash>();
        Flash.Init();

        var mover = GetComponent<EnemyMover>();
        mover.Init(enemyConfig.moveSpeed);

        _ai = new EnemyAI(
            mover,
            enemyConfig,
            () => transform.position,
            Attack
        );
        
        gameObject.layer = LayerMask.NameToLayer("Mob");
    }

    public void SetTarget(Transform target) => _ai.SetTarget(target);
    public void Activate() => _ai.Activate();
    public void Deactivate() => _ai.Deactivate();

    public void Tick(float deltaTime) => _ai.Tick(deltaTime);

    public abstract void Attack(IDamageable target);

    protected virtual void OnEnemyDied()
    {
        Deactivate();
        G.GameLoop.Unregister(this);
        Destroy(gameObject);
    }
}
