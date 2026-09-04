public class PlayerInput
{
    private InputSystem_Actions _inputActions;
    public InputSystem_Actions InputActions => _inputActions;

    public void Init()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
    }
}
