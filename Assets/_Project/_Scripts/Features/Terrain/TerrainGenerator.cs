using System.Collections;
using System.Collections.Generic;
using _Project._Scripts.Features.Terrain;
using UnityEngine;

public static class TerrainGenerator
{
    public static BlockState[,,] GenerateTerrain(float xOffset, float zOffset, TerrainConfig config)
    {
        var result = new BlockState[config.chunkWidth, config.chunkHeight, config.chunkWidth];

        var grassBlock = new BlockState(BlockType.Grass);
        var airBlock = new BlockState(BlockType.Air);
        
        for (int x = 0; x < config.chunkWidth; x++)
        {
            for (int z = 0; z < config.chunkWidth; z++)
            {
                float height = Mathf.PerlinNoise((x + xOffset) * .2f, (z + zOffset) * .2f) * 5 + 10;

                for (int y = 0; y < config.chunkHeight; y++)
                {
                    result[x, y, z] = y < height ? grassBlock : airBlock;
                }
            }
        }

        return result;
    }
}
