using System;
using System.Collections;
using System.Collections.Generic;
using _Source.Develop.Runtime.Gameplay.Features.Terrain;
using Unity.AI.Navigation;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    public TerrainConfig terrainConfig;
    
    public int ChunkWidth => terrainConfig.chunkWidth;
    public int ChunkHeight => terrainConfig.chunkHeight;

    public ChunkData chunkData;

    private List<Vector3> blocksVertices = new List<Vector3>();
    private List<Vector2> blocksUvs = new List<Vector2>();
    private List<int> blocksTriangles = new List<int>();
    private float maxBlocksBoundY;
    
    private List<Vector3> waterVertices = new List<Vector3>();
    private List<int> waterTriangles = new List<int>();
    private List<Vector2> waterUVs = new List<Vector2>();
    private Vector3 maxWaterBound;
    
    private ChunkData leftChunk;
    private ChunkData rightChunk;
    private ChunkData forwardChunk;
    private ChunkData backChunk;

    private int atlasWidth = 256;
    private int atlasHeight = 256;
    private int pixelsPerTile = 16;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh blocksMesh;
    private Mesh waterMesh;
    private Transform waterObj;
    private MeshFilter waterMeshFilter;
    private MeshRenderer waterMeshRenderer;
    private Transform trapdoorObj;
    
    public void Init(TerrainConfig config)
    {
        terrainConfig = config;
        CacheComponents();

        G.World.chunks.TryGetValue(chunkData.pos + Vector2Int.left, out leftChunk);
        G.World.chunks.TryGetValue(chunkData.pos + Vector2Int.right, out rightChunk);
        G.World.chunks.TryGetValue(chunkData.pos + Vector2Int.up, out forwardChunk);
        G.World.chunks.TryGetValue(chunkData.pos + Vector2Int.down, out backChunk);
        
        GenerateChunkMesh();
    }

    private void CacheComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        blocksMesh = new Mesh();
        blocksMesh.name = "Chunk Blocks Mesh";
        blocksMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        waterMesh = new Mesh();
        waterMesh.name = "Chunk Water Mesh";
        waterMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    }
    
    public void GenerateChunkMesh()
    {
        blocksVertices.Clear();
        blocksUvs.Clear();
        blocksTriangles.Clear();
        
        waterVertices.Clear();
        waterUVs.Clear();
        waterTriangles.Clear();
        
        maxBlocksBoundY = 0;
        maxWaterBound = Vector3.zero;
        
        for (int y = 0; y < ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    if (TryGenerateBlock(x, y, z))
                    {
                        if (chunkData.Blocks[x, y, z].type == BlockType.Water)
                        {
                            maxWaterBound.x = Mathf.Max(maxWaterBound.x, x);
                            maxWaterBound.y = Mathf.Max(maxWaterBound.y, y);
                            maxWaterBound.z = Mathf.Max(maxWaterBound.z, z);
                        }
                        else
                        {
                            maxBlocksBoundY = Mathf.Max(maxBlocksBoundY, y);
                        }
                    }
                }
            }
        }

        GenerateBaseBlocksMesh();
        GenerateWaterMesh();
    }

    private void GenerateBaseBlocksMesh()
    {
        GenerateMesh(blocksMesh, meshFilter, meshCollider, blocksVertices, blocksUvs, blocksTriangles);
    }

    private void GenerateWaterMesh()
    {
        if (waterObj == null)
        {
            waterObj = new GameObject("Water").transform;
            waterObj.SetParent(transform);
            waterObj.localPosition = Vector3.zero;

            waterMeshFilter = waterObj.gameObject.AddComponent<MeshFilter>();
            waterMeshRenderer = waterObj.gameObject.AddComponent<MeshRenderer>();
            waterMeshRenderer.material = meshRenderer.material;
            
            //waterObj.gameObject.AddComponent<MeshCollider>();
            waterObj.gameObject.tag = "Water";
        }

        foreach (var col in waterObj.GetComponents<BoxCollider>())
            Destroy(col);
        
        for (int y = 0; y < ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    if (chunkData.Blocks[x, y, z].type != BlockType.Water) continue;

                    BoxCollider col = waterObj.gameObject.AddComponent<BoxCollider>();
                    col.isTrigger = true;
                    col.center = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                    col.size = Vector3.one;
                }
            }
        }

        GenerateMesh(waterMesh, waterMeshFilter, null, waterVertices, waterUVs, waterTriangles);
    }
    
    private void GenerateTrapdoorColliders()
    {
        // Находим или создаём дочерний объект
        if (trapdoorObj == null)
        {
            trapdoorObj = new GameObject("Trapdoors").transform;
            trapdoorObj.SetParent(transform);
            trapdoorObj.localPosition = Vector3.zero;
        }

        // Удаляем старые коллайдеры
        foreach (var col in trapdoorObj.GetComponents<BoxCollider>())
            Destroy(col);

        for (int y = 0; y < ChunkHeight; y++)
        for (int x = 0; x < ChunkWidth; x++)
        for (int z = 0; z < ChunkWidth; z++)
        {
            if (chunkData.Blocks[x, y, z].type != BlockType.Trapdoor) continue;

            BlockState blockState = chunkData.Blocks[x, y, z];
            BlockConfig config = G.BlockRegistry.GetBlockConfig(BlockType.Trapdoor)
                .GetStateConfig(blockState.state);
            var (rotatedOffset, rotatedSize) = RotateBlockConfig(config.offset, config.size, blockState.direction);

            BoxCollider col = trapdoorObj.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(x, y, z) + rotatedOffset + rotatedSize / 2f;
            col.size = rotatedSize;
        }
    }
    
    private void GenerateMesh(
        Mesh mesh,
        MeshFilter filter,
        MeshCollider collider,
        List<Vector3> vertices,
        List<Vector2> uvs,
        List<int> triangles)
    {
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        mesh.Optimize();
        mesh.RecalculateNormals();
        
        mesh.RecalculateBounds();
        filter.sharedMesh = mesh;
        
        if (collider != null)
            collider.sharedMesh = mesh;
    }
    
    // public void GeneratePlacedBlock(Vector3Int pos)
    // {
    //     int x = pos.x;
    //     int y = pos.y;
    //     int z = pos.z;
    //     
    //     TryGenerateBlock(x, y, z);
    //     TryGenerateBlock(x, y, z + 1);
    //     TryGenerateBlock(x, y + 1, z);
    //     TryGenerateBlock(x + 1, y, z);
    //     TryGenerateBlock(x - 1, y, z);
    //     TryGenerateBlock(x, y - 1, z);
    //     TryGenerateBlock(x, y, z - 1);
    //
    //     chunkMesh.Clear();
    //     chunkMesh.vertices = vertices.ToArray();
    //     chunkMesh.uv = uvs.ToArray();
    //     chunkMesh.triangles = triangles.ToArray();
    //
    //     chunkMesh.Optimize();
    //     chunkMesh.RecalculateNormals();
    //     chunkMesh.RecalculateBounds();
    //     
    //     GetComponent<MeshCollider>().sharedMesh = chunkMesh;
    // }
    
    private bool TryGenerateBlock(int x, int y, int z)
    {
        if (chunkData.Blocks[x, y, z].type == BlockType.Air) return false;
        
        var blockPosition = new Vector3Int(x, y, z);
        BlockType blockType = chunkData.Blocks[x, y, z].type;
        BlockState blockState = chunkData.Blocks[x, y, z];
        var blockConfig = BlockGeometry.GetStateConfig(blockState);
        byte playerDirection = blockState.direction;
        
        BlockType rightBlock = GetBlockAtPosition(blockPosition + Vector3Int.right);
        if (blockConfig.type == BlockType.Water && rightBlock == BlockType.Air)
            GenerateRightSide(blockPosition, blockConfig, playerDirection);
        else if (blockConfig.type != BlockType.Water && (rightBlock == BlockType.Water || G.BlockRegistry.GetBlockConfig(rightBlock).size != Vector3.one))
            GenerateRightSide(blockPosition, blockConfig, playerDirection);
        else if (rightBlock == BlockType.Air)
            GenerateRightSide(blockPosition, blockConfig, playerDirection);

        BlockType leftBlock = GetBlockAtPosition(blockPosition + Vector3Int.left);
        if (blockConfig.type == BlockType.Water && leftBlock == BlockType.Air)
            GenerateLeftSide(blockPosition, blockConfig, playerDirection);
        else if (blockConfig.type != BlockType.Water && (leftBlock == BlockType.Water || G.BlockRegistry.GetBlockConfig(leftBlock).size != Vector3.one))
            GenerateLeftSide(blockPosition, blockConfig, playerDirection);
        else if (leftBlock == BlockType.Air)
            GenerateLeftSide(blockPosition, blockConfig, playerDirection); // было GenerateRightSide

        BlockType forwardBlock = GetBlockAtPosition(blockPosition + Vector3Int.forward);
        if (blockConfig.type == BlockType.Water && forwardBlock == BlockType.Air)
            GenerateFrontSide(blockPosition, blockConfig, playerDirection);
        else if (blockConfig.type != BlockType.Water && (forwardBlock == BlockType.Water || G.BlockRegistry.GetBlockConfig(forwardBlock).size != Vector3.one))
            GenerateFrontSide(blockPosition, blockConfig, playerDirection);
        else if (forwardBlock == BlockType.Air)
            GenerateFrontSide(blockPosition, blockConfig, playerDirection); // было GenerateRightSide

        BlockType backBlock = GetBlockAtPosition(blockPosition + Vector3Int.back);
        if (blockConfig.type == BlockType.Water && backBlock == BlockType.Air)
            GenerateBackSide(blockPosition, blockConfig, playerDirection);
        else if (blockConfig.type != BlockType.Water && (backBlock == BlockType.Water || G.BlockRegistry.GetBlockConfig(backBlock).size != Vector3.one))
            GenerateBackSide(blockPosition, blockConfig, playerDirection);
        else if (backBlock == BlockType.Air)
            GenerateBackSide(blockPosition, blockConfig, playerDirection); // было GenerateRightSide

        BlockType topBlock = GetBlockAtPosition(blockPosition + Vector3Int.up);
        if (blockConfig.type == BlockType.Water && topBlock == BlockType.Air)
            GenerateTopSide(blockPosition, blockConfig, playerDirection);
        else if (blockConfig.type != BlockType.Water && (topBlock == BlockType.Water || G.BlockRegistry.GetBlockConfig(topBlock).size != Vector3.one))
            GenerateTopSide(blockPosition, blockConfig, playerDirection);
        else if (topBlock == BlockType.Air)
            GenerateTopSide(blockPosition, blockConfig, playerDirection); // было GenerateRightSide

        BlockType bottomBlock = GetBlockAtPosition(blockPosition + Vector3Int.down);
        if (blockConfig.type == BlockType.Water && bottomBlock == BlockType.Air)
            GenerateBottomSide(blockPosition, blockConfig, playerDirection);
        else if (blockConfig.type != BlockType.Water && (bottomBlock == BlockType.Water || G.BlockRegistry.GetBlockConfig(bottomBlock).size != Vector3.one))
            GenerateBottomSide(blockPosition, blockConfig, playerDirection);
        else if (bottomBlock == BlockType.Air)
            GenerateBottomSide(blockPosition, blockConfig, playerDirection); // было GenerateRightSide
        
        return true;
    }

    private BlockType GetBlockAtPosition(Vector3Int pos)
    {
        if (pos.x >= 0 && pos.x < ChunkWidth &&
            pos.y >= 0 && pos.y < ChunkHeight &&
            pos.z >= 0 && pos.z < ChunkWidth)
        {
            return chunkData.Blocks[pos.x, pos.y, pos.z].type;
        }
        else
        {
            if (pos.y >= ChunkHeight) return BlockType.Air;
            if (pos.y < 0) return BlockType.Grass;
            
            Vector2Int adjacentChunkPos = chunkData.pos;
            if (pos.x < 0)
            {
                if (leftChunk == null) return BlockType.Grass;
                pos.x += ChunkWidth;
                return leftChunk.Blocks[pos.x, pos.y, pos.z].type;
            }
            if (pos.x >= ChunkWidth)
            {
                if (rightChunk == null) return BlockType.Grass;
                pos.x -= ChunkWidth;
                return rightChunk.Blocks[pos.x, pos.y, pos.z].type;
            }
            if (pos.z < 0)
            {
                if (backChunk == null) return BlockType.Grass;
                pos.z += ChunkWidth;
                return backChunk.Blocks[pos.x, pos.y, pos.z].type;
            }
            if (pos.z >= ChunkWidth)
            {
                if (forwardChunk == null) return BlockType.Grass;
                pos.z -= ChunkWidth;
                return forwardChunk.Blocks[pos.x, pos.y, pos.z].type;
            }

            return BlockType.Air;
        }
    }

    private void GenerateRightSide(Vector3Int blockPosition, BlockConfig blockConfig, byte playerDirection)
    {
        var (rotatedOffset, rotatedSize) = BlockGeometry.RotateBounds(blockConfig.offset, blockConfig.size, playerDirection);

        float x  = rotatedOffset.x + rotatedSize.x;
        float y0 = rotatedOffset.y;
        float y1 = rotatedOffset.y + rotatedSize.y;
        float z0 = rotatedOffset.z;
        float z1 = rotatedOffset.z + rotatedSize.z;

        if (blockConfig.type == BlockType.Water)
        {
            waterVertices.Add(new Vector3(x, y0, z1) + blockPosition);
            waterVertices.Add(new Vector3(x, y0, z0) + blockPosition);
            waterVertices.Add(new Vector3(x, y1, z1) + blockPosition);
            waterVertices.Add(new Vector3(x, y1, z0) + blockPosition);
            
            AddLastUVs(waterUVs, blockConfig, BlockFaceOffset.Right, playerDirection);
            AddLastVerticiesSquare(waterTriangles, waterVertices);
        }
        else
        {
            blocksVertices.Add(new Vector3(x, y0, z1) + blockPosition); // bottom-left
            blocksVertices.Add(new Vector3(x, y0, z0) + blockPosition); // bottom-right
            blocksVertices.Add(new Vector3(x, y1, z1) + blockPosition); // top-left
            blocksVertices.Add(new Vector3(x, y1, z0) + blockPosition); // top-right
            
            AddLastUVs(blocksUvs, blockConfig, BlockFaceOffset.Right, playerDirection);
            AddLastVerticiesSquare(blocksTriangles, blocksVertices);
        }
    }
    private void GenerateLeftSide(Vector3Int blockPosition, BlockConfig blockConfig, byte playerDirection)
    {
        var (rotatedOffset, rotatedSize) = BlockGeometry.RotateBounds(blockConfig.offset, blockConfig.size, playerDirection);

        float x  = rotatedOffset.x;
        float y0 = rotatedOffset.y;
        float y1 = rotatedOffset.y + rotatedSize.y;
        float z0 = rotatedOffset.z;
        float z1 = rotatedOffset.z + rotatedSize.z;

        if (blockConfig.type == BlockType.Water)
        {
            waterVertices.Add(new Vector3(x, y0, z0) + blockPosition);
            waterVertices.Add(new Vector3(x, y0, z1) + blockPosition);
            waterVertices.Add(new Vector3(x, y1, z0) + blockPosition);
            waterVertices.Add(new Vector3(x, y1, z1) + blockPosition);
            AddLastUVs(waterUVs, blockConfig, BlockFaceOffset.Left, playerDirection);
            AddLastVerticiesSquare(waterTriangles, waterVertices);
        }
        else
        {
            blocksVertices.Add(new Vector3(x, y0, z0) + blockPosition);
            blocksVertices.Add(new Vector3(x, y0, z1) + blockPosition);
            blocksVertices.Add(new Vector3(x, y1, z0) + blockPosition);
            blocksVertices.Add(new Vector3(x, y1, z1) + blockPosition);
            AddLastUVs(blocksUvs, blockConfig, BlockFaceOffset.Left, playerDirection);
            AddLastVerticiesSquare(blocksTriangles, blocksVertices);
        }
    }

    private void GenerateFrontSide(Vector3Int blockPosition, BlockConfig blockConfig, byte playerDirection)
    {
        var (rotatedOffset, rotatedSize) = BlockGeometry.RotateBounds(blockConfig.offset, blockConfig.size, playerDirection);

        float x0 = rotatedOffset.x;
        float x1 = rotatedOffset.x + rotatedSize.x;
        float y0 = rotatedOffset.y;
        float y1 = rotatedOffset.y + rotatedSize.y;
        float z  = rotatedOffset.z + rotatedSize.z;

        if (blockConfig.type == BlockType.Water)
        {
            waterVertices.Add(new Vector3(x0, y0, z) + blockPosition);
            waterVertices.Add(new Vector3(x1, y0, z) + blockPosition);
            waterVertices.Add(new Vector3(x0, y1, z) + blockPosition);
            waterVertices.Add(new Vector3(x1, y1, z) + blockPosition);
            AddLastUVs(waterUVs, blockConfig, BlockFaceOffset.Front, playerDirection);
            AddLastVerticiesSquare(waterTriangles, waterVertices);
        }
        else
        {
            blocksVertices.Add(new Vector3(x0, y0, z) + blockPosition);
            blocksVertices.Add(new Vector3(x1, y0, z) + blockPosition);
            blocksVertices.Add(new Vector3(x0, y1, z) + blockPosition);
            blocksVertices.Add(new Vector3(x1, y1, z) + blockPosition);
            AddLastUVs(blocksUvs, blockConfig, BlockFaceOffset.Front, playerDirection);
            AddLastVerticiesSquare(blocksTriangles, blocksVertices);
        }
    }

    private void GenerateBackSide(Vector3Int blockPosition, BlockConfig blockConfig, byte playerDirection)
    {
        var (rotatedOffset, rotatedSize) = BlockGeometry.RotateBounds(blockConfig.offset, blockConfig.size, playerDirection);

        float x0 = rotatedOffset.x;
        float x1 = rotatedOffset.x + rotatedSize.x;
        float y0 = rotatedOffset.y;
        float y1 = rotatedOffset.y + rotatedSize.y;
        float z  = rotatedOffset.z;

        if (blockConfig.type == BlockType.Water)
        {
            waterVertices.Add(new Vector3(x1, y0, z) + blockPosition);
            waterVertices.Add(new Vector3(x0, y0, z) + blockPosition);
            waterVertices.Add(new Vector3(x1, y1, z) + blockPosition);
            waterVertices.Add(new Vector3(x0, y1, z) + blockPosition);
            AddLastUVs(waterUVs, blockConfig, BlockFaceOffset.Back, playerDirection);
            AddLastVerticiesSquare(waterTriangles, waterVertices);
        }
        else
        {
            blocksVertices.Add(new Vector3(x1, y0, z) + blockPosition);
            blocksVertices.Add(new Vector3(x0, y0, z) + blockPosition);
            blocksVertices.Add(new Vector3(x1, y1, z) + blockPosition);
            blocksVertices.Add(new Vector3(x0, y1, z) + blockPosition);
            AddLastUVs(blocksUvs, blockConfig, BlockFaceOffset.Back, playerDirection);
            AddLastVerticiesSquare(blocksTriangles, blocksVertices);
        }
    }

    private void GenerateTopSide(Vector3Int blockPosition, BlockConfig blockConfig, byte playerDirection)
    {
        var (rotatedOffset, rotatedSize) = BlockGeometry.RotateBounds(blockConfig.offset, blockConfig.size, playerDirection);

        float x0 = rotatedOffset.x;
        float x1 = rotatedOffset.x + rotatedSize.x;
        float y  = rotatedOffset.y + rotatedSize.y;
        float z0 = rotatedOffset.z;
        float z1 = rotatedOffset.z + rotatedSize.z;

        if (blockConfig.type == BlockType.Water)
        {
            waterVertices.Add(new Vector3(x0, y, z0) + blockPosition);
            waterVertices.Add(new Vector3(x0, y, z1) + blockPosition);
            waterVertices.Add(new Vector3(x1, y, z0) + blockPosition);
            waterVertices.Add(new Vector3(x1, y, z1) + blockPosition);
            AddLastUVs(waterUVs, blockConfig, BlockFaceOffset.Top, playerDirection);
            AddLastVerticiesSquare(waterTriangles, waterVertices);
        }
        else
        {
            blocksVertices.Add(new Vector3(x0, y, z0) + blockPosition);
            blocksVertices.Add(new Vector3(x0, y, z1) + blockPosition);
            blocksVertices.Add(new Vector3(x1, y, z0) + blockPosition);
            blocksVertices.Add(new Vector3(x1, y, z1) + blockPosition);
            AddLastUVs(blocksUvs, blockConfig, BlockFaceOffset.Top, playerDirection);
            AddLastVerticiesSquare(blocksTriangles, blocksVertices);
        }
    }

    private void GenerateBottomSide(Vector3Int blockPosition, BlockConfig blockConfig, byte playerDirection)
    {
        var (rotatedOffset, rotatedSize) = BlockGeometry.RotateBounds(blockConfig.offset, blockConfig.size, playerDirection);

        float x0 = rotatedOffset.x;
        float x1 = rotatedOffset.x + rotatedSize.x;
        float y  = rotatedOffset.y;
        float z0 = rotatedOffset.z;
        float z1 = rotatedOffset.z + rotatedSize.z;

        if (blockConfig.type == BlockType.Water)
        {
            waterVertices.Add(new Vector3(x0, y, z0) + blockPosition);
            waterVertices.Add(new Vector3(x1, y, z0) + blockPosition);
            waterVertices.Add(new Vector3(x0, y, z1) + blockPosition);
            waterVertices.Add(new Vector3(x1, y, z1) + blockPosition);
            AddLastUVs(waterUVs, blockConfig, BlockFaceOffset.Bottom, playerDirection);
            AddLastVerticiesSquare(waterTriangles, waterVertices);
        }
        else
        {
            blocksVertices.Add(new Vector3(x0, y, z0) + blockPosition);
            blocksVertices.Add(new Vector3(x1, y, z0) + blockPosition);
            blocksVertices.Add(new Vector3(x0, y, z1) + blockPosition);
            blocksVertices.Add(new Vector3(x1, y, z1) + blockPosition);
            AddLastUVs(blocksUvs, blockConfig, BlockFaceOffset.Bottom, playerDirection);
            AddLastVerticiesSquare(blocksTriangles, blocksVertices);
        }
    }

    private void AddLastVerticiesSquare(List<int> triangles, List<Vector3> vertices)
    {
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);

        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2);
    }

    private void AddLastUVs(List<Vector2> uvs, BlockConfig blockConfig, BlockFaceOffset faceOffset, byte direction)
    {
        BlockFaceOffset rotatedFace = RotateFace(faceOffset, direction, blockConfig);
        BlockFaceOffset sizedFace = RotateFaceOffset(faceOffset, direction);
        var (rotatedOffset, rotatedSize) = BlockGeometry.RotateBounds(blockConfig.offset, blockConfig.size, direction);

        Vector2Int tile = GetTile(blockConfig, rotatedFace);

        float x0 = (float)tile.x * pixelsPerTile / atlasWidth;
        float x1 = (float)(tile.x + 1) * pixelsPerTile / atlasWidth;
        float y0 = (float)tile.y * pixelsPerTile / atlasHeight;
        float y1 = (float)(tile.y + 1) * pixelsPerTile / atlasHeight;
        
        float uSize = sizedFace switch
        {
            BlockFaceOffset.Top    or BlockFaceOffset.Bottom => rotatedSize.x,
            BlockFaceOffset.Front  or BlockFaceOffset.Back   => rotatedSize.x,
            BlockFaceOffset.Left   or BlockFaceOffset.Right  => rotatedSize.z,
            _ => 1f
        };
        float vSize = sizedFace switch
        {
            BlockFaceOffset.Top    or BlockFaceOffset.Bottom => rotatedSize.z,
            BlockFaceOffset.Front  or BlockFaceOffset.Back   => rotatedSize.y,
            BlockFaceOffset.Left   or BlockFaceOffset.Right  => rotatedSize.y,
            _ => 1f
        };
        float uOffset = sizedFace switch
        {
            BlockFaceOffset.Top    or BlockFaceOffset.Bottom => rotatedOffset.x,
            BlockFaceOffset.Front  or BlockFaceOffset.Back   => rotatedOffset.x,
            BlockFaceOffset.Left   or BlockFaceOffset.Right  => rotatedOffset.z,
            _ => 0f
        };
        float vOffset = sizedFace switch
        {
            BlockFaceOffset.Top    or BlockFaceOffset.Bottom => rotatedOffset.z,
            _ => rotatedOffset.y
        };

        uvs.Add(new Vector2(Mathf.Lerp(x0, x1, uOffset),         Mathf.Lerp(y0, y1, vOffset)));
        uvs.Add(new Vector2(Mathf.Lerp(x0, x1, uOffset + uSize), Mathf.Lerp(y0, y1, vOffset)));
        uvs.Add(new Vector2(Mathf.Lerp(x0, x1, uOffset),         Mathf.Lerp(y0, y1, vOffset + vSize)));
        uvs.Add(new Vector2(Mathf.Lerp(x0, x1, uOffset + uSize), Mathf.Lerp(y0, y1, vOffset + vSize)));
    }
    
    
    private Vector2Int GetTile(BlockConfig blockConfig, BlockFaceOffset faceOffset)
    {
        int index = blockConfig.isComplex
            ? blockConfig.baseId + blockConfig.faceOffsets[(int)faceOffset]
            : blockConfig.baseId;

        int tilesPerRow = atlasWidth / pixelsPerTile;
        int tilesPerCol = atlasHeight / pixelsPerTile;
    
        int x = index % tilesPerRow;
        int y = (tilesPerCol - 1) - index / tilesPerRow;
        
        return new Vector2Int(x, y);
    }
    
    private BlockFaceOffset RotateFace(BlockFaceOffset face, byte direction, BlockConfig blockConfig)
    {
        if (face == BlockFaceOffset.Top || face == BlockFaceOffset.Bottom)
            return face;
    
        BlockFaceOffset[] order = { BlockFaceOffset.Front, BlockFaceOffset.Right, BlockFaceOffset.Back, BlockFaceOffset.Left };

        int currentIndex = System.Array.IndexOf(order, face);
        int rotatedIndex = (currentIndex - (int)direction + 8) % 4;
    
        return order[rotatedIndex];
    }
    
    private BlockFaceOffset RotateFaceOffset(BlockFaceOffset face, byte direction)
    {
        if (face == BlockFaceOffset.Top || face == BlockFaceOffset.Bottom) return face;
    
        return direction switch
        {
            0 => face,
            1 => face switch
            {
                BlockFaceOffset.Front => BlockFaceOffset.Right,
                BlockFaceOffset.Right => BlockFaceOffset.Back,
                BlockFaceOffset.Back  => BlockFaceOffset.Left,
                BlockFaceOffset.Left  => BlockFaceOffset.Front,
                _ => face
            },
            2 => face switch
            {
                BlockFaceOffset.Front => BlockFaceOffset.Back,
                BlockFaceOffset.Back  => BlockFaceOffset.Front,
                BlockFaceOffset.Left  => BlockFaceOffset.Right,
                BlockFaceOffset.Right => BlockFaceOffset.Left,
                _ => face
            },
            3 => face switch
            {
                BlockFaceOffset.Front => BlockFaceOffset.Left,
                BlockFaceOffset.Left  => BlockFaceOffset.Back,
                BlockFaceOffset.Back  => BlockFaceOffset.Right,
                BlockFaceOffset.Right => BlockFaceOffset.Front,
                _ => face
            },
            _ => face
        };
    }
    
    private (Vector3 offset, Vector3 size) RotateBlockConfig(Vector3 offset, Vector3 size, byte direction)
    {
        return direction switch
        {
            0 => (offset, size),
            1 => (new Vector3(1f - offset.z - size.z, offset.y, offset.x), 
                new Vector3(size.z, size.y, size.x)),
            2 => (new Vector3(1f - offset.x - size.x, offset.y, 1f - offset.z - size.z), 
                size),
            3 => (new Vector3(offset.z, offset.y, 1f - offset.x - size.x), 
                new Vector3(size.z, size.y, size.x)),
            _ => (offset, size)
        };
    }
}
