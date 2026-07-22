using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Knockback : MonoBehaviour
{
    private NavMeshAgent _agent;
    private CharacterController _characterController;

    public void Init()
    {
        _agent = GetComponent<NavMeshAgent>();
        _characterController = GetComponent<CharacterController>();
    }

    public void Apply(Vector3 attackerPosition, float knockbackForce)
    {
        Vector3 direction = (transform.position - attackerPosition).normalized;
        StartCoroutine(KnockbackRoutine(direction, knockbackForce));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float knockbackForce)
    {
        Vector3 targetPos = transform.position + direction * knockbackForce;
        float elapsed = 0f;
        float duration = 0.15f;
        Vector3 startPos = transform.position;

        if (_agent != null) _agent.enabled = false;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);

            if (_characterController != null)
                _characterController.Move(newPos - transform.position);
            else
                transform.position = newPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_agent != null) _agent.enabled = true;
    }
}