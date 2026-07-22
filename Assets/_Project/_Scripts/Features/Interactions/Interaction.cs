using System.Collections;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    private Camera _camera;
    
    public float distance = 5f;
    public float knockbackForce = 2f;
    
    public void Init()
    {
        _camera = Camera.main;
        
        StartCoroutine(SecondaryKeyRoutine());
    }
    
    private void Update()
    {
        if (G.PlayerInput.InputActions.Player.Attack.IsPressed())
        {
            G.BlockDestroing.DigBlock();
            if (G.PlayerInput.InputActions.Player.Attack.WasPerformedThisFrame())
                TryAttackEnemy();
        }
        else
        {
            G.BlockDestroing.StopDigBlock();
        }
    }

    private IEnumerator SecondaryKeyRoutine()
    {
        while (true)
        {
            if (G.PlayerInput.InputActions.Player.Secondary.IsPressed())
            {
                if (TryInteractWithBlock())
                {
                    yield return new WaitForSeconds(0.1f);
                }
                else if (G.Hotbar.TryUseSelectedItem())
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            yield return null;
        }
    }
    
    private bool TryAttackEnemy()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (!Physics.Raycast(ray, out RaycastHit hit, distance)) return false;
        if (!hit.collider.TryGetComponent<Enemy>(out var enemy)) return false;

        var player = G.Player;
        enemy.TakeDamage(player.AttackDamage);
        enemy.Knockback.Apply(player.transform.position, knockbackForce);
        enemy.Flash.Play();

        return true;
    }
    
    private bool TryInteractWithBlock()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        int layerMask = ~LayerMask.GetMask("Player");

        if (!Physics.Raycast(ray, out RaycastHit hit, distance, layerMask)) return false;

        Vector3 blockCenter = hit.point - hit.normal * 0.01f;
        Vector3Int blockWorldPos = Vector3Int.FloorToInt(blockCenter);

        Vector2Int chunkPos = G.World.GetChunkCoordinatesContainingBlock(blockWorldPos);
        if (!G.World.chunks.TryGetValue(chunkPos, out ChunkData chunkData)) return false;

        Vector3Int chunkOrigin = new Vector3Int(chunkPos.x, 0, chunkPos.y) * G.World.terrainConfig.chunkWidth;
        Vector3Int localPos = blockWorldPos - chunkOrigin;

        BlockState blockState = chunkData.Blocks[localPos.x, localPos.y, localPos.z];
        
        switch (blockState.type)
        {
            case BlockType.Trapdoor:
                chunkData.Blocks[localPos.x, localPos.y, localPos.z] = new BlockState(
                    blockState.type,
                    blockState.direction,
                    (byte)(blockState.state == 0 ? 1 : 0)
                );

                G.World.MarkChunkAndBoundaryNeighborsDirty(chunkPos, localPos);
                return true;
            default:
                return false;
        }
    }
}
