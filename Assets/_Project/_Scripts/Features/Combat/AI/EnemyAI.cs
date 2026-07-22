using System;
using UnityEngine;

public class EnemyAI : ITickable
{
    private readonly EnemyMover _mover;
    private readonly EnemyConfig _config;
    private readonly Func<Vector3> _getEnemyPosition;
    private readonly Action<IDamageable> _attackAction;

    private Transform _target;
    private IDamageable _targetDamageable;
    private float _timeSinceLastUpdate;
    private float _attackCooldownRemaining;
    private bool _isActive;

    public EnemyAI(
        EnemyMover mover,
        EnemyConfig config,
        Func<Vector3> getEnemyPosition,
        Action<IDamageable> attackAction)
    {
        _mover = mover;
        _config = config;
        _getEnemyPosition = getEnemyPosition;
        _attackAction = attackAction;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        _targetDamageable = target != null && target.TryGetComponent<IDamageable>(out var damageable)
            ? damageable
            : null;
    }

    public void Activate()
    {
        _isActive = true;
        _attackCooldownRemaining = 0f;
        _mover.Stop();
    }

    public void Deactivate()
    {
        _isActive = false;
        _mover.Stop();
    }

    public void Tick(float deltaTime)
    {
        if (!_isActive || _target == null) return;
        
        if (_attackCooldownRemaining > 0f)
            _attackCooldownRemaining -= deltaTime;

        _timeSinceLastUpdate += deltaTime;
        if (_timeSinceLastUpdate < _config.updateTargetRate) return;
        _timeSinceLastUpdate = 0f;

        float sqrDistanceToTarget = (_getEnemyPosition() - _target.position).sqrMagnitude;
        float sqrChaseRange = _config.chaseRange * _config.chaseRange;
        float sqrAttackRange = _config.attackRange * _config.attackRange;

        if (sqrDistanceToTarget > sqrChaseRange)
        {
            _mover.Stop();
            return;
        }

        if (sqrDistanceToTarget <= sqrAttackRange)
        {
            _mover.Stop();

            if (_attackCooldownRemaining <= 0f && _targetDamageable != null)
            {
                _attackAction?.Invoke(_targetDamageable);
                _attackCooldownRemaining = _config.attackCooldown;
            }

            return;
        }

        _mover.MoveTo(_target.position);
    }
}
