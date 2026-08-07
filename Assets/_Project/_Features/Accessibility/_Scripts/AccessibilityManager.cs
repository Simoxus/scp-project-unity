using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central settings hub for the accessibility layer. Created automatically by AccessibilityBootstrap;
/// lives across scene loads. Persists settings in PlayerPrefs under A11y_* keys (self-contained on
/// purpose: no dependency on SettingsManager so the layer survives upstream settings refactors).
/// </summary>
public class AccessibilityManager : MonoBehaviour
{
    public static AccessibilityManager Instance { get; private set; }

    [Header("Sonar Settings")]
    public bool sonarEnabled = true;
    // Continuous parking-sensor beeping is opt-in: blind QA preferred the on-demand
    // scan (B) after playing TLOU's Enhanced Listen Mode ("I like firing it when I
    // actually need it"). Shift+F6 toggles it back on.
    public bool sonarContinuous = false;
    [Range(0f, 1f)] public float sonarVolume = 0.8f;
    [Range(3f, 30f)] public float sonarRadius = 12f;

    public event Action OnAccessibilitySettingsChanged;

    private InputAction _toggleSonarAction;
    private InputAction _toggleContinuousAction;

    public static class Keys
    {
        public const string SonarEnabled = "A11y_SonarEnabled";
        public const string SonarContinuous = "A11y_SonarContinuous";
        public const string SonarVolume = "A11y_SonarVolume";
        public const string SonarRadius = "A11y_SonarRadius";
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();

        if (GetComponent<ProximitySonar>() == null)
        {
            gameObject.AddComponent<ProximitySonar>();
        }

        if (GetComponent<UIReader>() == null)
        {
            gameObject.AddComponent<UIReader>();
        }

        if (GetComponent<SoundGlossary>() == null)
        {
            gameObject.AddComponent<SoundGlossary>();
        }
    }

    private void Start()
    {
        // Immediate, audible confirmation for screen reader users that the layer is up
        ScreenReaderOutput.Speak("Capa de accesibilidad activa. B escanea el área, H dice tu estado. Con el juego en pausa, G abre el glosario de sonidos.");
    }

    private void OnEnable()
    {
        // Standalone InputActions so the game's shared InputActionAsset stays untouched
        _toggleSonarAction = new InputAction("A11yToggleSonar", binding: "<Keyboard>/f6");
        _toggleSonarAction.performed += OnToggleSonarPerformed;
        _toggleSonarAction.Enable();

        _toggleContinuousAction = new InputAction("A11yToggleContinuous");
        _toggleContinuousAction.AddCompositeBinding("ButtonWithOneModifier")
            .With("Modifier", "<Keyboard>/leftShift")
            .With("Button", "<Keyboard>/f6");
        _toggleContinuousAction.performed += OnToggleContinuousPerformed;
        _toggleContinuousAction.Enable();
    }

    private void OnDisable()
    {
        if (_toggleSonarAction != null)
        {
            _toggleSonarAction.performed -= OnToggleSonarPerformed;
            _toggleSonarAction.Disable();
            _toggleSonarAction.Dispose();
            _toggleSonarAction = null;
        }
        if (_toggleContinuousAction != null)
        {
            _toggleContinuousAction.performed -= OnToggleContinuousPerformed;
            _toggleContinuousAction.Disable();
            _toggleContinuousAction.Dispose();
            _toggleContinuousAction = null;
        }
    }

    private void OnToggleSonarPerformed(InputAction.CallbackContext ctx)
    {
        // Shift+F6 is the continuous-mode gesture; plain F6 must not also fire then
        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)) return;
        SetSonarEnabled(!sonarEnabled);
    }

    private void OnToggleContinuousPerformed(InputAction.CallbackContext ctx)
    {
        sonarContinuous = !sonarContinuous;
        SaveSettings();
        OnAccessibilitySettingsChanged?.Invoke();
        ScreenReaderOutput.Speak(sonarContinuous ? "Sonar continuo activado." : "Sonar continuo desactivado.", true);
    }

    public void SetSonarEnabled(bool value)
    {
        sonarEnabled = value;
        SaveSettings();
        OnAccessibilitySettingsChanged?.Invoke();
        ScreenReaderOutput.Speak(value ? "Sonar activado." : "Sonar desactivado.", true);
        Debug.Log($"[Accessibility] Sonar {(value ? "enabled" : "disabled")}.");
    }

    public void SetSonarVolume(float value)
    {
        sonarVolume = Mathf.Clamp01(value);
        SaveSettings();
        OnAccessibilitySettingsChanged?.Invoke();
    }

    public void SetSonarRadius(float value)
    {
        sonarRadius = Mathf.Clamp(value, 3f, 30f);
        SaveSettings();
        OnAccessibilitySettingsChanged?.Invoke();
    }

    public void LoadSettings()
    {
        sonarEnabled = PlayerPrefs.GetInt(Keys.SonarEnabled, 1) == 1;
        sonarContinuous = PlayerPrefs.GetInt(Keys.SonarContinuous, 0) == 1;
        sonarVolume = PlayerPrefs.GetFloat(Keys.SonarVolume, 0.8f);
        sonarRadius = PlayerPrefs.GetFloat(Keys.SonarRadius, 12f);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt(Keys.SonarEnabled, sonarEnabled ? 1 : 0);
        PlayerPrefs.SetInt(Keys.SonarContinuous, sonarContinuous ? 1 : 0);
        PlayerPrefs.SetFloat(Keys.SonarVolume, sonarVolume);
        PlayerPrefs.SetFloat(Keys.SonarRadius, sonarRadius);
        PlayerPrefs.Save();
    }
}
