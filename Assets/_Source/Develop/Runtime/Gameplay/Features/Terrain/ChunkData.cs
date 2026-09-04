using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    public ChunkRenderer chunkRenderer;
    
    private BlockState[,,] blocks;
    public BlockState[,,] Blocks
    {
        get => blocks;
        set => blocks = value;
    }

    public Vector2Int pos;
}
