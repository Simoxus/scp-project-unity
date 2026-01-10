using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConsole : MonoBehaviour
{
    [Header("UI Base")]
    public Canvas canvas;

    [Header("Tab Buttons")]
    public Button buttonCommands;
    public TextMeshProUGUI buttonCommandsText;
    public Button buttonLogs;
    public TextMeshProUGUI buttonLogsText;

    [Header("Commands Panel")]
    public GameObject commandsPanel;
    public TMP_InputField commandInputField;
    public TMP_Text commandsOutputText;
    public ScrollRect commandsScrollRect;
    public TMP_Text autocompleteSuggestionsText;

    [Header("Logs Panel")]
    public GameObject logsPanel;
    public Button buttonLogsClear;
    public TextMeshProUGUI logsOutputText;
    public ScrollRect logsScrollRect;

    [Header("Settings")]
    public int maxCommandsLines = 100;
    public int maxLogLines = 200;

    // Commands state
    private const float SCROLL_BOTTOM_THRESHOLD = 0.1f;
    private string _initialOutputText;
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
        ValidateCanvas();
        InitializeCommands();
        InitializeLogs();
        InitializeTabs();
    }

    private void OnEnable()
    {
        // Commands events
        ConsoleManager.OnConsoleMessage += HandleCommandsMessage;

        // Unity log events
        Application.logMessageReceived += HandleUnityLog;

        // Button events
        if (buttonCommands != null)
            buttonCommands.onClick.AddListener(ShowCommandsTab);
        if (buttonLogs != null)
            buttonLogs.onClick.AddListener(ShowLogsTab);
        if (buttonLogsClear != null)
            buttonLogsClear.onClick.AddListener(ClearLogs);

        // Input events
        if (Core.Player != null)
        {
            _inputs = Core.Player.PlayerInputs;
            if (_inputs != null)
                _inputs.OnDebugUI += Toggle;
        }

        RebuildLogBufferAndDisplayText();
    }

    private void OnDisable()
    {
        // Commands events
        ConsoleManager.OnConsoleMessage -= HandleCommandsMessage;

        // Unity log events
        Application.logMessageReceived -= HandleUnityLog;

        // Button events
        if (buttonCommands != null)
            buttonCommands.onClick.RemoveListener(ShowCommandsTab);
        if (buttonLogs != null)
            buttonLogs.onClick.RemoveListener(ShowLogsTab);
        if (buttonLogsClear != null)
            buttonLogsClear.onClick.RemoveListener(ClearLogs);

        // Input events
        if (_inputs != null)
            _inputs.OnDebugUI -= Toggle;

        if (commandInputField != null)
        {
            commandInputField.DeactivateInputField();
        }

        ReleasePauseIfNeeded();
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    private void Update()
    {
        if (commandInputField != null && commandInputField.isFocused)
        {
            HandleCommandsInput();
        }
    }

    public void Show()
    {
        if (canvas == null)
        {
            Log.Error("UIConsole: Cannot show UI - Canvas is null");
            return;
        }

        canvas.enabled = true;
        _isVisible = true;

        // Disable other input contexts when console is open
        if (_inputs != null)
        {
            _inputs.DisableGameplayInputs();
            _inputs.DisableFreecamInputs();
            _inputs.DisableKeypadInputs();
        }

        FocusOnCommandsInput();

        if (Core.GameManager != null)
            Core.GameManager.RequestPause(this);
    }

    public void Hide()
    {
        if (canvas == null)
        {
            Log.Error("UIConsole: Cannot hide UI - Canvas is null");
            return;
        }

        if (commandInputField != null)
        {
            commandInputField.DeactivateInputField();
            commandInputField.ReleaseSelection();
        }

        canvas.enabled = false;
        _isVisible = false;

        // Re-enable gameplay inputs when console closes (if not paused by something else)
        if (_inputs != null && Core.GameManager != null && !Core.GameManager.disablePlayerInputs)
        {
            _inputs.EnableGameplayInputs();
        }

        ReleasePauseIfNeeded();
    }

    public void Toggle()
    {
        if (_isVisible)
            Hide();
        else
            Show();
    }

    public void ForceClose()
    {
        if (IsVisible)
            Hide();
    }

    public void ShowCommandsTab()
    {
        PlayPressSound();
        if (commandsPanel != null) commandsPanel.SetActive(true);
        if (logsPanel != null) logsPanel.SetActive(false);
        SetTabActive(true);
        FocusOnCommandsInput();
    }

    public void ShowLogsTab()
    {
        PlayPressSound();
        if (commandsPanel != null) commandsPanel.SetActive(false);
        if (logsPanel != null) logsPanel.SetActive(true);
        SetTabActive(false);

        RebuildLogBufferAndDisplayText();

        if (logsScrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(logsOutputText.rectTransform);
            logsScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void FocusOnCommandsInput()
    {
        if (commandInputField != null && commandsPanel != null && commandsPanel.activeInHierarchy)
        {
            commandInputField.Select();
            commandInputField.ActivateInputField();
        }
    }

    public void ClearLogs()
    {
        PlayPressSound();
        _logLines.Clear();
        RebuildLogBufferAndDisplayText();

        if (logsScrollRect != null)
        {
            logsScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public List<string> GetLogs()
    {
        return new List<string>(_logLines);
    }

    private void ValidateCanvas()
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                Log.Warning("UIConsole: Canvas was missing and has been added automatically.");
            }
        }
    }

    private void InitializeCommands()
    {
        if (commandsOutputText != null)
            _initialOutputText = commandsOutputText.text;

        if (commandInputField != null)
        {
            commandInputField.onEndEdit.AddListener(OnInputEndEdit);
            commandInputField.onValueChanged.AddListener(OnInputFieldChanged);
        }

        ClearAutocompleteSuggestions();
    }

    private void InitializeLogs()
    {
        if (logsOutputText != null)
        {
            string initialText = logsOutputText.text;
            string[] initialLines = initialText.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            _initialLogLines.AddRange(initialLines);
        }
    }

    private void InitializeTabs()
    {
        // Start with commands tab active
        ShowCommandsTab();

        // Start hidden
        Hide();
    }

    private void SetTabActive(bool commandsActive)
    {
        if (buttonCommandsText != null)
        {
            buttonCommandsText.text = commandsActive ? "<b>COMMANDS</b>" : "COMMANDS";
            buttonCommandsText.color = commandsActive ? Color.white : Color.gray;
        }
        if (buttonLogsText != null)
        {
            buttonLogsText.text = commandsActive ? "LOGS" : "<b>LOGS</b>";
            buttonLogsText.color = commandsActive ? Color.gray : Color.white;
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

    private void OnInputEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessInputField(input);
            commandInputField.ActivateInputField();
            commandInputField.text = "";
        }
        else
        {
            ClearAutocompleteSuggestions();
        }
    }

    private void OnInputFieldChanged(string input)
    {
        PopulateSuggestions(input);
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

        AppendToCommands($"> {input}".AsInput());

        if (ConsoleManager.Instance != null)
        {
            ConsoleManager.Instance.ProcessCommand(input);
        }
        else
        {
            AppendToCommands("ConsoleManager not found!".AsError());
        }

        ClearAutocompleteSuggestions();
    }

    private void CycleHistory(bool backward)
    {
        if (_history.Count == 0) return;

        if (backward)
        {
            _historyIndex--;
            if (_historyIndex < 0)
                _historyIndex = 0;
        }
        else
        {
            _historyIndex++;
            if (_historyIndex >= _history.Count)
            {
                if (_historyIndex == _history.Count)
                {
                    commandInputField.text = "";
                    ClearAutocompleteSuggestions();
                    return;
                }
                _historyIndex = _history.Count - 1;
            }
        }

        if (_historyIndex >= 0 && _historyIndex < _history.Count)
        {
            commandInputField.text = _history[_historyIndex];
            commandInputField.MoveTextEnd(true);
        }
        ClearAutocompleteSuggestions();
    }

    private void PopulateSuggestions(string currentInput)
    {
        if (autocompleteSuggestionsText == null || ConsoleManager.Instance == null)
            return;

        _currentSuggestions.Clear();
        _suggestionIndex = -1;

        if (string.IsNullOrWhiteSpace(currentInput))
        {
            ClearAutocompleteSuggestions();
            return;
        }

        string lowerInput = currentInput.ToLower();
        string commandWordPartial = lowerInput.Split(' ')[0];

        _currentSuggestions = ConsoleManager.Instance.GetCommands().Keys
            .Where(cmd => cmd.StartsWith(commandWordPartial))
            .OrderBy(cmd => cmd)
            .ToList();

        DisplaySuggestions();
    }

    private void CycleSuggestions()
    {
        if (_currentSuggestions.Count == 0)
            return;

        _suggestionIndex = (_suggestionIndex + 1) % _currentSuggestions.Count;
        string selectedSuggestion = _currentSuggestions[_suggestionIndex];

        commandInputField.text = selectedSuggestion;
        commandInputField.MoveTextEnd(false);

        DisplaySuggestions();
    }

    private void DisplaySuggestions()
    {
        if (autocompleteSuggestionsText == null) return;

        if (_currentSuggestions.Count == 0)
        {
            ClearAutocompleteSuggestions();
            return;
        }

        string suggestionsString = "";
        for (int i = 0; i < _currentSuggestions.Count; i++)
        {
            if (i > 0)
                suggestionsString += "\n";

            if (i == _suggestionIndex)
                suggestionsString += $"<b>{_currentSuggestions[i]}</b>".AsSuccess();
            else
                suggestionsString += _currentSuggestions[i];
        }
        autocompleteSuggestionsText.text = suggestionsString;
    }

    private void ClearAutocompleteSuggestions()
    {
        if (autocompleteSuggestionsText != null)
            autocompleteSuggestionsText.text = "";
        _currentSuggestions.Clear();
        _suggestionIndex = -1;
    }

    private void AppendToCommands(string message)
    {
        if (commandsOutputText == null) return;

        bool atBottom = false;
        if (commandsScrollRect != null)
        {
            atBottom = commandsScrollRect.verticalNormalizedPosition <= SCROLL_BOTTOM_THRESHOLD;
        }

        commandsOutputText.text += message + "\n";

        string[] lines = commandsOutputText.text.Split('\n');
        if (lines.Length > maxCommandsLines)
        {
            commandsOutputText.text = string.Join("\n", lines.Skip(lines.Length - maxCommandsLines).ToArray());
        }

        if (commandsScrollRect != null && atBottom)
        {
            Canvas.ForceUpdateCanvases();
            commandsScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void HandleCommandsMessage(string message)
    {
        if (message == "<CMD_CLEAR_CONSOLE>")
        {
            commandsOutputText.text = "";
            AppendToCommands(_initialOutputText + "Commands cleared.".AsInfo());
        }
        else
        {
            AppendToCommands(message);
        }
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        string colorTag = GetColorForLogType(type);
        string formattedLog = $"{colorTag}[{type}] {logString}</color>";

        _logLines.Add(formattedLog);

        while (_logLines.Count > maxLogLines)
        {
            _logLines.RemoveAt(0);
        }

        if (logsPanel != null && logsPanel.activeInHierarchy)
        {
            RebuildLogBufferAndDisplayText();
        }
    }

    private string GetColorForLogType(LogType type)
    {
        Color color = type switch
        {
            LogType.Error or LogType.Exception => ColorScheme.Error,
            LogType.Warning => ColorScheme.Warning,
            LogType.Assert or LogType.Log => ColorScheme.Info,
            _ => Color.white
        };
        return $"<color={ColorScheme.ToHex(color)}>";
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

        if (logsOutputText != null)
        {
            logsOutputText.text = _logBuffer.ToString();
        }
    }

    private void ReleasePauseIfNeeded()
    {
        if (Core.GameManager != null && Core.GameManager.HasPauseRequest(this))
            Core.GameManager.ReleasePause(this);
    }

    private void PlayPressSound()
    {
        if (Core.AudioDataAccess?.UI != null)
            FMODHelper.PlayOneShot(AudioDataAccess.Instance.UI.PressSound);
    }
}