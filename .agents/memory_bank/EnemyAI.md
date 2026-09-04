# Enemy AI

Status: Done

Summary:
Spawned enemies chase the player using NavMeshAgent and execute melee attacks at a configurable update rate. Zombie deals configured damage; Skeleton deals twice its configured damage.

Key files:
Assets/_Project/_Scripts/Features/Combat/Enemies/Enemy.cs
Assets/_Project/_Scripts/Features/Combat/Enemies/Zombie.cs
Assets/_Project/_Scripts/Features/Combat/Enemies/Skeleton.cs
Assets/_Project/_Scripts/Features/Combat/AI/EnemyAI.cs
Assets/_Project/_Scripts/Features/Combat/AI/EnemyMover.cs
Assets/_Project/_Scripts/Features/Combat/Knockback.cs
Assets/_Project/_Scripts/Features/Combat/Flash.cs
Assets/_Project/Configs/Entity/Enemies

Dependencies:
EnemyConfig
NavMeshAgent
GameLoop
Player
World NavMesh

Critical Notes:
Enemy initialization requires Knockback, Flash, EnemyMover, and NavMeshAgent components on its prefab.
AI stops beyond chase range, attacks inside attack range after a cooldown, otherwise sets the player as NavMeshAgent destination.
Movement decisions occur at `updateTargetRate`; timer/cooldown still decrement every GameLoop tick.
Enemy death deactivates AI, unregisters the enemy from GameLoop, and destroys its GameObject.
Enemy mover no-ops when its agent is disabled or not on a NavMesh, so a valid NavMesh is required for pursuit.

Requirements:
[x] NavMesh pursuit
[x] Chase and attack ranges
[x] Per-enemy attacks
[x] Hit flash and knockback
[x] Destroy and unregister on death
