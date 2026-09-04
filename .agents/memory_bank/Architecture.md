# Architecture

Status: Done

Summary:
The game uses a scene-wired composition root plus a global service locator. Frame-driven domain objects implement `ITickable` and are advanced by `GameLoop.Update()`.

Key files:
Assets/_Project/_Scripts/Core/EntryPoint.cs
Assets/_Project/_Scripts/Core/G.cs
Assets/_Project/_Scripts/Features/GameLoop.cs

Dependencies:
EntryPoint scene references
G
GameLoop

Critical Notes:
EntryPoint registers `PlayerInput`, player movement, block systems, world, interaction, player, hotbar, GameLoop, and CombatManager before calling their `Init` methods.
`G.Get<T>()` throws for an unregistered service; initialization order is therefore a runtime contract.
GameLoop has no duplicate guard and iterates its live list directly. Enemies and WaveManager register there for AI/timer ticking.
Do not introduce services that access `G` before their prerequisites are registered.

Requirements:
[x] Global access to scene systems
[x] Explicit bootstrap order
[x] Shared frame tick for non-MonoBehaviour logic
