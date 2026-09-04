using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRegistry : MonoBehaviour
{
    private Dictionary<BlockType, BlockConfig> blocks = new Dictionary<BlockType, BlockConfig>();
    
    public List<BlockConfig> blockConfigs;

    public void Init()
    {
        blocks.Clear();
        
        foreach (BlockConfig blockConfig in blockConfigs)
        {
            blocks[blockConfig.type] = blockConfig;
        }

        BlockConfig doorConfig = Resources.Load<BlockConfig>("Blocks/Door");
        if (doorConfig != null) blocks[doorConfig.type] = doorConfig;
    }

    public BlockConfig GetBlockConfig(BlockType type)
    {
        return blocks[type];
    }
}
