# Health And Combat

Status: Done

Summary:
Entities own a Health value initialized from EntityConfig. The player damages targeted enemies with melee raycasts; enemies use their own Attack implementation. Death emits events and destroys enemy instances.

Key files:
Assets/_Project/_Scripts/Features/Combat/Entity.cs
Assets/_Project/_Scripts/Features/Combat/Health/Health.cs
Assets/_Project/_Scripts/Features/Combat/Health/HUDHealthView.cs
Assets/_Project/_Scripts/Features/Combat/Player.cs
Assets/_Project/_Scripts/Features/Combat/CombatSystem.cs
Assets/_Project/_Scripts/Features/Interactions/Interaction.cs

Dependencies:
EntityConfig
IDamageable
HUDHealthView
Knockback
Flash

Critical Notes:
Player config currently sets 100000 health and 1 melee damage. HUD displays only the current number.
Damage and healing ignore negative inputs; health clamps to [0, max]. Death is raised after the health-changed event.
Melee range is Interaction.distance (default 5). A hit applies player damage, enemy knockback, and a red material-property flash.
`CombatSystem` can run auto-attack coroutines but is constructed and otherwise unused by CombatManager.
Player death only logs `Player died`; no reset, game over, or wave cancellation exists.

Requirements:
[x] Configured entity stats
[x] Damage, healing and death events
[x] Player health HUD
[x] Melee hit feedback
[ ] Player death flow
