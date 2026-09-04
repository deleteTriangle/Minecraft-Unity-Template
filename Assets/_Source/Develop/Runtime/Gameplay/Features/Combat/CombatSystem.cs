using System.Collections;
using UnityEngine;

public class CombatSystem
{
    private MonoBehaviour _coroutineRunner;

    public CombatSystem(MonoBehaviour coroutineRunner)
    {
        _coroutineRunner = coroutineRunner;
    }

    public void PerformAttack(Entity attacker, IDamageable target)
    {
        if (!attacker.IsAlive) return;
        target.TakeDamage(attacker.AttackDamage);
    }

    public void StartAutoAttack(Entity attacker, IDamageable target)
    {
        _coroutineRunner.StartCoroutine(AutoAttackRoutine(attacker, target));
    }

    private IEnumerator AutoAttackRoutine(Entity attacker, IDamageable target)
    {
        while (attacker.IsAlive && target.Health.CurrentHealth > 0)
        {
            PerformAttack(attacker, target);
            yield return new WaitForSeconds(attacker.AttackCooldown);
        }
    }
}