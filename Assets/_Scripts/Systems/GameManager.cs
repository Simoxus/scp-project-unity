using EditorAttributes;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public bool IsInMainMenu => SceneManager.GetActiveScene().name == "MainMenu";

    [Space]
    public bool gamePaused = false;
    public bool disablePlayerInputs = false;
    public bool hidePlayerHUD = false;

    [Header("Inherited QOL")]
    public bool inventoryPausesGame;
    public bool skipIntroSequence;
    public bool cameraShaking = true;

    public event Action<bool, object> OnPauseStateChanged;

    [ReadOnly] public int pauseRequestCount = 0;
    [ReadOnly] public int disableControlsRequestCount = 0;

    // Variables that track which scripts have requested pause/disable
    private readonly HashSet<object> _pauseRequesters = new HashSet<object>();
    private readonly HashSet<object> _disableControlsRequesters = new HashSet<object>();
    private readonly HashSet<object> _cursorControlRequesters = new HashSet<object>();

    private void Reset()
    {
        _pauseRequesters.Clear();
        _disableControlsRequesters.Clear();
        _cursorControlRequesters.Clear();
    }

    protected override void OnSingletonAwake()
    {
        Time.timeScale = 1.0f;
        gamePaused = false;
        pauseRequestCount = 0;
        disableControlsRequestCount = 0;
    }

    private void Start()
    {
        // Disable annoying ass PrimeTween warns
        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnZeroDuration = false;
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.SetTweensCapacity(400);

        if (IsInMainMenu)
        {
            Log.VerboseInfo("GameManager detected player is in MainMenu scene. Ensuring that cursor is shown.");
            UpdateCursorVisiblity(forceDisable: false);
        }
        else
        {
            RequestDisableControls(this, shouldDisable: disablePlayerInputs);
        }
    }

    public void RequestPause(object requester)
    {
        if (requester == null)
        {
            Log.VerboseWarning("RequestPause was called, but with no requester provided!");
            return;
        }

        if (_pauseRequesters.Add(requester))
        {
            pauseRequestCount = _pauseRequesters.Count;

            if (pauseRequestCount == 1)
            {
                ApplyPauseState(true, requester);
            }
        }
    }

    public void ReleasePause(object requester)
    {
        if (requester == null)
        {
            Log.VerboseWarning("ReleasePause was called, but with no requester provided!");
            return;
        }

        if (_pauseRequesters.Remove(requester))
        {
            pauseRequestCount = _pauseRequesters.Count;

            if (pauseRequestCount == 0 && gamePaused)
            {
                ApplyPauseState(false, requester);
            }
        }
    }

    public void RequestCursorControl(object requester)
    {
        if (requester == null)
        {
            Log.VerboseWarning("RequestCursorControl was called, but with no requester provided!");
            return;
        }

        if (_cursorControlRequesters.Add(requester))
        {
            // When first requester takes control, show and unlock cursor by default
            if (_cursorControlRequesters.Count == 1)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void ReleaseCursorControl(object requester)
    {
        if (requester == null)
        {
            Log.VerboseWarning("ReleaseCursorControl was called, but with no requester provided!");
            return;
        }

        if (_cursorControlRequesters.Remove(requester))
        {
            if (_cursorControlRequesters.Count == 0)
            {
                UpdateCursorVisiblity(); // Restore normal cursor behavior
            }
        }
    }

    /// <summary>
    /// Allows manual control of cursor state. Only works if requester has already called RequestCursorControl.
    /// </summary>
    public void SetCursorState(object requester, bool visible, CursorLockMode lockMode = CursorLockMode.None)
    {
        if (requester == null)
        {
            Log.VerboseWarning("SetCursorState was called, but with no requester provided!");
            return;
        }

        if (_cursorControlRequesters.Contains(requester))
        {
            Cursor.visible = visible;
            Cursor.lockState = lockMode;
        }
        else
        {
            Log.VerboseWarning($"SetCursorState was called by {requester}, but they haven't requested cursor control!");
        }
    }

    private void ApplyPauseState(bool shouldPause, object requester = null)
    {
        gamePaused = shouldPause;

        if (Core.AudioManager != null) Core.AudioManager.ToggleGameSounds(gamePaused);
        Time.timeScale = gamePaused ? 0f : 1.0f;

        RequestDisableControls(this, shouldDisable: gamePaused);
        UpdateCursorVisiblity();

        OnPauseStateChanged?.Invoke(shouldPause, requester);
    }

    public void RequestDisableControls(object requester, bool shouldDisable, bool updateCursor = true)
    {
        if (requester == null)
        {
            Log.VerboseWarning("RequestDisableControls was called, but with no requester provided!");
            return;
        }

        bool wasInList = _disableControlsRequesters.Contains(requester);
        bool stateChanged = false;

        if (shouldDisable)
        {
            if (_disableControlsRequesters.Add(requester))
            {
                stateChanged = _disableControlsRequesters.Count == 1;
            }
        }
        else
        {
            if (_disableControlsRequesters.Remove(requester))
            {
                stateChanged = _disableControlsRequesters.Count == 0;
            }
        }

        disableControlsRequestCount = _disableControlsRequesters.Count;

        if (stateChanged)
        {
            TogglePlayerControls(shouldDisable: disableControlsRequestCount > 0);

            if (updateCursor)
            {
                UpdateCursorVisiblity();
            }
        }
    }

    public void ForceResetPauseState()
    {
        _pauseRequesters.Clear();
        _disableControlsRequesters.Clear();
        _cursorControlRequesters.Clear();
        pauseRequestCount = 0;
        disableControlsRequestCount = 0;

        ApplyPauseState(false);
    }

    public bool HasPauseRequest(object requester)
    {
        return _pauseRequesters.Contains(requester);
    }

    public bool HasDisableControlsRequest(object requester)
    {
        return _disableControlsRequesters.Contains(requester);
    }

    public bool HasCursorControlRequest(object requester)
    {
        return _cursorControlRequesters.Contains(requester);
    }

    private void TogglePlayerControls(bool shouldDisable)
    {
        disablePlayerInputs = shouldDisable;

        bool enableComponents = !shouldDisable;

        if (Core.Player != null)
        {
            Core.Player.Controller.enabled = enableComponents;
            Core.Player.Bobbing.enabled = enableComponents;
            Core.Player.Interact.enabled = enableComponents;
            Core.Player.Footsteps.enabled = enableComponents;
        }
    }

    public void UpdateCursorVisiblity(bool? forceDisable = null)
    {
        // If any script has manual cursor control, don't interfere
        if (_cursorControlRequesters.Count > 0)
            return;

        bool showCursor = forceDisable.HasValue
            ? !forceDisable.Value   // forceDisable = true -> hide cursor
            : disablePlayerInputs;  // disable inputs = true -> show cursor

        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = showCursor;
    }
}