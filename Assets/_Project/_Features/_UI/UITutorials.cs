using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class UITutorials : MonoBehaviour
{
    [Space]
    public CanvasGroup freecamHintPanel;
    public TMP_Text freecamHintText;
    public LocalizedString freecamControlHintTemplate;
    public CanvasGroup canvasGroup;

    private PlayerFreecam _freecam;
    private bool _isVisible;

    public bool IsVisible => _isVisible;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                Log.Warning("UITutorials: CanvasGroup was missing and has been added automatically");
            }
        }

        if (Core.Player != null)
        {
            _freecam = Core.Player.Freecam;
        }

        // Setup Localization
        LocalizationHelper.LocaleChanged += UpdateFreecamHints;
        if (freecamControlHintTemplate != null)
        {
            freecamControlHintTemplate.StringChanged += OnFreecamStringChanged;
        }

        // Setup Input Listeners
        if (Core.Player?.Inputs != null)
        {
            var inputs = Core.Player.Inputs;
            inputs.OnFreecamLock += OnFreecamToggleStateChanged;
            inputs.OnFreecamSmooth += OnFreecamToggleStateChanged;
            inputs.OnFreecamWobble += OnFreecamToggleStateChanged;
            inputs.OnFreecamPause += OnFreecamToggleStateChanged;
        }

        UpdateFreecamHints();
    }

    private void OnDestroy()
    {
        LocalizationHelper.LocaleChanged -= UpdateFreecamHints;

        if (freecamControlHintTemplate != null)
            freecamControlHintTemplate.StringChanged -= OnFreecamStringChanged;

        if (Core.Player?.Inputs != null)
        {
            var inputs = Core.Player.Inputs;
            inputs.OnFreecamLock -= OnFreecamToggleStateChanged;
            inputs.OnFreecamSmooth -= OnFreecamToggleStateChanged;
            inputs.OnFreecamWobble -= OnFreecamToggleStateChanged;
            inputs.OnFreecamPause -= OnFreecamToggleStateChanged;
        }
    }

    public void Show()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        _isVisible = true;
    }

    public void Hide()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        _isVisible = false;
    }

    public void Toggle()
    {
        if (_isVisible)
            Hide();
        else
            Show();
    }

    public void ShowFreecamHints()
    {
        if (freecamHintPanel != null)
            ShowCanvasGroup(freecamHintPanel);
    }

    public void HideFreecamHints()
    {
        if (freecamHintPanel != null)
            HideCanvasGroup(freecamHintPanel);
    }

    public void UpdateFreecamHints()
    {
        if (freecamHintText == null || Core.Player?.Inputs == null) return;

        // Get Actions
        var hide = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Hide"));
        var move = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Move"));
        var look = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Look"));

        // Zoom Logic
        string zKb = InputDisplayHelper.GetDisplay(FindAction("Freecam/Zoom"), "KeyboardMouse");
        string zGp = InputDisplayHelper.GetModifiedDisplay(FindAction("Freecam/Zoom"), FindAction("Freecam/ZoomModifier"), "Gamepad");
        string zoom = $"<b>{zKb}/{zGp}</b>";

        var accel = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Accelerate"));
        var decel = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Decelerate"));
        var up = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Up"));
        var down = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Down"));
        var lck = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Lock"));
        var smth = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Smooth"));
        var wbl = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Wobble"));
        var pse = InputDisplayHelper.GetCombinedDisplayBold(FindAction("Freecam/Pause"));

        string template = freecamControlHintTemplate?.GetLocalizedString() ?? string.Empty;
        string formatted = string.Format(template, hide, move, look, zoom, accel, decel, up, down, lck, smth, wbl, pse);

        // Apply coloring based on toggle states
        freecamHintText.text = ApplyLineColors(formatted, lck, smth, wbl, pse);
    }

    private void OnFreecamStringChanged(string value) => UpdateFreecamHints();

    private async void OnFreecamToggleStateChanged()
    {
        await UniTask.Yield();
        UpdateFreecamHints();
    }

    private string ApplyLineColors(string text, string lckDisp, string smthDisp, string wblDisp, string pseDisp)
    {
        if (_freecam == null) return text;
        string[] lines = text.Split('\n');
        string successHex = ColorUtility.ToHtmlStringRGB(ColorScheme.Success);

        for (int i = 0; i < lines.Length; i++)
        {
            bool isActive = false;
            if (LineContainsDisplay(lines[i], lckDisp)) isActive = _freecam.IsCameraLocked;
            else if (LineContainsDisplay(lines[i], smthDisp)) isActive = _freecam.IsSmoothEnabled;
            else if (LineContainsDisplay(lines[i], wblDisp)) isActive = _freecam.IsWobbleEnabled;
            else if (LineContainsDisplay(lines[i], pseDisp)) isActive = Core.GameManager.HasPauseRequest(_freecam);

            if (isActive) lines[i] = $"<color=#{successHex}>{lines[i]}</color>";
        }
        return string.Join("\n", lines);
    }

    private bool LineContainsDisplay(string line, string display)
    {
        string cleanLine = System.Text.RegularExpressions.Regex.Replace(line, "<.*?>", string.Empty);
        string cleanDisplay = System.Text.RegularExpressions.Regex.Replace(display, "<.*?>", string.Empty);
        return cleanLine.Contains(cleanDisplay);
    }

    private InputAction FindAction(string path)
    {
        return Core.Player?.Inputs?.GetAction(path);
    }

    private static void ShowCanvasGroup(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private static void HideCanvasGroup(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}