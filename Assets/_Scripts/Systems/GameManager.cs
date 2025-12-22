using EditorAttributes;
using PrimeTween;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector] public bool IsInMainMenu => SceneManager.GetActiveScene().name == "MainMenu";

    [Header("Global Values")]
    public bool gamePaused = false;
    public bool disablePlayerInputs = false;
    public bool hidePlayerHUD = false;

    [Header("Player References")]
    public Player player;

    [Header("Inherited QOL")]
    public bool inventoryPausesGame;
    public bool skipIntroSequence;
    public bool cameraShaking = true;

    [ReadOnly] public int pauseRequestCount = 0;
    [ReadOnly] public int disableControlsRequestCount = 0;

    // Variables that track which scripts have requested pause/disable
    private readonly HashSet<object> _pauseRequesters = new HashSet<object>();
    private readonly HashSet<object> _disableControlsRequesters = new HashSet<object>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Log.VerboseWarning($"Duplicate instance of {GetType().Name} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        player = player != null ? player : Player.Instance;

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
                ApplyPauseState(true);
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
                ApplyPauseState(false);
            }
        }
    }

    private void ApplyPauseState(bool shouldPause)
    {
        gamePaused = shouldPause;

        AudioManager.Instance.ToggleGameSounds(gamePaused);
        Time.timeScale = gamePaused ? 0f : 1.0f;

        GameManager.Instance.RequestDisableControls(this, shouldDisable: gamePaused);
        UpdateCursorVisiblity();
    }

    public void RequestDisableControls(object requester, bool shouldDisable)
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
            UpdateCursorVisiblity();
        }
    }

    public void ForceResetPauseState()
    {
        _pauseRequesters.Clear();
        _disableControlsRequesters.Clear();
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

    private void TogglePlayerControls(bool shouldDisable)
    {
        disablePlayerInputs = shouldDisable;

        bool enableComponents = !shouldDisable;

        if (player != null)
        {
            player.playerController.enabled = enableComponents;
            player.playerBobbing.enabled = enableComponents;
            player.playerInteract.enabled = enableComponents;
            player.playerFootsteps.enabled = enableComponents;
        }
    }

    public void UpdateCursorVisiblity(bool? forceDisable = null)
    {
        bool showCursor = forceDisable.HasValue
            ? !forceDisable.Value   // forceDisable = true -> hide
            : disablePlayerInputs;  // disable inputs = true -> show

        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = showCursor;
    }
}