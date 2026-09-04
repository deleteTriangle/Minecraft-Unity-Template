using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarInput : MonoBehaviour
{
    private InputAction[] slotActions;

    public void Init(int slotsCount)
    {
        G.PlayerInput.InputActions.Player.SelectSlot1.performed += _ => G.Hotbar.SelectSlot(0);
        G.PlayerInput.InputActions.Player.SelectSlot2.performed += _ => G.Hotbar.SelectSlot(1);
        G.PlayerInput.InputActions.Player.SelectSlot3.performed += _ => G.Hotbar.SelectSlot(2);
        G.PlayerInput.InputActions.Player.SelectSlot4.performed += _ => G.Hotbar.SelectSlot(3);
        G.PlayerInput.InputActions.Player.SelectSlot5.performed += _ => G.Hotbar.SelectSlot(4);
        G.PlayerInput.InputActions.Player.SelectSlot6.performed += _ => G.Hotbar.SelectSlot(5);
        G.PlayerInput.InputActions.Player.SelectSlot7.performed += _ => G.Hotbar.SelectSlot(6);
        G.PlayerInput.InputActions.Player.SelectSlot8.performed += _ => G.Hotbar.SelectSlot(7);
        G.PlayerInput.InputActions.Player.SelectSlot9.performed += _ => G.Hotbar.SelectSlot(8);
        
        G.PlayerInput.InputActions.Player.ScrollSlot.performed += ScrollHotbar;
    }

    private void ScrollHotbar(InputAction.CallbackContext ctx)
    {
        Vector2 scroll = ctx.ReadValue<Vector2>();
    
        if (scroll.y > 0)
            G.Hotbar.SelectSlot(G.Hotbar.selectedSlotIndex - 1);
        else if (scroll.y < 0)
            G.Hotbar.SelectSlot(G.Hotbar.selectedSlotIndex + 1);
    }
}
