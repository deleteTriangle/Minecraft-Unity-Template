# Voxel World

Status: Done

Summary:
World creates a fixed 10x10 grid of 16x128 chunks at startup. Each chunk holds `BlockState[,,]` data generated from Perlin-noise height values, then renders visible block faces to runtime meshes.

Key files:
Assets/_Project/_Scripts/Features/Terrain/World.cs
Assets/_Project/_Scripts/Features/Terrain/TerrainGenerator.cs
Assets/_Project/_Scripts/Features/Terrain/ChunkData.cs
Assets/_Project/_Scripts/Features/Terrain/ChunkRenderer.cs
Assets/_Project/Configs/TerrainConfig.asset
Assets/_Project/Prefabs/Terrain/Chunk.prefab

Dependencies:
TerrainConfig
BlockRegistry
ChunkRenderer
NavMeshSurface

Critical Notes:
`RenderDistance` is currently a chunk count, not a camera-driven streaming distance. World creation spans local chunk coordinates 0..9 on both axes.
Terrain surface is grass below Perlin height `10..15`; all higher cells are air.
Chunk rebuilds are queued and capped by `chunkRebuildsPerFrame` (default 1), avoiding rebuilding every changed chunk in one frame.
Boundary edits also queue neighboring chunks so exposed faces remain correct.
ChunkRenderer makes separate block and water meshes, creates trigger colliders per water block, and rebuilds trapdoor box colliders from state/configuration.

Requirements:
[x] Chunk data storage
[x] Procedural terrain generation
[x] Face-culling mesh generation
[x] Incremental mesh rebuilding after edits
[x] Separate water and trapdoor collision handling
