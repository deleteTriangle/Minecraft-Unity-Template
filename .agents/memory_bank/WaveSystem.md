# Wave System

Status: Implemented, Not Enabled In MainScene

Summary:
WaveManager runs countdown, spawn, active, and completion states. WaveSpawner instantiates configured enemy groups at random scene spawn points and WaveManager tracks their deaths.

Key files:
Assets/_Project/_Scripts/Features/Combat/CombatManager.cs
Assets/_Project/_Scripts/Features/Combat/Waves/WaveManager.cs
Assets/_Project/_Scripts/Features/Combat/Waves/WaveSpawner.cs
Assets/_Project/_Scripts/Features/Combat/Waves/WaveTimer.cs
Assets/_Project/_Scripts/Features/Combat/Configs/WaveConfig.cs
Assets/_Project/Configs/Waves

Dependencies:
GameLoop
EnemyConfig and prefabs
Player transform
Scene spawn points

Critical Notes:
CombatManager registers WaveManager in GameLoop and starts the cycle during EntryPoint initialization.
Wave completion occurs when its duration expires or when all scheduled enemies have spawned and the tracked alive set becomes empty.
Provided assets: Wave1 (10 zombies then 4 skeletons), Wave2 (15 skeletons), Wave3 (empty), each with five-second countdown and 20-second duration.
`MainScene.unity` currently serializes `CombatManager.waves: []`; `StartCycle()` returns immediately. Assign Wave1, Wave2, and Wave3 to enable the feature.
An empty final wave completes at timer expiry; the code then logs a win but has no user-facing victory state.

Requirements:
[x] Configurable countdown and duration
[x] Delayed spawn groups
[x] Random spawn point selection
[x] Completion by timer or enemy clearance
[x] Multi-wave sequence events
[ ] Scene wiring for configured waves
