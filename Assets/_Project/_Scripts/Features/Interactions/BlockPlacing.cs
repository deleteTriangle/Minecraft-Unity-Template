using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacing : MonoBehaviour
{
    public event Action<Vector2> OnBlockPlaced;
    
    private Camera mainCamera;
    
    private float distance;
    private LayerMask layer;
    
    public void Init()
    {
        mainCamera = Camera.main;
        
        distance = G.Interaction.distance;
        layer = ~LayerMask.GetMask("Player");
    }
    
    public bool TryPlace(BlockType block)
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, distance, layer))
        {
            Vector3 blockCenter = hit.point + hit.normal * 0.5f;
            Vector3Int blockWorldPos = Vector3Int.FloorToInt(blockCenter);
            
            return TryPlace(blockWorldPos, block);
        }

        return false;
    }
    public bool TryPlace(Vector3Int pos, BlockType block)
    {
        Vector2Int chunkPos = G.World.GetChunkCoordinatesContainingBlock(pos);

        if (G.World.chunks.TryGetValue(chunkPos, out ChunkData chunkData))
        {
            Vector3Int chunkOrigin = new Vector3Int(chunkPos.x, 0,  chunkPos.y) * G.World.terrainConfig.chunkWidth;
            Vector3Int localPos = pos - chunkOrigin;

            if (chunkData.Blocks[localPos.x, localPos.y, localPos.z].type == BlockType.Air)
            {
                chunkData.Blocks[localPos.x, localPos.y, localPos.z] = new BlockState(block, GetPlayerDirection());
                G.World.MarkChunkAndBoundaryNeighborsDirty(chunkPos, localPos);

                OnBlockPlaced?.Invoke(chunkPos);
                
                return true;
            }
        }
        
        return false;
    }
    
    byte GetPlayerDirection()
    {
        float angle = mainCamera.transform.eulerAngles.y;
        return (byte)(Mathf.RoundToInt(angle / 90f) % 4);
    }
}
