# Input Architecture

## Local Reference

Read `Library/PackageCache/com.unity.inputsystem@*/Documentation~/Architecture.md` before input architecture changes.

## Project Rules

- Use new Input System only.
- Do not use old `UnityEngine.Input.*`.
- `activeInputHandler` is New only.
- Gameplay should depend on mechanic-level input meanings, not raw devices.
- Prefer high-level `InputAction`/callback style for gameplay actions.
- Keep bindings, processors, and interactions near input setup or input adapters.
- Keep raw device/control reads at adapters/edges unless a feature specifically needs device-level behavior.
- Input callbacks should translate input into service calls or view feedback, not contain gameplay rules.
- Avoid allocations in input paths.
- Centralize input blocking for ads, menus, tutorials, and loading instead of scattering flags.

## Key Files

- `Assets/_Project/_Scripts/Systems/PlayerMovement/InputSystem/InputSystem_Actions.inputactions`
- `Assets/_Project/_Scripts/Systems/PlayerMovement/InputSystem/InputSystem_Actions.cs`
- `Assets/_Project/_Scripts/Systems/PlayerMovement/InputSystem/PlayerInput.cs`
- `Assets/_Project/_Scripts/Systems/PlayerMovement/InputSystem/DesktopPlayerInputSource.cs`
- `Assets/_Project/_Scripts/Systems/PlayerMovement/MobilePlayerInputSource.cs`

## Generated Code

Generated `InputSystem_Actions.cs` should be regenerated from `.inputactions`, not hand-edited unless the task explicitly requires an emergency patch.
