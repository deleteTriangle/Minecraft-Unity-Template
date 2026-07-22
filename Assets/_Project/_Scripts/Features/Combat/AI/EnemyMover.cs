using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMover : MonoBehaviour, IMovable
{
    private NavMeshAgent _agent;

    public void Init(float speed)
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = speed;
    }

    public void MoveTo(Vector3 destination)
    {
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

        _agent.isStopped = true;
        _agent.ResetPath();
    }

    public bool IsReachedDestination()
    {
        return !_agent.pathPending
               && _agent.remainingDistance <= _agent.stoppingDistance;
    }
}