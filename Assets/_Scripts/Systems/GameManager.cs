using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Values")]
    public bool gamePaused = false; // Now reflects the current paused state, derived from pauseRequestCount
    public bool disablePlayerInputs = false;
    public bool hidePlayerHUD = false;

    [Header("Player References")]
    public Player player;

    [Header("Inherited QOL")]
    public bool inventoryPausesGame;
    public bool skipIntroSequence;
    public bool cameraShaking = true;

    public int pauseRequestCount = 0; // Public just in case another script needs to read this amount
    public int disableControlsRequestCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Check for player and if there's no player, try to find the singleton/instance
        player = player != null ? player : Player.Instance;

        Time.timeScale = 1.0f;
        gamePaused = false;
        pauseRequestCount = 0;
    }

    private void Start()
    {
        // Disable annoying ass PrimeTween warns
        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnZeroDuration = false;
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;

        Screen.SetResolution(1920, 1080, SettingsManager.Instance.fullScreenMode);

        RequestDisableControls(shouldDisable: disablePlayerInputs);
    }

    // Call this when needing to pause the game
    public void RequestPause()
    {
        pauseRequestCount++;

        // If the game wasn't already paused, apply the pause effects
        if (!gamePaused)
        {
            ApplyPauseState(true);
        }
    }

    // Call this when you no longer are needing to pause the game
    public void ReleasePause()
    {
        if (pauseRequestCount > 0) // Prevent going below zero
        {
            pauseRequestCount--;
        }

        // Only unpause if no more active pause requests AND the game is currently paused
        if (pauseRequestCount == 0 && gamePaused)
        {
            ApplyPauseState(false);
        }
    }

    // Internal method to apply the actual pause/unpause
    private void ApplyPauseState(bool shouldPause)
    {
        gamePaused = shouldPause; // Update the public flag/source of truth for the scene's run state

        AudioManager.Instance.ToggleSounds(gamePaused); // Call AudioManager with explicit bool :D
        Time.timeScale = gamePaused ? 0f : 1.0f;

        GameManager.Instance.RequestDisableControls(shouldDisable: gamePaused);
        UpdateCursorVisiblity();
    }

    public void PauseGame()
    {
        gamePaused = !gamePaused;
        AudioManager.Instance.ToggleSounds(gamePaused);
        Time.timeScale = gamePaused ? 0f : 1.0f;

        GameManager.Instance.RequestDisableControls(shouldDisable: gamePaused);
        UpdateCursorVisiblity();
    }

    public void RequestDisableControls(bool shouldDisable)
    {
        if (shouldDisable)
        {
            disableControlsRequestCount++;
        }
        else
        {
            disableControlsRequestCount--;
            if (disableControlsRequestCount < 0)
            {
                disableControlsRequestCount = 0;
            }
        }

        bool newState = disableControlsRequestCount > 0;
        if (newState != disablePlayerInputs)
        {
            // Only update the state if it has actually changed
            TogglePlayerControls(newState);
            UpdateCursorVisiblity();
        }
    }

    private void TogglePlayerControls(bool shouldDisable)
    {
        disablePlayerInputs = shouldDisable;

        bool enableComponents = !shouldDisable;

        if (player != null)
        {
            // player.playerInputs.enabled = enableComponents;
            player.playerController.enabled = enableComponents;
            player.playerBobbing.enabled = enableComponents;

            if (player.playerInteract != null)
            {
                player.playerInteract.enabled = enableComponents;
            }
            if (player.playerFootsteps != null)
            {
                player.playerFootsteps.enabled = enableComponents;
            }
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