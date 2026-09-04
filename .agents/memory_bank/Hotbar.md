# Hotbar

Status: Done

Summary:
The hotbar is a nine-slot in-memory inventory. Slots are selected with numeric actions or scroll input, rendered in UI, and block items are consumed after successful placement.

Key files:
Assets/_Project/_Scripts/Features/Inventory/Hotbar.cs
Assets/_Project/_Scripts/Features/Inventory/HotbarInput.cs
Assets/_Project/_Scripts/Features/Inventory/HotbarView.cs
Assets/_Project/_Scripts/Features/Inventory/Slot.cs
Assets/_Project/_Scripts/Features/Inventory/SlotView.cs
Assets/_Project/Prefabs/UI/Slot.prefab

Dependencies:
G.PlayerInput
BlockPlacing
ItemConfig
BlockConfig

Critical Notes:
Selection wraps around slot 0..8 and broadcasts `OnHotbarChanged`.
`PutItemInSlot` only fills an empty slot; it neither replaces an item nor stacks into an existing matching stack.
`TryUseSelectedItem` presently handles BlockConfig only. It spends one item only after BlockPlacing succeeds.
The UI displays counts for blocks, but not for other ItemConfig types.
There is no save/load, item pickup, crafting, drag/drop, or capacity/stack limit logic.

Requirements:
[x] Nine slots
[x] Number-key and scroll selection
[x] Selected-state UI and item count
[x] Consume placed block items
