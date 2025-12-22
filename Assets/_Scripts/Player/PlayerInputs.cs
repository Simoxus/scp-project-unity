using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset playerInputAsset;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public bool BlinkHeld { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }

    private bool _blinkPressed;
    private bool _crouchPressed;

    public bool BlinkPressed
    {
        get
        {
            bool val = _blinkPressed;
            _blinkPressed = false;
            return val;
        }
    }

    public bool CrouchPressed
    {
        get
        {
            bool val = _crouchPressed;
            _crouchPressed = false;
            return val;
        }
    }

    public event Action OnBlink;
    public event Action OnInteract;
    public event Action OnPauseUI;
    public event Action OnDebugUI;
    public event Action<string> OnKeypadInput;

    private InputContextManager _contexts;
    private InputBindingManager _bindings;

    private readonly Dictionary<string, InputAction> _actions = new();
    private readonly List<Action> _unbindCallbacks = new();

    public void AddContext(InputContext ctx)
    {
        if (_contexts == null) return;
        _contexts.EnableContext(ctx);
    }

    public void RemoveContext(InputContext ctx)
    {
        if (_contexts == null) return;
        _contexts.DisableContext(ctx);
    }

    public void SetSingleContext(InputContext ctx)
    {
        if (_contexts == null) return;
        _contexts.DisableAll();
        _contexts.EnableContext(ctx);
    }

    public IReadOnlyCollection<InputContext> ActiveContexts => _contexts?.ActiveContexts;

    public bool HasContext(InputContext ctx)
    {
        if (_contexts == null) return false;
        return _contexts.IsContextActive(ctx);
    }

    public void EnableGameplayInputs() => AddContext(InputContext.Gameplay);
    public void EnableUIInputs() => AddContext(InputContext.UI);
    public void EnableKeypadInputs() => AddContext(InputContext.Keypad);

    public void DisableGameplayInputs() => RemoveContext(InputContext.Gameplay);
    public void DisableUIInputs() => RemoveContext(InputContext.UI);
    public void DisableKeypadInputs() => RemoveContext(InputContext.Keypad);

    private void Awake()
    {
        if (playerInputAsset == null)
        {
            Log.Error("InputActionAsset is not assigned in the inspector!");
            enabled = false;
            return;
        }

        _contexts = new InputContextManager(playerInputAsset);
        _bindings = new InputBindingManager(playerInputAsset);

        CacheAction("Player/Move");
        CacheAction("Player/Look");
        CacheAction("Player/Sprint");
        CacheAction("Player/Crouch");
        CacheAction("Player/Blink");
        CacheAction("Player/Interact");

        CacheAction("Menus/PauseUI");
        CacheAction("Menus/DebugUI");

        CacheAction("Keypad/Number");
        CacheAction("Keypad/Enter");
        CacheAction("Keypad/Clear");

        BindAll();
    }

    private void Start()
    {
        _contexts.EnableContext(InputContext.Gameplay);
        _contexts.EnableContext(InputContext.Menus);
    }

    private void OnEnable()
    {
        if (_contexts != null && !_contexts.IsContextActive(InputContext.Gameplay))
            _contexts.EnableContext(InputContext.Gameplay);
    }

    private void OnDisable()
    {
        _contexts?.DisableAll();
    }

    private void OnDestroy()
    {
        foreach (var unbind in _unbindCallbacks)
            unbind?.Invoke();

        _unbindCallbacks.Clear();
    }

    private void Update()
    {
        MoveInput = Read<Vector2>("Player/Move");
        LookInput = Read<Vector2>("Player/Look");
    }

    private void BindAll()
    {
        BindHold("Player/Sprint", v => SprintHeld = v);
        BindHold("Player/Crouch", v => CrouchHeld = v, onPress: () => _crouchPressed = true);
        BindHold("Player/Blink", v =>
        {
            BlinkHeld = v;
            if (v)
            {
                _blinkPressed = true;
                OnBlink?.Invoke();
            }
        });
        BindPress("Player/Interact", () => OnInteract?.Invoke());

        BindPress("Menus/PauseUI", () => OnPauseUI?.Invoke());
        BindPress("Menus/DebugUI", () => OnDebugUI?.Invoke());

        BindPress("Keypad/Number", ctx => OnKeypadInput?.Invoke(ctx.control.name));
        BindPress("Keypad/Enter", () => OnKeypadInput?.Invoke("Enter"));
        BindPress("Keypad/Clear", () => OnKeypadInput?.Invoke("Clear"));
    }

    private void CacheAction(string path)
    {
        var split = path.Split('/');
        if (split.Length != 2)
        {
            Log.Warning($"Invalid action path format: {path}");
            return;
        }

        var map = playerInputAsset.FindActionMap(split[0]);
        if (map == null)
        {
            Log.Warning($"Action map not found: {split[0]}");
            return;
        }

        var action = map.FindAction(split[1]);
        if (action == null)
        {
            Log.Warning($"Action not found: {path}");
            return;
        }

        _actions[path] = action;
    }

    private T Read<T>(string path) where T : struct
        => _actions.TryGetValue(path, out var act) ? act.ReadValue<T>() : default;

    private void BindHold(string path, Action<bool> onHold, Action onPress = null)
    {
        if (!_actions.TryGetValue(path, out var a)) return;

        Action<InputAction.CallbackContext> performedHandler = ctx =>
        {
            onHold(true);
            onPress?.Invoke();
        };
        Action<InputAction.CallbackContext> canceledHandler = ctx => onHold(false);

        a.performed += performedHandler;
        a.canceled += canceledHandler;

        _unbindCallbacks.Add(() =>
        {
            a.performed -= performedHandler;
            a.canceled -= canceledHandler;
        });
    }

    private void BindPress(string path, Action onPress)
    {
        if (!_actions.TryGetValue(path, out var a)) return;

        Action<InputAction.CallbackContext> handler = ctx => onPress?.Invoke();
        a.performed += handler;

        _unbindCallbacks.Add(() => a.performed -= handler);
    }

    private void BindPress(string path, Action<InputAction.CallbackContext> onPress)
    {
        if (!_actions.TryGetValue(path, out var a)) return;

        a.performed += onPress;
        _unbindCallbacks.Add(() => a.performed -= onPress);
    }
}

public enum InputContext
{
    Gameplay,
    Keypad,
    Menus,
    UI
}

public class InputContextManager
{
    private readonly InputActionAsset _asset;
    private readonly Dictionary<InputContext, InputActionMap> _maps = new();
    private readonly HashSet<InputContext> _activeContexts = new();

    public IReadOnlyCollection<InputContext> ActiveContexts => _activeContexts;

    public InputContextManager(InputActionAsset asset)
    {
        _asset = asset;

        _maps[InputContext.Gameplay] = _asset.FindActionMap("Player");
        _maps[InputContext.Keypad] = _asset.FindActionMap("Keypad");
        _maps[InputContext.Menus] = _asset.FindActionMap("Menus");
        _maps[InputContext.UI] = _asset.FindActionMap("UI");
    }

    public void EnableContext(InputContext ctx)
    {
        if (_maps.TryGetValue(ctx, out var map) && map != null)
        {
            map.Enable();
            _activeContexts.Add(ctx);
        }
    }

    public void DisableContext(InputContext ctx)
    {
        if (_maps.TryGetValue(ctx, out var map) && map != null)
        {
            map.Disable();
            _activeContexts.Remove(ctx);
        }
    }

    public void DisableAll()
    {
        foreach (var map in _maps.Values)
        {
            if (map != null)
                map.Disable();
        }
        _activeContexts.Clear();
    }

    public bool IsContextActive(InputContext ctx) => _activeContexts.Contains(ctx);
}

public class InputBindingManager
{
    private readonly InputActionAsset _asset;

    public InputBindingManager(InputActionAsset asset)
    {
        _asset = asset;
    }

    public string GetDisplayString(string path)
    {
        var action = FindAction(path);
        return action != null ? action.GetBindingDisplayString() : "Unbound";
    }

    private InputAction FindAction(string path)
    {
        var parts = path.Split('/');
        if (parts.Length != 2) return null;

        var map = _asset.FindActionMap(parts[0]);
        return map?.FindAction(parts[1]);
    }
}