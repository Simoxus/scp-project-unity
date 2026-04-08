using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [Space]
    [SerializeField] private InputActionAsset playerInputAsset;
    public InputActionAsset PlayerInputAsset => playerInputAsset;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public bool BlinkHeld { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }

    // Freecam inputs
    public Vector2 FreecamMoveInput { get; private set; }
    public Vector2 FreecamLookInput { get; private set; }
    public bool FreecamAccelerateHeld { get; private set; }
    public bool FreecamDecelerateHeld { get; private set; }
    public bool FreecamUpHeld { get; private set; }
    public bool FreecamDownHeld { get; private set; }
    public float FreecamZoomInput { get; private set; }
    public bool FreecamZoomModifierHeld { get; private set; }

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
    public event Action OnUseItem;
    public event Action OnUnequipItem;
    public event Action OnQuickSave;
    public event Action OnQuickLoad;
    public event Action OnDebugUI;
    public event Action OnPauseUI;
    public event Action OnInventoryUI;
    public event Action<string> OnKeypadInput;
    public event Action OnFreecamLock;
    public event Action OnFreecamSmooth;
    public event Action OnFreecamWobble;
    public event Action OnFreecamPause;
    public event Action OnFreecamTutorial;

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
    public void EnableFreecamInputs() => AddContext(InputContext.Freecam);

    public void DisableGameplayInputs() => RemoveContext(InputContext.Gameplay);
    public void DisableUIInputs() => RemoveContext(InputContext.UI);
    public void DisableKeypadInputs() => RemoveContext(InputContext.Keypad);
    public void DisableFreecamInputs() => RemoveContext(InputContext.Freecam);

    private void Awake()
    {
        if (playerInputAsset == null)
        {
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
        CacheAction("Player/Use");
        CacheAction("Player/Unequip");
        CacheAction("Player/Quick Save");
        CacheAction("Player/Quick Load");

        CacheAction("Menus/DebugUI");
        CacheAction("Menus/PauseUI");
        CacheAction("Menus/InventoryUI");

        CacheAction("Keypad/Number");
        CacheAction("Keypad/Enter");
        CacheAction("Keypad/Clear");

        CacheAction("Freecam/Hide");
        CacheAction("Freecam/Move");
        CacheAction("Freecam/Look");
        CacheAction("Freecam/Zoom");
        CacheAction("Freecam/ZoomModifier");
        CacheAction("Freecam/Accelerate");
        CacheAction("Freecam/Decelerate");
        CacheAction("Freecam/Up");
        CacheAction("Freecam/Down");
        CacheAction("Freecam/Lock");
        CacheAction("Freecam/Smooth");
        CacheAction("Freecam/Wobble");
        CacheAction("Freecam/Pause");

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
        // Check if Player/Freecam inputs should be blocked
        bool shouldBlockGameplay = (Core.GameManager != null && Core.GameManager.gamePaused) ||
                                   (Core.GameManager != null && Core.GameManager.disablePlayerInputs);

        // Manage Player action map state
        bool isPlayerMapActive = _contexts.IsContextActive(InputContext.Gameplay);
        if (shouldBlockGameplay && isPlayerMapActive)
        {
            _contexts.DisableContext(InputContext.Gameplay);
        }
        else if (!shouldBlockGameplay && !isPlayerMapActive)
        {
            _contexts.EnableContext(InputContext.Gameplay);
        }

        // Manage Freecam action map state
        // Freecam should be active when freecam mode is enabled, but blocked if game is paused by something OTHER than freecam
        bool isFreecamMapActive = _contexts.IsContextActive(InputContext.Freecam);
        bool isInFreecamMode = Core.Player?.Freecam != null && Core.Player.Freecam.IsFreecamActive;

        bool isFreecamPaused = Core.GameManager != null &&
                               Core.GameManager.HasPauseRequest(Core.Player.Freecam) &&
                               Core.GameManager.pauseRequestCount == 1;
        bool shouldFreecamBeBlocked = Core.GameManager != null && Core.GameManager.gamePaused && !isFreecamPaused;

        bool shouldFreecamBeActive = isInFreecamMode && !shouldFreecamBeBlocked;

        if (shouldFreecamBeActive && !isFreecamMapActive)
        {
            _contexts.EnableContext(InputContext.Freecam);
        }
        else if (!shouldFreecamBeActive && isFreecamMapActive)
        {
            _contexts.DisableContext(InputContext.Freecam);
        }

        // Read input values (these will be zero if maps are disabled)
        MoveInput = Read<Vector2>("Player/Move");
        LookInput = Read<Vector2>("Player/Look");

        FreecamMoveInput = Read<Vector2>("Freecam/Move");
        FreecamLookInput = Read<Vector2>("Freecam/Look");
        FreecamZoomInput = Read<float>("Freecam/Zoom");
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
        BindPress("Player/Use", () => OnUseItem?.Invoke());
        BindPress("Player/Unequip", () => OnUnequipItem?.Invoke());

        BindPress("Player/Quick Save", () => OnQuickSave?.Invoke());
        BindPress("Player/Quick Load", () => OnQuickLoad?.Invoke());

        BindPress("Menus/DebugUI", () => OnDebugUI?.Invoke());
        BindPress("Menus/PauseUI", () => OnPauseUI?.Invoke());
        BindPress("Menus/InventoryUI", () => OnInventoryUI?.Invoke());

        BindPress("Keypad/Number", ctx => OnKeypadInput?.Invoke(ctx.control.name));
        BindPress("Keypad/Enter", () => OnKeypadInput?.Invoke("Enter"));
        BindPress("Keypad/Clear", () => OnKeypadInput?.Invoke("Clear"));

        BindPress("Freecam/Hide", () => OnFreecamTutorial?.Invoke());
        BindHold("Freecam/ZoomModifier", v => FreecamZoomModifierHeld = v);
        BindHold("Freecam/Accelerate", v => FreecamAccelerateHeld = v);
        BindHold("Freecam/Decelerate", v => FreecamDecelerateHeld = v);
        BindHold("Freecam/Up", v => FreecamUpHeld = v);
        BindHold("Freecam/Down", v => FreecamDownHeld = v);
        BindPress("Freecam/Lock", () => OnFreecamLock?.Invoke());
        BindPress("Freecam/Smooth", () => OnFreecamSmooth?.Invoke());
        BindPress("Freecam/Wobble", () => OnFreecamWobble?.Invoke());
        BindPress("Freecam/Pause", () => OnFreecamPause?.Invoke());
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
            Log.Error($"Action map not found: {split[0]}");
            return;
        }

        var action = map.FindAction(split[1]);
        if (action == null)
        {
            Log.Error($"Action not found: {path}");
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

    public InputAction GetAction(string path)
    {
        if (playerInputAsset == null) return null;

        var parts = path.Split('/');
        if (parts.Length != 2) return null;

        var actionMap = playerInputAsset.FindActionMap(parts[0]);
        return actionMap?.FindAction(parts[1]);
    }

    public void SetActionEnabled(string path, bool enabled)
    {
        if (_actions.TryGetValue(path, out var action))
        {
            if (enabled)
            {
                action.Enable();
            }
            else
            {
                action.Disable();
            }
        }
    }
}

public enum InputContext
{
    Gameplay,
    Keypad,
    Menus,
    UI,
    Freecam
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
        _maps[InputContext.Freecam] = _asset.FindActionMap("Freecam");
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