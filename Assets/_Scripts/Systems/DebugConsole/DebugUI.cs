using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FMODUnity;

public class DebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIAccess uiAccess;

    [Header("UI Elements")]
    public Button buttonConsole;
    public TextMeshProUGUI buttonConsoleText;
    public Button buttonLogs;
    public TextMeshProUGUI buttonLogsText;
    public GameObject panelConsole;
    public GameObject panelLogs;
    public Button buttonLogsClear;
    public TextMeshProUGUI logsOutputText;
    public ScrollRect logsScrollRect;

    [Header("Log Settings")] // New header for log-specific settings
    public int maxLogLines = 200; // NEW: Max number of lines to display, adjustable in Inspector

    private StringBuilder logBuffer = new StringBuilder();
    private const float SCROLL_BOTTOM_THRESHOLD = 0.01f;
    private string _initalLogText; // To keep the snarky ass line
    private List<string> _currentLogLines = new List<string>(); // NEW: To manage lines for trimming

    private void Awake()
    {
        if (panelConsole != null) panelConsole.SetActive(true);
        if (panelLogs != null) panelLogs.SetActive(false);

        if (buttonConsoleText != null)
        {
            buttonConsoleText.text = "<b>CONSOLE</b>";
            buttonConsoleText.color = Color.white;
        }
        if (buttonLogsText != null)
        {
            buttonLogsText.text = "LOGS";
            buttonLogsText.color = Color.gray;
        }

        _initalLogText = logsOutputText.text;

        // Add the initial text to our line buffer
        // Split and add line by line to handle multi-line initial text correctly
        string[] initialLines = _initalLogText.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in initialLines)
        {
            _currentLogLines.Add(line);
        }

        // Initialize the logBuffer with the initial text.
        // We do this here once, and then `HandleLog` will update `logsOutputText.text` directly from `_currentLogLines`.
        RebuildLogBufferAndDisplayText();
    }

    private void OnEnable()
    {
        if (buttonLogsClear != null)
        {
            buttonLogsClear.onClick.AddListener(ClearLogs);
        }
        if (buttonConsole != null)
        {
            buttonConsole.onClick.AddListener(ShowConsole);
        }
        if (buttonLogs != null)
        {
            buttonLogs.onClick.AddListener(ShowLogs);
        }

        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        if (buttonLogsClear != null)
        {
            buttonLogsClear.onClick.RemoveListener(ClearLogs);
        }
        if (buttonConsole != null)
        {
            buttonConsole.onClick.RemoveListener(ShowConsole);
        }
        if (buttonLogs != null)
        {
            buttonLogs.onClick.RemoveListener(ShowLogs);
        }

        Application.logMessageReceived -= HandleLog;
    }

    public void ShowConsole()
    {
        PlayPressSound();

        if (panelConsole != null)
        {
            panelConsole.SetActive(true);
            if (buttonConsoleText != null)
            {
                buttonConsoleText.text = "<b>CONSOLE</b>";
                buttonConsoleText.color = Color.white;
            }
        }
        if (panelLogs != null)
        {
            panelLogs.SetActive(false);
            if (buttonLogsText != null)
            {
                buttonLogsText.text = "LOGS";
                buttonLogsText.color = Color.gray;
            }
        }
    }

    public void ShowLogs()
    {
        PlayPressSound();

        if (panelConsole != null)
        {
            panelConsole.SetActive(false);
            if (buttonConsoleText != null)
            {
                buttonConsoleText.text = "CONSOLE";
                buttonConsoleText.color = Color.gray;
            }
        }
        if (panelLogs != null)
        {
            panelLogs.SetActive(true);
            if (buttonLogsText != null)
            {
                buttonLogsText.text = "<b>LOGS</b>";
                buttonLogsText.color = Color.white;
            }
        }
        if (logsScrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(logsOutputText.rectTransform);
            logsScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        bool atBottom = false;
        if (logsScrollRect != null)
        {
            atBottom = logsScrollRect.verticalNormalizedPosition <= SCROLL_BOTTOM_THRESHOLD;
        }

        string colorTag = "";
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                colorTag = "<color=red>";
                break;
            case LogType.Warning:
                colorTag = "<color=orange>";
                break;
            case LogType.Assert:
                colorTag = "<color=yellow>";
                break;
            case LogType.Log:
            default:
                colorTag = "<color=white>";
                break;
        }

        string formattedLog = $"{colorTag}[{type}] {logString}</color>"; // No newline here, it's added during join

        // Add new log to the list
        _currentLogLines.Add(formattedLog);

        while (_currentLogLines.Count > maxLogLines)
        {
            _currentLogLines.RemoveAt(0); // Remove the oldest line
        }

        RebuildLogBufferAndDisplayText();

        if (logsScrollRect != null && atBottom)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(logsOutputText.rectTransform);
            logsScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // NEW: Helper method to rebuild the StringBuilder and update the TextMeshPro text
    private void RebuildLogBufferAndDisplayText()
    {
        logBuffer.Clear();
        // Append all current lines from the list, adding a newline after each
        for (int i = 0; i < _currentLogLines.Count; i++)
        {
            logBuffer.AppendLine(_currentLogLines[i]);
        }

        if (logsOutputText != null)
        {
            logsOutputText.text = logBuffer.ToString();
        }
    }

    public void ClearLogs()
    {
        PlayPressSound();

        _currentLogLines.Clear(); // Clears all logs, including initial text for a moment

        // Re-add the initial text
        string[] initialLines = _initalLogText.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in initialLines)
        {
            _currentLogLines.Add(line);
        }

        RebuildLogBufferAndDisplayText(); // Update display with only initial text

        if (logsScrollRect != null)
        {
            logsScrollRect.verticalNormalizedPosition = 1f; // Scroll to top on clear
        }
    }

    private void PlayPressSound()
    {
        RuntimeManager.PlayOneShot(uiAccess.uiPressEvent);
    }
}