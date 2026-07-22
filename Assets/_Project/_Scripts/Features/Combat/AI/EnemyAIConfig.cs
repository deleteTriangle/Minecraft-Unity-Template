using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAIConfig", menuName = "Combat/EnemyAIConfig")]
public class EnemyAIConfig : ScriptableObject
{
    public float moveSpeed = 3.5f;
    public float attackRange = 1.5f;
    public float chaseRange = 15f;
    public float updateTargetRate = 0.2f;
}