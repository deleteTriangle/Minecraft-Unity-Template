# Block Interaction

Status: Done

Summary:
Centered camera raycasts support continuous mining, placement, and secondary interaction with doors and trapdoors. Mining displays a temporary progress cube and respects the selected pickaxe multiplier.

Key files:
Assets/_Source/Develop/Runtime/Gameplay/Features/Interactions/Interaction.cs
Assets/_Source/Develop/Runtime/Gameplay/Features/Interactions/BlockDestroing.cs
Assets/_Source/Develop/Runtime/Gameplay/Features/Interactions/BlockPlacing.cs

Dependencies:
Camera.main
World
BlockRegistry
Hotbar
Interaction distance

Critical Notes:
Primary attack holds mining and performs one enemy-attack attempt on the performed frame; mining raycasts exclude Player and Mob layers.
Mining duration comes from `BlockConfig.timeToDestroy`, reduced by selected `PickaxeConfig.digModifier`; no pickaxe assets are currently seeded into the hotbar.
The visual progress cube requires `destructionMaterial` with `_Progress` and `_Scale` properties.
Placement only succeeds in Air cells in loaded chunks. Door placement validates both cells and writes lower/upper halves atomically. Trapdoor placement chooses closed upper/lower position from hit height.
Destroying creates no item drop and does not add an item to the inventory.
Secondary interaction toggles traps and both door halves. Destroying either door half removes both. `World.TryGetBlock` and `TrySetBlock` own cross-chunk-safe reads/writes and dirty marking.

Requirements:
[x] Raycast targeting at crosshair
[x] Timed block destruction feedback
[x] Adjacent-face block placement
[x] Trapdoor open/close interaction
[x] Door placement, open/close, linked destruction
[x] Mesh update notifications
