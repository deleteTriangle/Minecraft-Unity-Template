using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarView : MonoBehaviour
{
    private Hotbar hotbar;

    public SlotView[] slots;
    
    public void Init()
    {
        hotbar = G.Hotbar;

        hotbar.OnHotbarChanged += UpdateSlotsView;
        UpdateSlotsView();
    }

    public void UpdateSlotsView()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            bool isSelected = hotbar.selectedSlotIndex == i;
            Slot slot = hotbar.GetSlot(i);
            
            slots[i].UpdateView(isSelected, slot.item, slot.amount);
        }
    }
}
