using UnityEngine;

[CreateAssetMenu(fileName = "BlockConfig", menuName = "Items/BlockConfig")]
public class PickaxeConfig : ItemConfig
{
    public PickaxeType type;

    public float digModifier;
}

public enum PickaxeType
{
    Wood,
    Stone,
    Iron,
    Diamond
}