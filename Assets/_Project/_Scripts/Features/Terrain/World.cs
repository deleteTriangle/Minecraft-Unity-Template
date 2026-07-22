using System.Collections;
using System.Collections.Generic;
using _Project._Scripts.Features.Terrain;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class World : MonoBehaviour
{
    public TerrainConfig terrainConfig;
    public ChunkRenderer chunkPrefab;

    public Dictionary<Vector2Int, ChunkData> chunks = new();

    public int RenderDistance = 10;
    public int chunkRebuildsPerFrame = 1;
    
    private int ChunkWidth => terrainConfig.chunkWidth;
    private int ChunkHeight => terrainConfig.chunkHeight;
    
    private NavMeshSurface navMeshSurface;
    private Coroutine rebuildCoroutine;
    private readonly Queue<Vector2Int> dirtyChunkQueue = new();
    private readonly HashSet<Vector2Int> dirtyChunks = new();

    public void Init()
    {
        for (int x = 0; x < RenderDistance; x++)
        {
            for (int z = 0; z < RenderDistance; z++)
            {
                float xPos = x * ChunkWidth;
                float zPos = z * ChunkWidth;

                ChunkData chunkData = new ChunkData();
                chunkData.pos = new Vector2Int(x, z);
                chunkData.Blocks = TerrainGenerator.GenerateTerrain(xPos, zPos, terrainConfig);
                chunks.Add(new Vector2Int(x, z), chunkData);
            }
        }

        foreach (ChunkData chunkData in chunks.Values)
        {
            var chunk = Instantiate(chunkPrefab, new Vector3(chunkData.pos.x  * ChunkWidth, 0, chunkData.pos.y * ChunkWidth), Quaternion.identity, transform);
            chunkData.chunkRenderer = chunk;
            chunk.chunkData = chunkData;
            chunk.Init(terrainConfig);
        }
        
        navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.All; // весь мир
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        G.BlockPlacing.OnBlockPlaced += chunkPos =>
        {
            var chunkWidth = G.World.terrainConfig.chunkWidth;
            var chunkHeight = G.World.terrainConfig.chunkHeight;
            Vector3 center = new Vector3(
                chunkPos.x * chunkWidth + chunkWidth / 2f,
                chunkHeight / 2f,
                chunkPos.y * chunkWidth + chunkWidth / 2f
            );
            RebuildAreaNavMesh(center, new Vector3(chunkWidth, chunkHeight, chunkWidth));
        };

        G.BlockDestroing.OnBlockDestroyed += chunkPos =>
        {
            var chunkWidth = G.World.terrainConfig.chunkWidth;
            var chunkHeight = G.World.terrainConfig.chunkHeight;
            Vector3 center = new Vector3(
                chunkPos.x * chunkWidth + chunkWidth / 2f,
                chunkHeight / 2f,
                chunkPos.y * chunkWidth + chunkWidth / 2f
            );
            RebuildAreaNavMesh(center, new Vector3(chunkWidth, chunkHeight, chunkWidth));
        };
        
        BuildNavMesh();
    }

    private void Update()
    {
        int rebuilds = Mathf.Min(chunkRebuildsPerFrame, dirtyChunkQueue.Count);
        for (int i = 0; i < rebuilds; i++)
        {
            Vector2Int chunkPos = dirtyChunkQueue.Dequeue();
            dirtyChunks.Remove(chunkPos);

            if (chunks.TryGetValue(chunkPos, out ChunkData chunkData))
                chunkData.chunkRenderer.GenerateChunkMesh();
        }
    }

    public void MarkChunkDirty(Vector2Int chunkPos)
    {
        if (!chunks.ContainsKey(chunkPos)) return;
        if (!dirtyChunks.Add(chunkPos)) return;

        dirtyChunkQueue.Enqueue(chunkPos);
    }

    public void MarkChunkAndBoundaryNeighborsDirty(Vector2Int chunkPos, Vector3Int localPos)
    {
        MarkChunkDirty(chunkPos);

        if (localPos.x == 0) MarkChunkDirty(chunkPos + Vector2Int.left);
        if (localPos.z == 0) MarkChunkDirty(chunkPos + Vector2Int.down);
        if (localPos.x == ChunkWidth - 1) MarkChunkDirty(chunkPos + Vector2Int.right);
        if (localPos.z == ChunkWidth - 1) MarkChunkDirty(chunkPos + Vector2Int.up);
    }

    public void BuildNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
    
    public void RebuildAreaNavMesh(Vector3 center, Vector3 chunkSize)
    {
        if (rebuildCoroutine != null)
            StopCoroutine(rebuildCoroutine);
        
        rebuildCoroutine = StartCoroutine(RebuildNavMeshRoutine(center, chunkSize));
    }
    
    private IEnumerator RebuildNavMeshRoutine(Vector3 center, Vector3 chunkSize)
    {
        yield return new WaitForSeconds(0.1f);
        yield return navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        rebuildCoroutine = null;
    }
    
    public Vector2Int GetChunkCoordinatesContainingBlock(Vector3Int blockWorldPos)
    {
        return new Vector2Int(blockWorldPos.x / ChunkWidth, blockWorldPos.z / ChunkWidth);
    }
}
