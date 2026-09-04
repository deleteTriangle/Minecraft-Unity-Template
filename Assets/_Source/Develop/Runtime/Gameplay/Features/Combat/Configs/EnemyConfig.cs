using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Entity/Enemy")]
public class EnemyConfig : EntityConfig
{
    public EnemyType type;
    public Enemy prefab;
    
    public float moveSpeed = 3.5f;
    public float attackRange = 1.5f;
    public float chaseRange = 15f;
    public float updateTargetRate = 0.2f;
}

public enum EnemyType : byte
{
    Zombie,
    Skeleton
}