# Navigation

Status: Done

Summary:
World creates a NavMeshSurface at runtime after generating chunk meshes, builds it from physics colliders, and schedules an update after block placement or destruction.

Key files:
Assets/_Project/_Scripts/Features/Terrain/World.cs
Assets/_Project/_Scripts/Features/Combat/AI/EnemyMover.cs
Assets/_Project/_Scripts/Features/Interactions/BlockPlacing.cs
Assets/_Project/_Scripts/Features/Interactions/BlockDestroing.cs

Dependencies:
Unity AI Navigation
NavMeshSurface
Chunk mesh colliders
NavMeshAgent

Critical Notes:
The surface collects all scene objects and uses PhysicsColliders geometry.
World builds the NavMesh once during initialization after every initial ChunkRenderer has generated its mesh.
Block edit events delay 0.1 seconds then call `UpdateNavMesh`; a pending rebuild coroutine is cancelled when a new edit arrives.
`RebuildAreaNavMesh` accepts center/size but currently calls `UpdateNavMesh` for the surface data rather than restricting collection to that region.
Water has trigger colliders and does not receive a MeshCollider in ChunkRenderer.

Requirements:
[x] Runtime NavMesh build
[x] NavMesh refresh after terrain edits
[x] Agent-safe movement guards
