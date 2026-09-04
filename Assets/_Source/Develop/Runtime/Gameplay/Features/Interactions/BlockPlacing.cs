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
            
            byte state = GetPlacementState(block, hit.point, blockWorldPos);
            return TryPlace(blockWorldPos, block, state);
        }

        return false;
    }
    public bool TryPlace(Vector3Int pos, BlockType block)
    {
        return TryPlace(pos, block, 0);
    }

    private bool TryPlace(Vector3Int pos, BlockType block, byte state)
    {
        if (!G.World.TryGetBlock(pos, out BlockState targetBlock) || targetBlock.type != BlockType.Air) return false;

        if (block == BlockType.Door)
        {
            Vector3Int upperPos = pos + Vector3Int.up;
            if (!G.World.TryGetBlock(upperPos, out BlockState upperBlock) || upperBlock.type != BlockType.Air) return false;

            byte direction = GetPlayerDirection();
            G.World.TrySetBlock(pos, new BlockState(block, direction));
            G.World.TrySetBlock(upperPos, new BlockState(block, direction, BlockStateFlags.Upper));
        }
        else
        {
            G.World.TrySetBlock(pos, new BlockState(block, GetPlayerDirection(), state));
        }

        OnBlockPlaced?.Invoke(G.World.GetChunkCoordinatesContainingBlock(pos));
        return true;
    }

    private byte GetPlacementState(BlockType block, Vector3 hitPoint, Vector3Int blockPos)
    {
        if (block != BlockType.Trapdoor) return 0;

        return hitPoint.y - blockPos.y < 0.5f ? (byte)0 : BlockStateFlags.Upper;
    }
    
    byte GetPlayerDirection()
    {
        float angle = mainCamera.transform.eulerAngles.y;
        return (byte)((4 - Mathf.RoundToInt(angle / 90f) % 4) % 4);
    }
}
