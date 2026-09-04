# Player Movement

Status: Done

Summary:
The active first-person controller is Rigidbody-based. It supports mouse look, walking, sprinting, jumping, crouching, zoom, head bob, custom gravity, stepping over low obstacles, and downward snapping.

Key files:
Assets/_Project/_Scripts/Features/CharacterMovement/RigidbodyController.cs
Assets/_Project/_Scripts/Features/CharacterMovement/PlayerInput.cs
Assets/_Project/_Scripts/Features/CharacterMovement/InputSystem/InputSystem_Actions.inputactions
Assets/_Project/Prefabs/Player/FirstPersonController.prefab

Dependencies:
Rigidbody
BoxCollider
Camera
Input System
Ground layer mask
G.PlayerInput

Critical Notes:
`EntryPoint` initializes `RigidbodyController`; `FirstPersonController.cs` is an unused alternative implementation.
Movement uses `Rigidbody.linearVelocity`, disables built-in gravity, and applies its own gravity in FixedUpdate.
Ground detection casts from the center and four collider corners; the configured `groundMask` must include terrain colliders.
Crouching prevents standing when a ceiling raycast hits, and also blocks motion off unsupported edges.
Sprint may be unlimited or use duration/cooldown; zoom is disabled while sprinting.

Requirements:
[x] Walk, sprint, crouch and jump
[x] Mouse look and FOV effects
[x] Step-up and snap-down on voxel terrain
[x] Head bob and zoom
