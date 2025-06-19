using System.Collections;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

public class DisplayManager : MonoBehaviour
{
    public static DisplayManager Instance { get; private set; }

    [Header("UI-related References")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject playerUIOverlays;
    [SerializeField] private GameObject playerUIIndicators;
    [SerializeField] private CanvasGroup blinkOverlayGroup;

    // Tweens
    private Tween _blinkTween;
    private Tween _hudTween;

    // Other
    private GameManager _gameManager => GameManager.Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public async void MakePlayerBlink() 
    {
        _blinkTween = FadeCanvasGroup(blinkOverlayGroup, from: 0, to: 1, duration: 0.15f, ease: Ease.Linear, false);
        await UniTask.WaitForSeconds(0.4f);
        _blinkTween = FadeCanvasGroup(blinkOverlayGroup, from: 1, to: 0, duration: 0.15f, ease: Ease.Linear, false);
    }

    public void TogglePlayerHUD()
    {
        _gameManager.hidePlayerHUD = !_gameManager.hidePlayerHUD;
        CanvasGroup playerUIIndicatorGroup = playerUIIndicators.GetComponent<CanvasGroup>();

        _hudTween.Stop();
        if (_gameManager.hidePlayerHUD == true)
        {
            _hudTween = FadeCanvasGroup(playerUIIndicatorGroup, from: 1, to: 0, duration: 0.8f, ease: Ease.InOutCubic, true);
        }
        else
        {
            _hudTween = FadeCanvasGroup(playerUIIndicatorGroup, from: 0, to: 1, duration: 0.8f, ease: Ease.InOutCubic, true);
        }
    }

    private Tween FadeCanvasGroup(CanvasGroup group, float from, float to, float duration, Ease ease, bool useUnscaledTime)
    {
        return Tween.Custom(
            from,
            to,
            duration: duration,
            ease: ease,
            onValueChange: val => group.alpha = val,
            useUnscaledTime: true
        );
    }
}