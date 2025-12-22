using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("References")]
    public PlayableDirector playableDirector;
    public Player player;

    [Header("Settings")]
    [Tooltip("Should the cutscene automatically disable player controls?")]
    public bool autoDisableControls = true;

    [Tooltip("Should the cutscene hide the player HUD?")]
    public bool autoHideHUD = true;

    [Tooltip("Restore the main camera priority after cutscene ends")]
    public bool restoreMainCamera = true;

    [Header("Camera Transition")]
    [Tooltip("Blend time when transitioning back to gameplay camera")]
    public float returnBlendDuration = 1f;

    private float _previousBlendTime;
    private bool _isPlayingCutscene = false;

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

        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }
    }

    private void OnEnable()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped += OnCutsceneFinished;
        }
    }

    private void OnDisable()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnCutsceneFinished;
        }

        // Clean up if disabled during cutscene
        if (_isPlayingCutscene && GameManager.Instance != null)
        {
            RestorePlayerControl();
        }
    }

    /// <summary>
    /// Plays a cutscene using the assigned Timeline asset
    /// </summary>
    public void PlayCutscene(TimelineAsset timeline = null)
    {
        if (_isPlayingCutscene)
        {
            Log.Warning("Cannot start cutscene - another cutscene is already playing!");
            return;
        }

        if (playableDirector == null)
        {
            Log.Error("PlayableDirector is not assigned!");
            return;
        }

        if (timeline != null)
        {
            playableDirector.playableAsset = timeline;
        }

        StartCutscene();
    }

    /// <summary>
    /// Plays a cutscene and waits for it to complete
    /// </summary>
    public async UniTask PlayCutsceneAsync(TimelineAsset timeline = null)
    {
        PlayCutscene(timeline);

        // Wait until cutscene is finished
        while (_isPlayingCutscene)
        {
            await UniTask.Yield();
        }
    }

    private void StartCutscene()
    {
        _isPlayingCutscene = true;

        // Store previous camera blend time
        if (CameraManager.Instance != null)
        {
            _previousBlendTime = CameraManager.Instance.cameraBrain.DefaultBlend.Time;
        }

        // Disable player controls
        if (autoDisableControls && GameManager.Instance != null)
        {
            GameManager.Instance.RequestDisableControls(this, shouldDisable: true);
        }

        // Hide HUD
        if (autoHideHUD && GameManager.Instance != null)
        {
            GameManager.Instance.hidePlayerHUD = true;
        }

        // Disable player input actions
        if (player != null && player.playerInputs != null)
        {
            player.playerInputs.DisableGameplayInputs();
        }

        // Start playing the timeline
        playableDirector.Play();

        Log.VerboseInfo($"Started cutscene: {playableDirector.playableAsset.name}");
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        if (!_isPlayingCutscene) return;

        Log.VerboseInfo($"Cutscene finished: {director.playableAsset.name}");

        RestorePlayerControl();
    }

    private void RestorePlayerControl()
    {
        _isPlayingCutscene = false;

        // Restore main camera priority if needed
        if (restoreMainCamera && CameraManager.Instance != null)
        {
            CameraManager.Instance.cameraMain.Priority = 10;
            CameraManager.Instance.cameraBrain.DefaultBlend.Time = returnBlendDuration;
        }

        // Re-enable player controls
        if (autoDisableControls && GameManager.Instance != null)
        {
            GameManager.Instance.RequestDisableControls(this, shouldDisable: false);
        }

        // Show HUD
        if (autoHideHUD && GameManager.Instance != null)
        {
            GameManager.Instance.hidePlayerHUD = false;
        }

        // Re-enable player input actions
        if (player != null && player.playerInputs != null)
        {
            player.playerInputs.EnableGameplayInputs();
        }

        // Restore original blend time
        RestoreCameraBlendTime().Forget();
    }

    private async UniTask RestoreCameraBlendTime()
    {
        if (CameraManager.Instance == null) return;

        // Wait for the blend to complete
        await UniTask.WaitForSeconds(returnBlendDuration, ignoreTimeScale: false);

        // Restore original blend time
        CameraManager.Instance.cameraBrain.DefaultBlend.Time = _previousBlendTime;
    }

    /// <summary>
    /// Stops the current cutscene and restores player control
    /// </summary>
    public void StopCutscene()
    {
        if (!_isPlayingCutscene) return;

        if (playableDirector != null && playableDirector.state == PlayState.Playing)
        {
            playableDirector.Stop();
        }

        RestorePlayerControl();
    }

    /// <summary>
    /// Pauses the current cutscene
    /// </summary>
    public void PauseCutscene()
    {
        if (_isPlayingCutscene && playableDirector != null)
        {
            playableDirector.Pause();
        }
    }

    /// <summary>
    /// Resumes a paused cutscene
    /// </summary>
    public void ResumeCutscene()
    {
        if (_isPlayingCutscene && playableDirector != null)
        {
            playableDirector.Resume();
        }
    }

    /// <summary>
    /// Check if a cutscene is currently playing
    /// </summary>
    public bool IsPlayingCutscene()
    {
        return _isPlayingCutscene;
    }

    // Signal receiver methods (can be called from Timeline Signals)

    public void OnCutsceneEvent(string eventName)
    {
        Log.VerboseInfo($"Cutscene event triggered: {eventName}");
        // Handle custom events here
    }

    public void ShakeCamera(float intensity)
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.GenerateShake(intensity);
        }
    }

    public void PlaySound(string soundEventPath)
    {
        // Integrate with your FMOD system
        Log.VerboseInfo($"Playing cutscene sound: {soundEventPath}");
    }
}