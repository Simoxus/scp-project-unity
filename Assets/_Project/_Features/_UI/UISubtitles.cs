using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UISubtitles : MonoBehaviour
{
    public static bool SubtitlesEnabled { get; set; } = true;

    [Space]
    public CanvasGroup canvasGroup;
    public RectTransform linesContainer;
    public GameObject subtitleLinePrefab;

    [Header("Settings")]
    public float fadeDuration = 0.15f;
    public float smoothDuration = 0.15f;
    public Ease smoothEase = Ease.OutCubic;
    public float lineHeight = 50f;
    public float lineSpacing = 5f;
    public int maxLines = 5;
    public Color backgroundColor = new Color(0.078f, 0.078f, 0.078f, 0.9f);
    public Color textColor = Color.white;

    private List<SubtitleLine> _activeLines = new List<SubtitleLine>();
    private List<SubtitleLine> _linePool = new List<SubtitleLine>();
    private Dictionary<int, SubtitleLine> _handleToLine = new Dictionary<int, SubtitleLine>();
    private int _nextHandle = 1;

    public bool HasActiveSubtitles => _activeLines.Count > 0;

    private void Awake()
    {
        LocalizationHelper.LocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        LocalizationHelper.LocaleChanged -= OnLocaleChanged;

        foreach (var line in _activeLines)
        {
            line.cts?.Cancel();
            line.cts?.Dispose();
            line.PositionTween.Stop();
            line.AlphaTween.Stop();
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Update line timers and handle removal
        for (int i = _activeLines.Count - 1; i >= 0; i--)
        {
            var line = _activeLines[i];
            line.timeLeft -= deltaTime;

            if (line.timeLeft < 0f && !line.isFadingOut)
            {
                line.isFadingOut = true;
                FadeOutLine(line, i);
            }
        }

        UpdateLinePositions();
    }

    private void UpdateLinePositions()
    {
        float currentY = lineSpacing;

        for (int i = _activeLines.Count - 1; i >= 0; i--)
        {
            var line = _activeLines[i];

            if (line.Text != null && Mathf.Abs(line.targetY - currentY) > 0.01f)
            {
                line.targetY = currentY;
                line.PositionTween.Stop();

                line.PositionTween = Tween.Custom(
                    line.RectTransform.anchoredPosition.y,
                    currentY,
                    smoothDuration,
                    value =>
                    {
                        if (line.RectTransform != null)
                        {
                            line.RectTransform.anchoredPosition = new Vector2(0f, value);
                        }
                    },
                    ease: smoothEase
                );
            }

            // Consistent lineHeight
            currentY += lineHeight + lineSpacing;
        }
    }

    private void FadeOutLine(SubtitleLine line, int index)
    {
        if (line == null) return;

        line.AlphaTween.Stop();
        line.AlphaTween = Tween.Custom(1f, 0f, fadeDuration,
            value => line.SetAlpha(value),
            ease: Ease.Linear)
            .OnComplete(() =>
            {
                ReturnLineToPool(line);
                _activeLines.Remove(line);
                UpdateLinePositions();
            });
    }

    public void SetSubtitlesEnabled(bool enabled, bool clearExisting = true)
    {
        SubtitlesEnabled = enabled;

        if (!enabled && clearExisting)
        {
            Clear();
        }
    }

    public void ShowSubtitle(string message, float duration = 3f, string speaker = null)
    {
        if (!SubtitlesEnabled || string.IsNullOrEmpty(message)) return;
        CreateSubtitleLine(message, duration, false, null, null, speaker);
    }

    public int ShowSubtitleWithHandle(string message, float duration = 3f, string speaker = null)
    {
        if (!SubtitlesEnabled || string.IsNullOrEmpty(message)) return -1;
        return CreateSubtitleLineWithHandle(message, duration, false, null, null, speaker);
    }

    public void ShowLocalizedSubtitle(string tableName, string key, float duration = 3f, string speaker = null)
    {
        if (!SubtitlesEnabled || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(key)) return;
        CreateSubtitleLine(null, duration, true, tableName, key, speaker);
    }

    public int ShowLocalizedSubtitleWithHandle(string tableName, string key, float duration = 3f, string speaker = null)
    {
        if (!SubtitlesEnabled || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(key)) return -1;
        return CreateSubtitleLineWithHandle(null, duration, true, tableName, key, speaker);
    }

    public void ShowSubtitleForSound(string message, FMOD.Studio.EventInstance soundEvent, string speaker = null)
    {
        if (!SubtitlesEnabled || string.IsNullOrEmpty(message)) return;
        float duration = GetSoundDuration(soundEvent);
        if (duration <= 0f) duration = 3f;
        ShowSubtitle(message, duration, speaker);
    }

    public void ShowLocalizedSubtitleForSound(string tableName, string key, FMOD.Studio.EventInstance soundEvent, string speaker = null)
    {
        if (!SubtitlesEnabled || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(key)) return;
        float duration = GetSoundDuration(soundEvent);
        if (duration <= 0f) duration = 3f;
        ShowLocalizedSubtitle(tableName, key, duration, speaker);
    }

    public void RemoveSubtitle(int handle)
    {
        if (_handleToLine.TryGetValue(handle, out SubtitleLine line))
        {
            _handleToLine.Remove(handle);

            if (line != null && _activeLines.Contains(line))
            {
                int index = _activeLines.IndexOf(line);
                if (!line.isFadingOut)
                {
                    line.isFadingOut = true;
                    FadeOutLine(line, index);
                }
            }
        }
    }

    public void Clear()
    {
        foreach (var line in _activeLines)
        {
            line.cts?.Cancel();
            line.cts?.Dispose();
            line.PositionTween.Stop();
            line.AlphaTween.Stop();
            ReturnLineToPool(line);
        }
        _activeLines.Clear();
        _handleToLine.Clear();
    }

    private void CreateSubtitleLine(string message, float duration, bool isLocalized, string tableName, string key, string speaker)
    {
        // Enforce max lines
        while (_activeLines.Count >= maxLines)
        {
            var oldestLine = _activeLines[0];
            oldestLine.PositionTween.Stop();
            oldestLine.AlphaTween.Stop();
            ReturnLineToPool(oldestLine);
            _activeLines.RemoveAt(0);
        }

        var line = GetLineFromPool();
        if (line == null) return;

        line.message = message;
        line.isLocalized = isLocalized;
        line.tableName = tableName;
        line.localizationKey = key;
        line.speaker = speaker;
        line.timeLeft = duration;
        line.targetY = 0f;
        line.isFadingOut = false;
        line.cts = new CancellationTokenSource();

        _activeLines.Add(line);

        // Set colors
        line.SetBackgroundColor(backgroundColor);
        line.SetTextColor(textColor);
        line.SetAlpha(0f);

        // Fade in
        line.AlphaTween = Tween.Custom(0f, 1f, fadeDuration,
            value => line.SetAlpha(value),
            ease: Ease.Linear);

        if (isLocalized)
        {
            LoadLocalizedText(line).Forget();
        }
        else
        {
            line.SetText(message);
        }

        UpdateLinePositions();
    }

    private int CreateSubtitleLineWithHandle(string message, float duration, bool isLocalized, string tableName, string key, string speaker)
    {
        // Enforce max lines
        while (_activeLines.Count >= maxLines)
        {
            var oldestLine = _activeLines[0];
            oldestLine.PositionTween.Stop();
            oldestLine.AlphaTween.Stop();
            ReturnLineToPool(oldestLine);
            _activeLines.RemoveAt(0);
        }

        var line = GetLineFromPool();
        if (line == null) return -1;

        line.message = message;
        line.isLocalized = isLocalized;
        line.tableName = tableName;
        line.localizationKey = key;
        line.speaker = speaker;
        line.timeLeft = duration;
        line.targetY = 0f;
        line.isFadingOut = false;
        line.cts = new CancellationTokenSource();

        _activeLines.Add(line);

        int handle = _nextHandle++;
        _handleToLine[handle] = line;

        // Set colors
        line.SetBackgroundColor(backgroundColor);
        line.SetTextColor(textColor);
        line.SetAlpha(0f);

        // Fade in
        line.AlphaTween = Tween.Custom(0f, 1f, fadeDuration,
            value => line.SetAlpha(value),
            ease: Ease.Linear);

        if (isLocalized)
        {
            LoadLocalizedText(line).Forget();
        }
        else
        {
            line.SetText(message);
        }

        UpdateLinePositions();

        return handle;
    }

    private async UniTaskVoid LoadLocalizedText(SubtitleLine line)
    {
        try
        {
            string text = await LocalizationHelper.GetStringAsync(line.tableName, line.localizationKey);
            if (!string.IsNullOrEmpty(text) && line.cts != null && !line.cts.IsCancellationRequested)
            {
                line.SetText(text);
            }
        }
        catch (System.Exception e)
        {
            Log.Error($"Failed to load localized subtitle: {e.Message}");
        }
    }

    private SubtitleLine GetLineFromPool()
    {
        SubtitleLine line = null;

        if (_linePool.Count > 0)
        {
            line = _linePool[_linePool.Count - 1];
            _linePool.RemoveAt(_linePool.Count - 1);
            line.gameObject.SetActive(true);
        }
        else if (subtitleLinePrefab != null)
        {
            GameObject instantiated = Instantiate(subtitleLinePrefab, linesContainer);
            line = instantiated.GetComponent<SubtitleLine>();

            if (line == null)
            {
                Destroy(instantiated);
                return null;
            }

            line.SetAlpha(0f);

            RectTransform rectTransform = line.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, lineHeight);
        }

        if (line != null)
        {
            line.RectTransform.anchoredPosition = new Vector2(0f, -lineHeight);
        }

        return line;
    }

    private void ReturnLineToPool(SubtitleLine line)
    {
        if (line != null)
        {
            line.gameObject.SetActive(false);
            _linePool.Add(line);
        }

        line.PositionTween.Stop();
        line.AlphaTween.Stop();
        line.cts?.Cancel();
        line.cts?.Dispose();
        line.cts = null;
    }

    private float GetSoundDuration(FMOD.Studio.EventInstance soundEvent)
    {
        if (!soundEvent.isValid()) return 0f;

        soundEvent.getDescription(out FMOD.Studio.EventDescription description);
        if (description.isValid())
        {
            description.getLength(out int length);
            return length / 1000f;
        }

        return 0f;
    }

    private void OnLocaleChanged()
    {
        foreach (var line in _activeLines)
        {
            if (line.isLocalized && !string.IsNullOrEmpty(line.tableName))
            {
                LoadLocalizedText(line).Forget();
            }
        }
    }
}