# Game Overview

Status: In Progress

Summary:
First-person voxel survival/combat template. The player explores a procedurally generated block world, mines and places blocks through a nine-slot hotbar, opens trapdoors, and can fight NavMesh-driven enemies. Wave assets and the full wave runtime exist, but the current `MainScene` has no waves assigned, so combat waves do not start in the scene as saved.

Key files:
Assets/_Project/Scenes/MainScene.unity
Assets/_Project/_Scripts/Core/EntryPoint.cs
Assets/_Project/_Scripts/Core/G.cs
Assets/_Project/_Scripts/Features

Dependencies:
Unity 6000.5.8f1
Input System 1.20.0
AI Navigation 2.0.14
TextMesh Pro / UGUI

Critical Notes:
`EntryPoint.Awake()` is the composition root and must initialize services in its existing order.
`G` is a static service locator; disabling EntryPoint clears all registrations.
The active controller is `RigidbodyController`; the legacy `FirstPersonController` is not registered or initialized.
The scene initializes a 9-slot inventory with Water, Trapdoor, Furnace, and Grass (60 each).
No persistence, crafting, drops, win UI, or player respawn is implemented.

Requirements:
[x] First-person voxel world
[x] Mining and block placement
[x] Hotbar UI and selection
[x] Enemy prefabs, health, melee and wave framework
[ ] Assign wave assets to CombatManager in MainScene to enable actual waves
