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

        if (!G.World.TryGetBlock(blockWorldPos, out BlockState blockState)) return false;
        
        switch (blockState.type)
        {
            case BlockType.Trapdoor:
                G.World.TrySetBlock(blockWorldPos, new BlockState(
                    blockState.type,
                    blockState.direction,
                    BlockStateFlags.WithOpen(blockState, !BlockStateFlags.IsOpen(blockState))));
                return true;
            case BlockType.Door:
                Vector3Int lowerPos = BlockStateFlags.IsUpper(blockState)
                    ? blockWorldPos + Vector3Int.down
                    : blockWorldPos;
                Vector3Int upperPos = lowerPos + Vector3Int.up;
                if (!G.World.TryGetBlock(lowerPos, out BlockState lowerBlock) || !G.World.TryGetBlock(upperPos, out BlockState upperBlock)) return false;
                if (lowerBlock.type != BlockType.Door || upperBlock.type != BlockType.Door) return false;

                bool isOpen = !BlockStateFlags.IsOpen(lowerBlock);
                G.World.TrySetBlock(lowerPos, new BlockState(BlockType.Door, lowerBlock.direction, BlockStateFlags.WithOpen(lowerBlock, isOpen)));
                G.World.TrySetBlock(upperPos, new BlockState(BlockType.Door, upperBlock.direction, (byte)(BlockStateFlags.WithOpen(upperBlock, isOpen) | BlockStateFlags.Upper)));
                return true;
            default:
                return false;
        }
    }
}
