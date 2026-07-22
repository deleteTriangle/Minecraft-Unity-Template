using UnityEngine;

[CreateAssetMenu(fileName = "BlockConfig", menuName = "Items/BlockConfig")]
public class BlockConfig : ItemConfig
{
    public BlockType type;
    public int baseId;

    public float timeToDestroy = 1f;
    
    public BlockConfig[] states;
    
    public bool isComplex;
    public int[] faceOffsets = new int[6]; // Top, Bottom, Front, Back, Left, Right
    
    public Vector3 size = Vector3.one;
    public Vector3 offset =  Vector3.zero;

    public BlockConfig GetStateConfig(byte state)
    {
        if (states == null || states.Length == 0) return this;
        if (state >= states.Length) return this;
        return states[state];
    }
    
    public BlockConfig WithOffsetAndSize(Vector3 newOffset, Vector3 newSize)
    {
        var copy = (BlockConfig)MemberwiseClone();
        copy.offset = newOffset;
        copy.size = newSize;
        return copy;
    }
}

public enum BlockFaceOffset
{
    Top    = 0,
    Bottom = 1,
    Front  = 2,
    Back   = 3,
    Left   = 4,
    Right  = 5,
}