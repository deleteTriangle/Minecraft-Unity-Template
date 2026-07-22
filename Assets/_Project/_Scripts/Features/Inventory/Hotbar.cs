using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotbar
{
    public event Action OnHotbarChanged;
    
    private Slot[] slots;
    public int selectedSlotIndex;

    private int slotsCount;

    public Hotbar(int slotsCount)
    {
        slots = new Slot[slotsCount];
        this.slotsCount = slotsCount;

        selectedSlotIndex = 0;
        
        for (int i = 0; i < slotsCount; i++)
            slots[i] = new Slot();
    }

    public void SelectSlot(int slotIndex)
    {
        slotIndex = (slotIndex + slotsCount) % slotsCount;
        selectedSlotIndex = slotIndex;
        
        OnHotbarChanged?.Invoke();
    }

    public ItemConfig GetSelectedItem()
    {
        return GetItemInSlot(selectedSlotIndex);
    }

    public bool TryPutItemInFreeSlot(ItemConfig item, int amount)
    {
        for (int i = 0; i < slotsCount; i++)
        {
            if (slots[i].item == null)
            {
                PutItemInSlot(i, item, amount);
                return true;
            }
        }

        return false;
    }
    
    public void PutItemInSlot(int slotIndex, ItemConfig item, int amount)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            Slot slot = GetSlot(slotIndex);
            
            if (slot.item == null)
            {
                slots[slotIndex].item = item;
                slots[slotIndex].amount = amount;
                
                OnHotbarChanged?.Invoke();
            }
        }
    }

    private void SpendItemInSlot(int slotIndex, int amount)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            Slot slot = GetSlot(slotIndex);
            
            if (amount <= slot.amount)
            {
                GetSlot(slotIndex).amount -= amount;
                
                if (slot.amount <= 0)
                {
                    slot.item = null;
                }
                
                OnHotbarChanged?.Invoke();
            }
            else
            {
                throw new Exception("You cannot spend more than " + amount + " items");
            }
        }
    }

    public Slot GetSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
            return slots[slotIndex];
        
        return null;
    }

    public ItemConfig GetItemInSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
            return slots[slotIndex].item;
        
        return null;
    }

    public bool TryUseSelectedItem()
    {
        Slot itemSlot = GetSlot(selectedSlotIndex);

        if (itemSlot.item != null)
        {
            if (itemSlot.item is BlockConfig block)
            {
                if (G.BlockPlacing.TryPlace(block.type))
                {
                    SpendItemInSlot(selectedSlotIndex, 1);
                    return true;
                }
            }
        }
        
        return false;
    }
}
