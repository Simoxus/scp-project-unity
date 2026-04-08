using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConsole : MonoBehaviour
{
    private const float SCROLL_BOTTOM_THRESHOLD = 0.1f;
    private const string CMD_CLEAR_CONSOLE = "<CMD_CLEAR_CONSOLE>";
    private const string INPUT_PREFIX = "> ";

    [Space]
    public Canvas Canvas;

    [Header("Tab Buttons")]
    public Button ButtonCommands;
    public TextMeshProUGUI ButtonCommandsText;
    public Button ButtonLogs;
    public TextMeshProUGUI ButtonLogsText;

    [Header("Commands Panel")]
    public GameObject CommandsPanel;
    public TMP_InputField CommandInputField;
    public TMP_Text CommandsOutputText;
    public ScrollRect CommandsScrollRect;
    public TMP_Text AutocompleteSuggestionsText;

    [Header("Logs Panel")]
    public GameObject LogsPanel;
    public Button ButtonLogsClear;
    public TextMeshProUGUI LogsOutputText;
    public ScrollRect LogsScrollRect;

    [Header("Settings")]
    [SerializeField] private int maxCommandLines = 100;
    [SerializeField] private int maxLogLines = 200;

    // Commands state
    private string _initialCommandsText;
    private List<string> _history = new List<string>();
    private int _historyIndex = -1;
    private List<string> _currentSuggestions = new List<string>();
    private int _suggestionIndex = -1;

    // Logs state
    private List<string> _logLines = new List<string>();
    private StringBuilder _logBuffer = new StringBuilder();
    private List<string> _initialLogLines = new List<string>();

    // General state
    private PlayerInputs _inputs;
    private bool _isVisible;

    public bool IsVisible => _isVisible;

    private void Awake()
    {
        InitializeLogs();
        InitializeTabs();

        if (CommandsOutputText != null)
        {
            _initialCommandsText = CommandsOutputText.text;
        }
    }

    private void OnEnable()
    {
        ConsoleManager.OnConsoleMessage += HandleCommandsMessage;
        Application.logMessageReceived += HandleUnityLog;

        if (Core.Player != null)
        {
            _inputs = Core.Player.Inputs;
            _inputs.OnDebugUI += Toggle;
        }

        RebuildLogBufferAndDisplayText();
    }

    private void OnDisable()
    {
        ConsoleManager.OnConsoleMessage -= HandleCommandsMessage;
        Application.logMessageReceived -= HandleUnityLog;

        _inputs.OnDebugUI -= Toggle;

        if (CommandInputField != null)
        {
            CommandInputField.DeactivateInputField();
        }

        Core.GameManager.ReleasePauseIfRequested(this);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    private void Update()
    {
        if (_isVisible && CommandInputField != null && CommandInputField.isFocused)
        {
            HandleCommandsInput();
        }
    }

    public void Show()
    {
        if (Canvas == null) return;

        Canvas.enabled = true;
        _isVisible = true;

        Core.GameManager.RequestPause(this);
        _inputs.DisableGameplayInputs();
        _inputs.DisableFreecamInputs();
        _inputs.DisableKeypadInputs();

        CommandInputField.interactable = true;
        FocusOnCommandsInput();
    }

    public void Hide()
    {
        if (Canvas == null) return;

        Canvas.enabled = false;
        _isVisible = false;

        if (_inputs != null && !Core.GameManager.disablePlayerInputs)
        {
            _inputs.EnableGameplayInputs();
        }

        CommandInputField.interactable = false;
        CommandInputField.DeactivateInputField();
        CommandInputField.ReleaseSelection();

        Core.GameManager.ReleasePauseIfRequested(this);
    }

    public void Toggle()
    {
        if (_isVisible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void ForceClose()
    {
        if (IsVisible)
        {
            Hide();
        }
    }

    public void ShowCommandsTab()
    {
        PlayPressSound();
        CommandsPanel.SetActive(true);
        LogsPanel.SetActive(false);
        SetTabActive(true);
        FocusOnCommandsInput();
    }

    public void ShowLogsTab()
    {
        PlayPressSound();
        CommandsPanel.SetActive(false);
        LogsPanel.SetActive(true);
        SetTabActive(false);

        RebuildLogBufferAndDisplayText();

        LayoutRebuilder.ForceRebuildLayoutImmediate(LogsOutputText.rectTransform);
        LogsScrollRect.verticalNormalizedPosition = 0f;
    }

    public void FocusOnCommandsInput()
    {
        if (CommandInputField != null && CommandsPanel != null && CommandsPanel.activeInHierarchy)
        {
            CommandInputField.Select();
            CommandInputField.ActivateInputField();
        }
    }

    public void ClearLogs()
    {
        PlayPressSound();
        _logLines.Clear();
        RebuildLogBufferAndDisplayText();
        LogsScrollRect.verticalNormalizedPosition = 1f;
    }

    public List<string> GetLogs()
    {
        return new List<string>(_logLines);
    }

    public void OnInputEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessInputField(input);
            CommandInputField.ActivateInputField();
            CommandInputField.text = "";
        }
        else
        {
            ClearAutocompleteSuggestions();
        }
    }

    public void PopulateSuggestions(string currentInput)
    {
        if (AutocompleteSuggestionsText == null) return;

        _currentSuggestions.Clear();
        _suggestionIndex = -1;

        if (string.IsNullOrWhiteSpace(currentInput))
        {
            ClearAutocompleteSuggestions();
            return;
        }

        string commandWordPartial = currentInput.ToLower().Split(' ')[0];

        _currentSuggestions = ConsoleManager.Instance.GetCommandsForAutocomplete()
            .Where(kvp => kvp.Key.StartsWith(commandWordPartial))
            .Select(kvp => kvp.Value.CommandWord.ToLower())
            .Distinct()
            .OrderBy(cmd => cmd)
            .ToList();

        DisplaySuggestions();
    }

    public void CopyLogsOutputToClipboard()
    {
        GUIUtility.systemCopyBuffer = GetLogsPlainOutput();
    }

    private void InitializeLogs()
    {
        if (LogsOutputText != null)
        {
            string[] initialLines = LogsOutputText.text.Split(
                new char[] { '\n', '\r' },
                System.StringSplitOptions.RemoveEmptyEntries
            );
            _initialLogLines.AddRange(initialLines);
        }
    }

    private void InitializeTabs()
    {
        ShowCommandsTab();
        Hide();
    }

    private void SetTabActive(bool commandsActive)
    {
        if (ButtonCommandsText != null)
        {
            ButtonCommandsText.text = commandsActive ? "<b>COMMANDS</b>" : "COMMANDS";
            ButtonCommandsText.color = commandsActive ? Color.white : Color.gray;
        }

        if (ButtonLogsText != null)
        {
            ButtonLogsText.text = commandsActive ? "LOGS" : "<b>LOGS</b>";
            ButtonLogsText.color = commandsActive ? Color.gray : Color.white;
        }
    }

    private void HandleCommandsInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CycleHistory(true);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CycleHistory(false);
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleSuggestions();
        }
    }

    private void ProcessInputField(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            ClearAutocompleteSuggestions();
            return;
        }

        if (_history.Count == 0 || _history[_history.Count - 1] != input)
        {
            _history.Add(input);
        }

        _historyIndex = _history.Count;

        AppendToCommands($"{INPUT_PREFIX}{input}".AsInput());
        Core.ConsoleManager.ProcessCommand(input);
        ClearAutocompleteSuggestions();
    }

    private void CycleHistory(bool backward)
    {
        if (_history.Count == 0) return;

        if (backward)
        {
            _historyIndex = Mathf.Max(0, _historyIndex - 1);
        }
        else
        {
            _historyIndex++;

            if (_historyIndex >= _history.Count)
            {
                _historyIndex = _history.Count;
                CommandInputField.text = "";
                ClearAutocompleteSuggestions();
                return;
            }
        }

        CommandInputField.text = _history[_historyIndex];
        CommandInputField.MoveTextEnd(true);
        ClearAutocompleteSuggestions();
    }

    private void CycleSuggestions()
    {
        if (_currentSuggestions.Count == 0) return;

        _suggestionIndex = (_suggestionIndex + 1) % _currentSuggestions.Count;

        CommandInputField.text = _currentSuggestions[_suggestionIndex];
        CommandInputField.MoveTextEnd(false);

        DisplaySuggestions();
    }

    private void DisplaySuggestions()
    {
        if (_currentSuggestions.Count == 0)
        {
            ClearAutocompleteSuggestions();
            return;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < _currentSuggestions.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('\n');
            }

            string suggestion = _currentSuggestions[i];
            sb.Append(i == _suggestionIndex ? $"<b>{suggestion}</b>".AsSuccess() : suggestion);
        }

        AutocompleteSuggestionsText.text = sb.ToString();
    }

    private void ClearAutocompleteSuggestions()
    {
        AutocompleteSuggestionsText.text = "";
        _currentSuggestions.Clear();
        _suggestionIndex = -1;
    }

    private void AppendToCommands(string message)
    {
        if (CommandsOutputText == null) return;

        bool atBottom = false;
        if (CommandsScrollRect != null)
        {
            atBottom = CommandsScrollRect.verticalNormalizedPosition <= SCROLL_BOTTOM_THRESHOLD;
        }

        CommandsOutputText.text += message + "\n";

        string[] lines = CommandsOutputText.text.Split('\n');
        if (lines.Length > maxCommandLines)
        {
            CommandsOutputText.text = string.Join("\n", lines.Skip(lines.Length - maxCommandLines).ToArray());
        }

        if (CommandsScrollRect != null && atBottom)
        {
            Canvas.ForceUpdateCanvases();
            CommandsScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void HandleCommandsMessage(string message)
    {
        if (message == CMD_CLEAR_CONSOLE)
        {
            CommandsOutputText.text = "";
            AppendToCommands(_initialCommandsText + "Commands cleared.".AsInfo());
        }
        else
        {
            AppendToCommands(message);
        }
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        string colorTag = GetColorForLogType(type);
        _logLines.Add($"{colorTag}[{type}] {logString}</color>");

        while (_logLines.Count > maxLogLines)
        {
            _logLines.RemoveAt(0);
        }

        if (LogsPanel != null && LogsPanel.activeInHierarchy)
        {
            RebuildLogBufferAndDisplayText();
        }
    }

    private string GetColorForLogType(LogType type)
    {
        Color color = type switch
        {
            LogType.Error => ColorScheme.Error,
            LogType.Exception => ColorScheme.Exception,
            LogType.Warning => ColorScheme.Warning,
            LogType.Assert or LogType.Log => ColorScheme.Info,
            _ => Color.white
        };
        return $"<color={ColorScheme.ToHex(color)}>";
    }

    private string GetLogsPlainOutput()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var line in _logLines)
        {
            string plain = System.Text.RegularExpressions.Regex.Replace(line, "<.*?>", "");
            plain = System.Text.RegularExpressions.Regex.Replace(plain, @"^\[(?!EXCEPTION).*?\]\s*", "");
            if (!string.IsNullOrWhiteSpace(plain)) sb.AppendLine(plain);
        }

        return sb.ToString().TrimEnd();
    }

    private void RebuildLogBufferAndDisplayText()
    {
        _logBuffer.Clear();

        foreach (string line in _initialLogLines)
        {
            _logBuffer.AppendLine(line);
        }

        foreach (string line in _logLines)
        {
            _logBuffer.AppendLine(line);
        }

        if (LogsOutputText != null)
        {
            LogsOutputText.text = _logBuffer.ToString();
        }
    }

    private void PlayPressSound()
    {
        FMODHelper.PlayOneShot(Core.AudioDataAccess.UI.PressSound);
    }
}