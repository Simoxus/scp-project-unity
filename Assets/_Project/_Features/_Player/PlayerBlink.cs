using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Threading;
using TriInspector;
using UnityEngine;

public class PlayerBlink : MonoBehaviour
{
    [Space]
    [SerializeField] private bool allowBlinkHold = true;
    [SerializeField] private float blinkFrequency = 15.43f;
    [SerializeField] private float blinkingTime = 0.18f;

    [Header("Runtime")]
    [ReadOnly] public float currentBlink = 1f;

    private float _blinkTimer;
    private bool _isHoldingBlink = false;

    public event Action OnBlinkStarted;
    public event Action OnBlinkEnded;

    public bool IsBlinking { get; private set; }

    private CancellationTokenSource _blinkCts;

    private void Start()
    {
        currentBlink = 1f;
        _blinkTimer = blinkFrequency;

        if (Core.Player.Inputs != null)
        {
            Core.Player.Inputs.OnBlink += () => DoBlink().Forget();
        }

        // Start blink timer loop
        _blinkCts = new CancellationTokenSource();
        BlinkTimerLoop(_blinkCts.Token).Forget();
    }

    private void OnDestroy()
    {
        if (Core.Player.Inputs != null)
        {
            Core.Player.Inputs.OnBlink -= () => DoBlink().Forget();
        }

        _blinkCts?.Cancel();
        _blinkCts?.Dispose();
    }

    private void Update()
    {
        if (Core.GameManager == null || Core.GameManager.gamePaused) return;

        HandleBlinkHold();
        UpdateUI();
    }

    private void HandleBlinkHold()
    {
        if (!allowBlinkHold) return;

        bool isHoldingBlink = Core.Player.Inputs != null && Core.Player.Inputs.BlinkHeld;

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

            if (Core.GameManager && Core.GameManager.gamePaused) continue;

            _blinkTimer -= Time.deltaTime;
            currentBlink = Mathf.Clamp01(_blinkTimer / blinkFrequency);

            if (_blinkTimer <= 0f)
            {
                await DoBlink();
            }
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

        if (allowBlinkHold && Core.Player?.Inputs != null && Core.Player.Inputs.BlinkHeld)
        {
            while (Core.Player.Inputs.BlinkHeld)
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

    private void UpdateUI()
    {
        if (Core.UI.Indicators != null)
        {
            Core.UI.Indicators.SetBlinkProgress(currentBlink);
        }
    }

    public void StartBlink()
    {
        if (!IsBlinking)
        {
            DoBlink().Forget();
        }
    }

    public void StopBlink()
    {
        _isHoldingBlink = false;
    }

    public void AccelerateBlinkDepletion(float multiplier)
    {
        blinkFrequency /= multiplier;
    }

    public void ResetBlinkFrequency()
    {
        blinkFrequency = 15.43f;
    }
}