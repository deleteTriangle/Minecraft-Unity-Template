# Code Style


## Naming

- Use `PascalCase` for classes, interfaces, methods, properties, events, public fields.
- Prefix interfaces with `I`, for example `IUsable`.
- Use `_camelCase` for private fields, for example `_selectedIndex`.
- Use `camelCase` for parameters and local variables.
- Prefer clear mechanic-oriented names like `hotbar`, `selectedIndex` over abbreviations.

Examples:

```csharp
public class HotbarView : MonoBehaviour
public interface IUsable

private int _lastSelectedIndex = -1;

public Hotbar Hotbar;
[SerializeField] private TMP_Text _indexText;
```

## File Rules

- One type per file.
- File name must match the main type name.
- Keep related mechanic files in the same system folder, for example `Systems/Hotbar/`.

## Class Shape

- Keep classes small and focused.
- Prefer plain C# classes for data and logic.
- Use `MonoBehaviour` only for scene-bound view/reference code.
- Keep interfaces minimal and behavior-focused.

## Fields And Properties

- Avoid public fields. Use `[SerializeField] private ...` for values and references that must be assigned in the Inspector.
- Use properties only for read-only access, encapsulation, or validation.
- If a field does not need property logic, keep it as a field.
- Use private backing fields plus read-only properties when runtime state should not be writable from outside.
- Prefer auto-properties or expression-bodied getters for trivial read-only access.

Example:

```csharp
private int selectedIndex;
public int SelectedIndex
{
    get => selectedIndex;
    set => selectedIndex = value > 0 ? value : 0;
}
```

```csharp
[SerializeField] private List<HotbarSlotView> _slotViews;
[SerializeField] private Transform _container;
```

```csharp
[SerializeField] private HotbarSlotView slotPrefab;
```

## Member Order

- Order class members as follows: public fields, protected fields, `[SerializeField] private` fields, private fields, constructors, public properties, protected properties, private properties, `Init()` (when present), Unity methods (`Awake`, `Start`, `Update`, and so on), then other methods with public methods first.

## Methods

- Keep methods short.
- Use expression-bodied members for trivial one-line methods.
- Use full block bodies when there is branching, validation, or side effects.
- Prefer early validation for invalid input.
- If invalid external usage should fail, fail explicitly instead of masking it.

Examples:

```csharp
public HotbarSlot GetSelectedSlot() => GetSlot(selectedIndex);
```

```csharp
public void AddItem(IUsable item)
{
    if (item == null) throw new Exception("Item is null");

    _slots.Add(new HotbarSlot(item));
    OnHotbarChanged?.Invoke();
}
```

## Events And Flow

- Use `Action` and `Action<T>` for lightweight mechanic events.
- Name events with `On...`, for example `OnSlotSelected`, `OnHotbarChanged`.
- Invoke events with null-propagation: `OnHotbarChanged?.Invoke();`
- Views should react to model/service events instead of polling.

## Collections

- Use `List<T>` as the default mutable collection unless a different structure is justified.
- Initialize collections at declaration when they are owned by the class.

## Unity-Specific Style

- Put `using` directives at the top of the file.
- Keep Unity API usage near view code or system edges.
- In view scripts, prefer explicit methods like `Init()`, `UpdateSlotsView()`, `UpdateSelectedView()` over hidden setup spread across Unity callbacks.
- Use serialized scene references directly instead of runtime lookups when possible.

Examples:

```csharp
public void Init()
{
    hotbarView.Init();

    hotbar.OnHotbarChanged += hotbarView.UpdateSlotsView;
}
```

## Performance

- Do not call `GetComponent` repeatedly in hot paths. Cache component references.
- Prefer wiring references through fields, constructors, `Init()`, or Inspector assignment.
- Use `CompareTag()` instead of `tag == "TagName"`.
- Use `Update()` for input and frame-based logic.
- Use `FixedUpdate()` for physics-related work.
- Use object pooling for objects that are instantiated and destroyed frequently.
- Avoid unnecessary allocations inside loops and frequently called methods.

Examples:

```csharp
private Rigidbody rb;

public void Init()
{
    rb = GetComponent<Rigidbody>();
}
```

```csharp
if (other.CompareTag("Player"))
{
    //
}
```

## Formatting

- Keep a blank line between logically separate members or blocks.
- Use inline conditional operators only when immediately readable.

## Comments

- Do not add comments.

## What To Avoid

- vague names
- large multi-purpose classes
- hidden cross-system dependencies
- unnecessary abstractions
- mixing gameplay logic into `MonoBehaviour` views
- adding complexity when a simple list, event, or helper method is enough
- repeated `GetComponent` calls in frequently executed code
- `tag == "Player"` checks
- physics code in `Update()`
- frequent instantiate/destroy cycles for reusable runtime objects
