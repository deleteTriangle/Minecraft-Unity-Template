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
            blocks.Add(blockConfig.type, blockConfig);
        }
    }

    public BlockConfig GetBlockConfig(BlockType type)
    {
        return blocks[type];
    }
}
