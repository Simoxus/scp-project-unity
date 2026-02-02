using EditorAttributes;
using PrimeTween;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleLine : MonoBehaviour
{
    [Space]
    public RectTransform RectTransform;
    public CanvasGroup CanvasGroup;
    public Image BackgroundPanel;
    public TextMeshProUGUI Text;

    [Header("Settings")]
    public float maxWidth = 1400f;
    public float paddingHorizontal = 30f;
    public float paddingVertical = 5f;

    [Header("Runtime")]
    [ReadOnly] public string message;
    [ReadOnly] public bool isLocalized;
    [ReadOnly] public string tableName;
    [ReadOnly] public string localizationKey;
    [ReadOnly] public string speaker;
    [ReadOnly] public float timeLeft;
    [ReadOnly] public float targetY;
    [ReadOnly] public bool isFadingOut;

    [HideInInspector] public CancellationTokenSource cts;
    public Tween PositionTween;
    public Tween AlphaTween;

    private void Awake()
    {
        if (Text != null)
        {
            Text.transform.SetAsLastSibling();
            Text.overflowMode = TextOverflowModes.Overflow;
            Text.alignment = TextAlignmentOptions.Center;
        }
    }

    public void SetAlpha(float alpha)
    {
        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = alpha;
        }
    }

    public void SetText(string text)
    {
        if (Text != null)
        {
            // Add speaker prefix if present
            if (!string.IsNullOrEmpty(speaker))
            {
                text = $"<b>{speaker}:</b> {text}";
            }

            // Set text with max width constraint
            Text.rectTransform.sizeDelta = new Vector2(maxWidth - paddingHorizontal * 2, RectTransform.sizeDelta.y - paddingVertical * 2);
            Text.text = text;
            Text.ForceMeshUpdate();

            // Get actual rendered text size
            Vector2 textSize = Text.GetRenderedValues(false);

            // Resize background to fit text width + padding
            if (BackgroundPanel != null)
            {
                float bgWidth = Mathf.Min(textSize.x + paddingHorizontal * 2, maxWidth);
                BackgroundPanel.rectTransform.sizeDelta = new Vector2(bgWidth, RectTransform.sizeDelta.y);
            }
        }
    }

    public void SetBackgroundColor(Color color)
    {
        if (BackgroundPanel != null)
        {
            BackgroundPanel.color = color;
        }
    }

    public void SetTextColor(Color color)
    {
        if (Text != null)
        {
            Text.color = color;
        }
    }

    public void Cleanup()
    {
        PositionTween.Stop();
        AlphaTween.Stop();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}