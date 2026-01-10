using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ValueTooltipSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private RectTransform backgroundPanel;

    [Header("Format Settings")]
    [SerializeField] private DisplayMode displayMode = DisplayMode.Value;
    [SerializeField] private string valueFormat = "{0:F1}";
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";

    [Header("Resize Settings")]
    [SerializeField] private Vector2 padding = new Vector2(20f, 16f);
    [SerializeField] private float maxWidth = 400f;

    private bool isDragging = false;

    public enum DisplayMode
    {
        Value,
        Percentage,
        WholeNumber
    }

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }

        SetupEventTrigger();
    }

    private void SetupEventTrigger()
    {
        EventTrigger trigger = slider.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.gameObject.AddComponent<EventTrigger>();
        }

        // Add PointerUp
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnDragEnd(); });
        trigger.triggers.Add(pointerUp);

        // Add PointerDown
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnDragStart(); });
        trigger.triggers.Add(pointerDown);
    }

    private void OnEnable()
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnDisable()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    private void OnDragStart()
    {
        isDragging = true;

        if (tooltipObject != null)
        {
            tooltipObject.SetActive(true);
            UpdateTooltipText(slider.value);
        }
    }

    private void OnDragEnd()
    {
        isDragging = false;

        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (isDragging && tooltipObject != null && tooltipObject.activeSelf)
        {
            UpdateTooltipText(value);
        }
    }

    private void UpdateTooltipText(float value)
    {
        if (tooltipText != null)
        {
            string displayValue = FormatValue(value);
            tooltipText.text = prefix + displayValue + suffix;

            ResizeTooltip();
        }
    }

    private void ResizeTooltip()
    {
        if (tooltipText == null || backgroundPanel == null) return;

        Canvas.ForceUpdateCanvases();

        tooltipText.rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            maxWidth - padding.x * 2
        );
        tooltipText.ForceMeshUpdate();

        Vector2 textSize = tooltipText.GetRenderedValues(false);
        textSize.x = Mathf.Min(textSize.x, maxWidth - padding.x * 2);
        Vector2 panelSize = textSize + padding * 2;

        backgroundPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelSize.x);
        backgroundPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelSize.y);
    }

    private string FormatValue(float value)
    {
        switch (displayMode)
        {
            case DisplayMode.Value:
                return string.Format(valueFormat, value);

            case DisplayMode.Percentage:
                float normalizedValue = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
                float percentage = normalizedValue * 100f;
                return string.Format(valueFormat, percentage);

            case DisplayMode.WholeNumber:
                return Mathf.RoundToInt(value).ToString();

            default:
                return value.ToString();
        }
    }
}