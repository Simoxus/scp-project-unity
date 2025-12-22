using Cysharp.Threading.Tasks;
using EditorAttributes;
using FMODUnity;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private IndicatorsUI indicatorsUI;
    [SerializeField] private UIAccess uiAccess;

    [Header("Sprint Settings")]
    [SerializeField] private float sprintDrainRate = 0.4f;
    [SerializeField] private float sprintRegenRateMoving = 0.15f;
    [SerializeField] private float sprintRegenRateIdle = 0.1875f;
    [SerializeField] private float minSprintThreshold = 0.0f;

    [Header("Blink Settings")]
    [SerializeField] private bool allowBlinkHold = true;
    [SerializeField] private float blinkFrequency = 15.43f;
    [SerializeField] private float blinkingTime = 0.21f;

    [Header("Tired Sound Settings")]
    [SerializeField] private EventReference tiredBreathingSound;
    [SerializeField] private float tiredSoundThreshold = 0.2f;

    [Header("Current Values")]
    [ReadOnly] public float currentSprint = 1f;
    [ReadOnly] public float currentBlink = 1f;

    private float _blinkTimer;
    private bool _isSprinting;
    private bool _isMoving;

    private float _currentBlinkHoldTime = 0f;
    private bool _isHoldingBlink = false;

    public event Action OnBlinkStarted;
    public event Action OnBlinkEnded;

    public bool IsBlinking { get; private set; }

    private CancellationTokenSource _blinkCts;

    private void Awake()
    {
        player = player != null ? player : Player.Instance;
    }

    private void Start()
    {
        currentSprint = 1f;
        currentBlink = 1f;
        _blinkTimer = blinkFrequency;

        if (indicatorsUI == null)
        {
            indicatorsUI = FindFirstObjectByType<IndicatorsUI>();
        }

        if (player?.playerInputs != null)
        {
            player.playerInputs.OnBlink += () => DoBlink().Forget();
        }

        // Start blink timer loop
        _blinkCts = new CancellationTokenSource();
        BlinkTimerLoop(_blinkCts.Token).Forget();
    }

    private void OnDestroy()
    {
        if (player?.playerInputs != null)
        {
            player.playerInputs.OnBlink -= () => DoBlink().Forget();
        }

        _blinkCts?.Cancel();
        _blinkCts?.Dispose();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.gamePaused) return;

        HandleSprint();
        HandleBlinkHold();
        UpdateUI();
    }

    private void HandleBlinkHold()
    {
        if (!allowBlinkHold) return;

        bool isHoldingBlink = player?.playerInputs != null && player.playerInputs.BlinkHeld;

        if (isHoldingBlink && !IsBlinking)
        {
            if (!_isHoldingBlink)
            {
                _isHoldingBlink = true;
                _currentBlinkHoldTime = 0f;
            }

            _currentBlinkHoldTime += Time.deltaTime;
        }
        else if (_isHoldingBlink && !isHoldingBlink)
        {
            _isHoldingBlink = false;
            _currentBlinkHoldTime = 0f;
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
            FMODHelper.PlayOneShot(tiredBreathingSound);
        }
    }

    private async UniTask DoBlink()
    {
        IsBlinking = true;
        OnBlinkStarted?.Invoke();

        _blinkTimer = 0f;
        currentBlink = 0f;

        await Tween.Alpha(uiAccess.blinkOverlay, 1f, 0.11f, ease: Ease.InOutCirc);
        await UniTask.WaitForSeconds(blinkingTime, false);

        if (allowBlinkHold && player?.playerInputs != null && player.playerInputs.BlinkHeld)
        {
            while (player.playerInputs.BlinkHeld)
            {
                _blinkTimer = 0f;
                currentBlink = 0f;

                await UniTask.Yield(PlayerLoopTiming.Update);

                if (GameManager.Instance && GameManager.Instance.gamePaused)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
        }

        await Tween.Alpha(uiAccess.blinkOverlay, 0f, 0.12f, ease: Ease.InOutCirc);

        _blinkTimer = blinkFrequency;
        currentBlink = 1f;
        _isHoldingBlink = false;
        _currentBlinkHoldTime = 0f;

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
        if (indicatorsUI != null)
        {
            indicatorsUI.UpdateIndicators(currentSprint, currentBlink, player.currentState);
        }
    }
}