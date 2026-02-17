using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class UILoading : MonoBehaviour
{
    [Space]
    public Image background;
    public Image metalBackground;
    public TMP_Text pressAnyKeyText;
    public CanvasGroup pressAnyKeyGroup;

    [Header("Loading")]
    public TMP_Text loadingText;
    public BarProgress loadingMeter;
    public Image loadingOutlineMeter;
    public LocalizedString loadingTextTemplate;

    [Header("Facts")]
    public TMP_Text headerText;
    public TMP_Text descriptionText;

    private CanvasGroup _mainCanvasGroup;
    private CancellationTokenSource _flashCTS;

    private void Awake()
    {
        _mainCanvasGroup = GetComponent<CanvasGroup>();
        if (_mainCanvasGroup == null)
        {
            _mainCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            Log.Warning("UILoading: CanvasGroup was missing and has been added automatically.");
        }

        // Setup Localization
        LocalizationHelper.LocaleChanged += OnLocaleChanged;
        if (loadingTextTemplate != null)
        {
            loadingTextTemplate.StringChanged += OnLoadingStringChanged;
        }

        HideImmediate();
    }

    private void OnDestroy()
    {
        StopFlashing();

        LocalizationHelper.LocaleChanged -= OnLocaleChanged;
        if (loadingTextTemplate != null)
        {
            loadingTextTemplate.StringChanged -= OnLoadingStringChanged;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _mainCanvasGroup.alpha = 1f;
        _mainCanvasGroup.interactable = true;
        _mainCanvasGroup.blocksRaycasts = true;
        pressAnyKeyGroup.alpha = 0f;
        loadingMeter.SetProgress(0f);
        UpdateLoadingText(0f);
    }

    public void Hide()
    {
        StopFlashing();
        _mainCanvasGroup.alpha = 0f;
        _mainCanvasGroup.interactable = false;
        _mainCanvasGroup.blocksRaycasts = false;
    }

    public void HideImmediate()
    {
        StopFlashing();
        gameObject.SetActive(false);
        _mainCanvasGroup.alpha = 0f;
        _mainCanvasGroup.interactable = false;
        _mainCanvasGroup.blocksRaycasts = false;
    }

    public async UniTask FadeIn(float duration = 0.3f)
    {
        gameObject.SetActive(true);
        _mainCanvasGroup.interactable = true;
        _mainCanvasGroup.blocksRaycasts = true;
        await Tween.Alpha(_mainCanvasGroup, 1f, duration).ToYieldInstruction().ToUniTask();
    }

    public async UniTask FadeOut(float duration = 0.3f)
    {
        StopFlashing();
        await Tween.Alpha(_mainCanvasGroup, 0f, duration).ToYieldInstruction().ToUniTask();
        _mainCanvasGroup.interactable = false;
        _mainCanvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void SetProgress(float progress)
    {
        loadingMeter.SetProgress(progress);
    }

    public void UpdateLoadingText(float progress)
    {
        if (loadingText == null) return;

        int percentage = Mathf.RoundToInt(progress * 100f);
        string template = loadingTextTemplate?.GetLocalizedString();

        try
        {
            loadingText.text = string.Format(template, percentage);
        }
        catch (System.FormatException)
        {
        }
    }

    public void SetLoadingText(string text)
    {
        if (loadingText != null)
            loadingText.text = text;
    }

    public void SetFactText(string header, string description)
    {
        if (headerText != null)
            headerText.text = header;
        if (descriptionText != null)
            descriptionText.text = description;
    }

    public void ShowPressAnyKey()
    {
        if (pressAnyKeyGroup != null)
        {
            pressAnyKeyGroup.alpha = 1f;
            StartFlashing();
        }
    }

    public void HidePressAnyKey()
    {
        StopFlashing();
        if (pressAnyKeyGroup != null)
        {
            pressAnyKeyGroup.alpha = 0f;
        }
    }

    public void StartFlashing(float flashSpeed = 1f)
    {
        StopFlashing();
        if (pressAnyKeyGroup != null)
        {
            pressAnyKeyGroup.alpha = 1f;
            _flashCTS = new CancellationTokenSource();
            FlashAsync(flashSpeed, _flashCTS.Token).Forget();
        }
    }

    public void StopFlashing()
    {
        if (_flashCTS != null)
        {
            _flashCTS.Cancel();
            _flashCTS.Dispose();
            _flashCTS = null;
        }

        if (pressAnyKeyGroup != null)
        {
            Tween.StopAll(pressAnyKeyGroup);
        }
    }

    private async UniTaskVoid FlashAsync(float speed, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Tween.Alpha(pressAnyKeyGroup, 0.3f, 0.5f / speed).ToYieldInstruction().ToUniTask(cancellationToken: ct);
                await Tween.Alpha(pressAnyKeyGroup, 1f, 0.5f / speed).ToYieldInstruction().ToUniTask(cancellationToken: ct);
            }
        }
        catch (System.OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
    }

    private void OnLocaleChanged()
    {
        // Update loading text with current progress
        if (loadingMeter != null)
        {
            UpdateLoadingText(loadingMeter.GetProgress());
        }
    }

    private void OnLoadingStringChanged(string value)
    {
        // Update loading text with current progress
        if (loadingMeter != null)
        {
            UpdateLoadingText(loadingMeter.GetProgress());
        }
    }
}