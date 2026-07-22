using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class BlockDestroing : MonoBehaviour
{
    public event Action<Vector2> OnBlockDestroyed;
    
    private Camera mainCamera;

    private float distance;
    private LayerMask layer;

    private bool isFirstDigging = true;
    private float timer = 0f;
    
    private GameObject destructionCube;
    private List<MeshRenderer> destructionRenderers;
    private MaterialPropertyBlock propertyBlock;
    public Material destructionMaterial;
    private Vector3Int currentBlockPos;
    
    public void Init()
    {
        mainCamera = Camera.main;
        
        distance = G.Interaction.distance;
        layer = ~LayerMask.GetMask("Player", "Mob");
        
        propertyBlock = new MaterialPropertyBlock();
        destructionRenderers = new List<MeshRenderer>();
        
        CreateDestructionCube();
    }

    public void DigBlock()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, distance, layer))
        {
            Vector3 blockCenter = hit.point - hit.normal * 0.01f;
            Vector3Int blockWorldPos = Vector3Int.FloorToInt(blockCenter);
            BlockState block = GetBlockAtPos(blockWorldPos);
            BlockConfig blockConfig = G.BlockRegistry.GetBlockConfig(block.type);

            float timeToDestroy = blockConfig.timeToDestroy;

            float digModifier = 1;
            ItemConfig selectedItem = G.Hotbar.GetSelectedItem();
            if (selectedItem is PickaxeConfig pickaxe)
            {
                digModifier = pickaxe.digModifier;
            }

            if (currentBlockPos != blockWorldPos)
            {
                StopDigBlock();
                currentBlockPos = blockWorldPos;
                
                PositionDestructionCube(blockWorldPos);
            }
            
            if (isFirstDigging)
            {
                timer = timeToDestroy;
                isFirstDigging = false;
                
                destructionCube.SetActive(true);
            }

            if (timer > 0)
            {
                timer -= digModifier * Time.deltaTime;
                
                float progress = 1f - (timer / timeToDestroy);
                UpdateDestructionProgress(progress);
            }
            else
            {
                DestroyBlock(blockWorldPos);
                StopDigBlock();
            }
        }
        else
        {
            StopDigBlock();
        }
    }

    public void StopDigBlock()
    {
        if (destructionCube != null)
        {
            destructionCube.SetActive(false);
        }
        
        isFirstDigging = true;
    }
   
    private void CreateDestructionCube()
    {
        destructionCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        destructionCube.name = "DestructionCube";
        
        Destroy(destructionCube.GetComponent<Collider>());
        
        MeshRenderer[] renderers = destructionCube.GetComponentsInChildren<MeshRenderer>();
        
        destructionRenderers.Clear();
        
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material = destructionMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                
                destructionRenderers.Add(renderer);
            }
        }
        
        destructionCube.SetActive(false);
    }
    
    private void PositionDestructionCube(Vector3Int blockPos)
    {
        destructionCube.transform.position = blockPos + Vector3.one * 0.5f;
        destructionCube.transform.localScale = Vector3.one * 1.02f;
    }
    
    private void UpdateDestructionProgress(float progress)
    {
        if (destructionRenderers != null)
        {
            foreach (var renderer in destructionRenderers)
            {
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_Progress", progress);
                
                propertyBlock.SetFloat("_Scale", 1f + progress * 2f);
                
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
    
    public void DestroyBlock()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            
        if (Physics.Raycast(ray, out RaycastHit hit, distance, layer))
        {
            Vector3 blockCenter = hit.point - hit.normal * 0.01f;
            Vector3Int blockWorldPos = Vector3Int.FloorToInt(blockCenter);
                
            DestroyBlock(blockWorldPos);
        };
    }
    
    public void DestroyBlock(Vector3Int pos)
    {
        Vector2Int chunkPos = G.World.GetChunkCoordinatesContainingBlock(pos);

        if (G.World.chunks.TryGetValue(chunkPos, out ChunkData chunkData))
        {
            Vector3Int chunkOrigin = new Vector3Int(chunkPos.x, 0,  chunkPos.y) * G.World.terrainConfig.chunkWidth;
            Vector3Int localPos = pos - chunkOrigin;

            BlockState block = new BlockState(BlockType.Air);
            chunkData.Blocks[localPos.x, localPos.y, localPos.z] = block;
            G.World.MarkChunkAndBoundaryNeighborsDirty(chunkPos, localPos);
            
            OnBlockDestroyed?.Invoke(chunkPos);
        }
    }

    private BlockState GetBlockAtPos(Vector3Int pos)
    {
        Vector2Int chunkPos = G.World.GetChunkCoordinatesContainingBlock(pos);

        if (G.World.chunks.TryGetValue(chunkPos, out ChunkData chunkData))
        {
            Vector3Int chunkOrigin = new Vector3Int(chunkPos.x, 0, chunkPos.y) * G.World.terrainConfig.chunkWidth;
            Vector3Int localPos = pos - chunkOrigin;
            return chunkData.Blocks[localPos.x, localPos.y, localPos.z];
        }
        
        return null;
    }
}
