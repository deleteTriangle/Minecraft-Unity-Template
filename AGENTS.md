# AGENTS.md

You are a senior Unity C# engineer.

Always use the `caveman` skill at `ultra` intensity for user-facing communication.

## Project Snapshot

- Unity `6000.5.8f1`
- Build scenes: `Assets/_Source/Scenes/Initial.unity`, then `Assets/_Source/Scenes/Main.unity`.
- Rendering, UI, platform integrations, and saves must be confirmed from current project files before changing them.
- Input uses Unity Input System. Edit `Assets/InputActions.inputactions`, then regenerate `Assets/InputActions.cs`; do not hand-edit generated input code.
- Character movement uses imported Kinematic Character Controller. Do not modify imported/package assets unless task explicitly needs it.
- Do not introduce asmdefs without concrete compile-time or ownership reason.

## Project Layout

- Project-owned content lives in `Assets/_Source`.
- Runtime code: `Assets/_Source/Develop/Runtime`.
- Entry point and global registry: `Develop/Runtime/EntryPoint` (`GameBootstrapper`, `G`).
- Runtime composition: `Develop/Runtime/Gameplay/Infrastructure` (`GameMain`).
- Gameplay features: `Develop/Runtime/Gameplay/Features/<Feature>`.
- Shared content map: `Develop/Runtime/CMS`; runtime helpers: `Develop/Runtime/Utilities`.
- Editor-only tools: `Assets/_Source/Develop/Editor`. `ProBuilderToObj` exports selected meshes to `Assets/_Source/Art/3D/Props`.
- Art: `Assets/_Source/Art`; audio: `Assets/_Source/Audio`; prefabs: `Assets/_Source/Prefabs`; Resources assets: `Assets/_Source/Resources`; scenes: `Assets/_Source/Scenes`.
- Most project types use global namespace. Do not mass-add namespaces or move types as cleanup.

## Composition

- `GameBootstrapper.Awake()` is startup entry. It initializes `R`, preserves `servicesHolder`, creates and initializes `SceneLoader`, then initializes `GameMain`.
- `GameMain.Init()` owns scene-level startup: it instantiates and initializes `FirstPersonController`, preserves it across scenes, then loads `_Source/Scenes/Main` through `G.sceneLoader`.
- `G` is current global registry. Add only explicit, necessary cross-feature references; do not introduce a new singleton, locator, DI container, installer, or service framework.
- Add scene-bound services as components under `servicesHolder` through established bootstrapper pattern. Keep feature-specific logic in feature folder.
- Use `MonoBehaviour` for scene objects, component references, and Unity callbacks. Avoid new per-frame loops unless feature needs frame work.

## Input

- Keep raw action reads in `PlayerController/PlayerInput`. Gameplay code consumes mechanic-level values (`Move`, `Look`, sprint, jump buffer), not device reads.
- Preserve current `InputActions` lifecycle: create and enable in `PlayerInput.Init()`.
- Read `.agents/rules/input_architecture.md` before changing input.

## Local Guidance

Read only guidance relevant to task:

- `.agents/rules/code_style.md` for every C# change.
- `.agents/rules/input_architecture.md` before input changes.
- `.agents/rules/saves_yandex.md` before save or Yandex changes.
- `.agents/rules/architecture.md` before changing bootstrap, registry, loading, or feature composition.

Do not read obsolete architecture files if they reappear. Current source code and the files above win over stale notes.

## Engineering Rules

- No comments in project code.
- Follow `.agents/rules/code_style.md`: one main type per file, matching file/type names, PascalCase public APIs, camelCase locals/fields, `On...` `Action` events, and no hidden cross-system dependencies.
- Keep changes surgical. Match local style; no unrelated refactors, speculative abstractions, new framework layers, or broad namespace/formatting churn.
- Unity YAML assets, prefabs, and scenes are serialization-sensitive. Use Unity CLI for Unity Editor interaction, validation, and asset/scene work. Do not casually mass-edit GUIDs, file IDs, or `.meta` files.
- Do not modify generated input code or imported/vendor code unless task explicitly asks.
- Preserve user changes in dirty files. Remove only code made unused by your change.
- Before implementation, state assumptions when task is materially ambiguous. Ask instead of guessing when choice changes gameplay, data compatibility, or scene wiring.

## Implementation Workflow

- Before writing code, identify implementation questions that materially affect behavior, gameplay, data compatibility, scene wiring, or user experience. Ask the user and wait for an answer when such questions exist; do not guess.
- Before implementation, provide a TO-DO checklist covering the intended work and verification. Keep it current by marking items complete as they are finished.
- Consider a mechanic complete only when every applicable TO-DO item, including required verification, is complete. Clearly report any unchecked item or blocked validation; do not present the mechanic as finished.

## Verification

- Prefer focused checks. Project-owned automated test coverage is not established; use Unity compile/play-mode validation for scene, prefab, and gameplay work when environment permits.
- For input changes, test desktop and mobile paths plus input blocking where implemented.
- State clearly when Unity-only validation was not run.

## Durable Knowledge

Update a relevant `.agents/memory_bank` entry only when work changes durable architecture, feature behavior, save schema, platform integration, or workflow constraints. Keep it concise and factual.
