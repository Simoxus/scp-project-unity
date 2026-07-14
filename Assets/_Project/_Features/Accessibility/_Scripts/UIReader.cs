using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Generic screen reader support for uGUI: announces the scene on load, keeps something
/// selected so keyboard navigation works, and speaks the focused element (alt text from
/// A11yAltText first, visible label second, humanized object name last, plus its role).
/// Maps to gameaccessibilityguidelines.com (Vision): "Provide pre-recorded voiceovers for
/// all text including menus" (via the player's own screen reader TTS).
/// </summary>
public class UIReader : MonoBehaviour
{
    private const float AutoSelectInterval = 0.5f;

    private GameObject _lastSelected;
    private float _nextAutoSelectTime;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _lastSelected = null;
        if (A11yAltText.TryGet("scene:" + scene.name, out string sceneText))
        {
            ScreenReaderOutput.Speak(sceneText);
        }
    }

    private void Update()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        GameObject selected = eventSystem.currentSelectedGameObject;

        if (selected == null)
        {
            TryAutoSelect(eventSystem);
            return;
        }

        if (selected != _lastSelected)
        {
            _lastSelected = selected;
            ScreenReaderOutput.Speak(DescribeElement(selected), true);
        }
    }

    // Menus should always have a focused element, otherwise keyboard (and screen reader)
    // navigation has no starting point. Gated on the cursor being free: during first-person
    // gameplay the cursor is locked, and grabbing stray HUD sliders there hijacks the
    // keyboard (found by QA in Testing_Core).
    private void TryAutoSelect(EventSystem eventSystem)
    {
        if (Cursor.lockState == CursorLockMode.Locked) return;
        if (Time.unscaledTime < _nextAutoSelectTime) return;
        _nextAutoSelectTime = Time.unscaledTime + AutoSelectInterval;

        var selectables = Selectable.allSelectablesArray;
        for (int i = 0; i < selectables.Length; i++)
        {
            var candidate = selectables[i];
            if (candidate != null && candidate.IsActive() && candidate.IsInteractable())
            {
                eventSystem.SetSelectedGameObject(candidate.gameObject);
                return;
            }
        }
    }

    public static string DescribeElement(GameObject element)
    {
        string label = ResolveLabel(element);
        string role = ResolveRole(element);
        return string.IsNullOrEmpty(role) ? label : $"{label}, {role}";
    }

    private static string ResolveLabel(GameObject element)
    {
        // 1) Curated alt text beats everything (covers images with baked-in text)
        if (A11yAltText.TryGet(element.name, out string altText)) return altText;

        // 2) The visible label of the control
        var tmpLabel = element.GetComponentInChildren<TMP_Text>(true);
        if (tmpLabel != null && !string.IsNullOrWhiteSpace(tmpLabel.text)) return tmpLabel.text;

        var legacyLabel = element.GetComponentInChildren<Text>(true);
        if (legacyLabel != null && !string.IsNullOrWhiteSpace(legacyLabel.text)) return legacyLabel.text;

        // 3) Whatever the developers named the object
        return A11yAltText.HumanizeName(element.name);
    }

    private static string ResolveRole(GameObject element)
    {
        if (element.GetComponent<Button>() != null) return "botón";
        if (element.GetComponent<Toggle>() != null) return "interruptor";
        if (element.GetComponent<Slider>() != null) return "deslizador";
        if (element.GetComponent<TMP_InputField>() != null || element.GetComponent<InputField>() != null) return "campo de texto";
        if (element.GetComponent<TMP_Dropdown>() != null || element.GetComponent<Dropdown>() != null) return "lista desplegable";
        if (element.GetComponent<Scrollbar>() != null) return "barra de desplazamiento";
        return element.GetComponent<Selectable>() != null ? "control" : string.Empty;
    }
}
