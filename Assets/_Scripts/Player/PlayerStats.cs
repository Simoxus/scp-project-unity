using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Sprint Settings")]
    [SerializeField] private float sprintDrainRate = 0.26f;
    [SerializeField] private float sprintRegenRateMoving = 0.194f;
    [SerializeField] private float sprintRegenRateIdle = 0.2f;
    [SerializeField] private float minSprintThreshold = 0.0f;

    [Header("Blink Settings")]
    [SerializeField] private bool allowBlinkHold = true;
    [SerializeField] private float blinkFrequency = 15.43f;
    [SerializeField] private float blinkingTime = 0.18f;

    [Header("Tired Sound Settings")]
    [SerializeField] private float tiredSoundThreshold = 0.07f;

    [Header("Current Values")]
    [ReadOnly] public float currentSprint = 1f;
    [ReadOnly] public float currentBlink = 1f;

    private float _blinkTimer;
    private bool _isSprinting;
    private bool _isMoving;

    private bool _isHoldingBlink = false;

    public event Action OnBlinkStarted;
    public event Action OnBlinkEnded;

    public bool IsBlinking { get; private set; }

    private CancellationTokenSource _blinkCts;

    private void Start()
    {
        currentSprint = 1f;
        currentBlink = 1f;
        _blinkTimer = blinkFrequency;

        if (Core.Player.PlayerInputs != null)
        {
            Core.Player.PlayerInputs.OnBlink += () => DoBlink().Forget();
        }

        // Start blink timer loop
        _blinkCts = new CancellationTokenSource();
        BlinkTimerLoop(_blinkCts.Token).Forget();
    }

    private void OnDestroy()
    {
        if (Core.Player.PlayerInputs != null)
        {
            Core.Player.PlayerInputs.OnBlink -= () => DoBlink().Forget();
        }

        _blinkCts?.Cancel();
        _blinkCts?.Dispose();
    }

    private void Update()
    {
        if (Core.GameManager == null || Core.GameManager.gamePaused) return;

        HandleSprint();
        HandleBlinkHold();
        UpdateUI();
    }

    private void HandleBlinkHold()
    {
        if (!allowBlinkHold) return;

        bool isHoldingBlink = Core.Player.PlayerInputs != null && Core.Player.PlayerInputs.BlinkHeld;

        if (isHoldingBlink && !IsBlinking)
        {
            if (!_isHoldingBlink)
            {
                _isHoldingBlink = true;
            }
        }
        else if (_isHoldingBlink && !isHoldingBlink)
        {
            _isHoldingBlink = false;
        }
    }

    private async UniTaskVoid BlinkTimerLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

            if (GameManager.Instance && GameManager.Instance.gamePaused) continue;

            _blinkTimer -= Time.deltaTime;
            currentBlink = Mathf.Clamp01(_blinkTimer / blinkFrequency);

            if (_blinkTimer <= 0f)
            {
                await DoBlink();
            }
        }
    }

    private void HandleSprint()
    {
        if (_isSprinting && _isMoving && currentSprint > minSprintThreshold)
        {
            currentSprint -= (sprintDrainRate / 100f) * Time.deltaTime * 60f;

            if (currentSprint <= 0f)
            {
                currentSprint = -0.2f;
            }
        }
        else
        {
            float regenRate = _isMoving ? sprintRegenRateMoving : sprintRegenRateIdle;
            currentSprint += (regenRate / 100f) * Time.deltaTime * 60f;
            currentSprint = Mathf.Min(currentSprint, 1f);
        }

        HandleTiredSounds();
    }

    private void HandleTiredSounds()
    {
        bool shouldPlayTired = currentSprint < tiredSoundThreshold;

        if (shouldPlayTired)
        {
            FMODHelper.PlayOneShot(AudioDataAccess.Instance.Player.TiredSound);
        }
    }

    private async UniTask DoBlink()
    {
        IsBlinking = true;
        OnBlinkStarted?.Invoke();

        _blinkTimer = 0f;
        currentBlink = 0f;

        await Tween.Alpha(Core.UI.BlinkOverlay, 1f, 0.11f, ease: Ease.InOutCirc);
        await UniTask.WaitForSeconds(blinkingTime, false);

        if (allowBlinkHold && Core.Player?.PlayerInputs != null && Core.Player.PlayerInputs.BlinkHeld)
        {
            while (Core.Player.PlayerInputs.BlinkHeld)
            {
                _blinkTimer = 0f;
                currentBlink = 0f;

                await UniTask.Yield(PlayerLoopTiming.Update);

                if (Core.GameManager && Core.GameManager.gamePaused)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
        }

        await Tween.Alpha(Core.UI.BlinkOverlay, 0f, 0.12f, ease: Ease.InOutCirc);

        _blinkTimer = blinkFrequency;
        currentBlink = 1f;
        _isHoldingBlink = false;

        IsBlinking = false;
        OnBlinkEnded?.Invoke();
    }

    public bool CanSprint()
    {
        return currentSprint > minSprintThreshold;
    }

    public void SetCurrentState(bool isSprinting, bool isMoving, bool isCrouching)
    {
        _isSprinting = isSprinting;
        _isMoving = isMoving;
    }

    private void UpdateUI()
    {
        if (Core.UI.Indicators != null)
        {
            Core.UI.Indicators.SetProgress(currentBlink, currentSprint);
        }
    }
}