# Blocks

Status: Done

Summary:
Block definitions are ScriptableObject assets indexed by `BlockType`. `BlockState` stores type, horizontal direction, and flags. `BlockGeometry` resolves state bounds before rotation.

Key files:
Assets/_Source/Develop/Runtime/Gameplay/Features/Blocks/BlockConfig.cs
Assets/_Source/Develop/Runtime/Gameplay/Features/Blocks/BlockGeometry.cs
Assets/_Source/Develop/Runtime/Gameplay/Features/Blocks/BlockState.cs
Assets/_Source/Resources/Blocks

Dependencies:
ItemConfig
BlockRegistry
ChunkRenderer

Critical Notes:
BlockRegistry must contain every BlockType read by terrain generation or rendering; `GetBlockConfig` indexes directly and throws when absent.
Each config provides destruction time, six texture-atlas face offsets, optional state-config variants, size, and offset.
Block textures come from `Assets/Bare Bones 1.21.11/assets/minecraft/textures/block`. `Tools/Blocks/Rebuild Texture Atlas` rebuilds `Assets/_Source/Art/Textures/BlockTextureAtlas.png`, rebinds `World.mat`, and updates configured block tile IDs. Add new source names and tile IDs to `BlockTextureAtlasBuilder` before rebuilding; set the new block's `baseId` and per-face offsets there.
`Open` is bit 0. `Upper` is bit 1. Trapdoors use `Upper` only when closed; doors use it to identify their upper half.
Trapdoor state 0 uses TrapdoorClosed; state 1 uses TrapdoorOpened. Door has closed/open assets and is loaded directly from `Resources/Blocks/Door`.

Requirements:
[x] Data-driven block registry
[x] Block state and orientation
[x] Per-face atlas data
[x] Stateful trapdoor variants
[x] Two-block door variants
