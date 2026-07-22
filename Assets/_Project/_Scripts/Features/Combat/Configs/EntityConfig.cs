using UnityEngine;

[CreateAssetMenu(fileName = "Entity", menuName = "Entity/EntityConfig")]
public class EntityConfig : ScriptableObject
{
    public int maxHealth = 100;
    public int attackDamage = 10;
    public float attackCooldown = 1f;
}