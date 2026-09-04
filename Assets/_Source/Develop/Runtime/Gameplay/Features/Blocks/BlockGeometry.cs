using UnityEngine;

public static class BlockGeometry
{
    public static BlockConfig GetStateConfig(BlockState blockState)
    {
        BlockConfig blockConfig = G.BlockRegistry.GetBlockConfig(blockState.type);
        byte visualState = blockState.type is BlockType.Trapdoor or BlockType.Door
            ? (byte)(blockState.state & BlockStateFlags.Open)
            : blockState.state;

        BlockConfig stateConfig = blockConfig.GetStateConfig(visualState);
        if (blockState.type == BlockType.Door && BlockStateFlags.IsUpper(blockState))
        {
            return stateConfig.WithBaseId(stateConfig.baseId + 1);
        }

        if (blockState.type != BlockType.Trapdoor || !BlockStateFlags.IsUpper(blockState) || BlockStateFlags.IsOpen(blockState)) return stateConfig;

        return stateConfig.WithOffsetAndSize(
            new Vector3(stateConfig.offset.x, 1f - stateConfig.size.y, stateConfig.offset.z),
            stateConfig.size);
    }

    public static (Vector3 offset, Vector3 size) RotateBounds(Vector3 offset, Vector3 size, byte direction)
    {
        return direction switch
        {
            0 => (offset, size),
            1 => (new Vector3(1f - offset.z - size.z, offset.y, offset.x), new Vector3(size.z, size.y, size.x)),
            2 => (new Vector3(1f - offset.x - size.x, offset.y, 1f - offset.z - size.z), size),
            3 => (new Vector3(offset.z, offset.y, 1f - offset.x - size.x), new Vector3(size.z, size.y, size.x)),
            _ => (offset, size)
        };
    }
}
