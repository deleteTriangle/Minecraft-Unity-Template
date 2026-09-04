# Architecture

## Scope

- Project code and assets belong under `Assets/_Project/`.
- Main code path is `Assets/_Project/_Scripts`.
- Do not edit imported packages/plugins unless task explicitly requires it.

## Composition And Lifecycle

- `GameBootstrapper.Awake()` is sole `Awake()` in project composition.
- `GameBootstrapper` calls `GameMain.Init()`.
- `GameMain` is composition root: it owns serialized scene refs, creates services, calls `Init()`, and registers frame callbacks in `GameLoop`.
- Services are plain C# classes with explicit `Init()` and, where needed, `Dispose()`. Do not add `IService`, `ITickable`, feature installers, or another composition root.
- Add frame work through `GameLoop.Register(...)`; unregister callbacks and event listeners during disposal when lifetime requires it.

## Dependencies

- `G` is global cross-service registry. Keep it; do not introduce other singleton patterns.
- Use constructors and `Init()` parameters for dependencies inside one mechanic. Use `G` only for cross-system access.
- Keep gameplay logic out of `MonoBehaviour` views and adapters.
- Use assigned scene references over hierarchy searches and repeated `GetComponent` calls.

## UI And Data

- Use uGUI and TMP for UI.
- Put tuning data in ScriptableObjects under `Assets/_Project/Gameplay Settings`.
- Keep UI event-driven where practical; views render state and send user intent to services.

## Project Standards

- Use Unity Input System only. Read `input_architecture.md` for input work.
- Read `saves_yandex.md` for save or Yandex work.
- Read memory-bank entries only for mechanics touched by task.
- Do not add code comments.

## Core Principles

- DRY: keep one source of truth; extract repeated inventory update logic into one method.
- KISS: prefer the simplest working design; use a `List<T>` when it is sufficient.
- YAGNI: add functionality only when it is needed; do not create an unused save abstraction.
- SOLID: give each class one responsibility and depend on focused interfaces; keep `HotbarView` rendering separate from hotbar logic.
