using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Localized Content")]
    [SerializeField] private LocalizedString localizedTooltip;

    private string _cachedTooltipText;
    private bool _isHovered;

    private void Start()
    {
        if (localizedTooltip != null && !localizedTooltip.IsEmpty)
        {
            LoadLocalizedTextAsync().Forget();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        LoadLocalizedTextAsync().Forget();
    }

    private async UniTaskVoid LoadLocalizedTextAsync()
    {
        if (localizedTooltip == null || localizedTooltip.IsEmpty) return;

        try
        {
            var loadOperation = localizedTooltip.GetLocalizedStringAsync();
            _cachedTooltipText = await loadOperation;

            if (_isHovered && Core.UI.Tooltips != null)
            {
                Core.UI.Tooltips.Show(_cachedTooltipText).Forget();
            }
        }
        catch (System.Exception e)
        {
            Log.VerboseWarning($"Failed to load localized tooltip: {e.Message}");
        }
    }

    public void SetLocalizedTooltip(LocalizedString newTooltip)
    {
        localizedTooltip = newTooltip;
        LoadLocalizedTextAsync().Forget();
    }

    public void SetTooltipText(string text)
    {
        _cachedTooltipText = text;
        if (_isHovered && Core.UI.Tooltips != null)
        {
            Core.UI.Tooltips.Show(_cachedTooltipText).Forget();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;

        if (Core.UI.Tooltips != null && !string.IsNullOrEmpty(_cachedTooltipText))
        {
            Core.UI.Tooltips.Show(_cachedTooltipText).Forget();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;

        if (Core.UI.Tooltips != null)
        {
            Core.UI.Tooltips.Hide();
        }
    }

    private void OnDisable()
    {
        _isHovered = false;

        if (Core.UI.Tooltips != null)
        {
            Core.UI.Tooltips.Hide();
        }
    }
}