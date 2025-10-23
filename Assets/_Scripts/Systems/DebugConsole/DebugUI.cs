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

    [Header("Log Settings")]
    public int maxLogLines = 200;

    private StringBuilder logBuffer = new StringBuilder();
    private List<string> _initialLogLines = new List<string>();

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

        // Store the initial text permanently
        string initialText = logsOutputText.text;
        string[] initialLines = initialText.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in initialLines)
        {
            _initialLogLines.Add(line);
        }

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

        RefreshLogs();
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

        RefreshLogs();

        if (logsScrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(logsOutputText.rectTransform);
            logsScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void RefreshLogs()
    {
        if (DebugManager.Instance != null)
        {
            RebuildLogBufferAndDisplayText();
        }
    }

    private void RebuildLogBufferAndDisplayText()
    {
        logBuffer.Clear();

        // Always add initial lines first
        for (int i = 0; i < _initialLogLines.Count; i++)
        {
            logBuffer.AppendLine(_initialLogLines[i]);
        }

        // Then add logs from DebugManager
        if (DebugManager.Instance != null)
        {
            List<string> logs = DebugManager.Instance.GetLogs();
            for (int i = 0; i < logs.Count; i++)
            {
                logBuffer.AppendLine(logs[i]);
            }
        }

        if (logsOutputText != null)
        {
            logsOutputText.text = logBuffer.ToString();
        }
    }

    public void ClearLogs()
    {
        PlayPressSound();

        if (DebugManager.Instance != null)
        {
            DebugManager.Instance.ClearLogs();
        }

        RebuildLogBufferAndDisplayText();

        if (logsScrollRect != null)
        {
            logsScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void PlayPressSound()
    {
        FMODHelper.PlayOneShot(uiAccess.uiPressEvent);
    }
}