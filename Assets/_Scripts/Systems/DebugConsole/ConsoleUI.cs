using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ConsoleUI : MonoBehaviour
{
    [Header("Elements")]
    public TMP_InputField commandInputField;
    public TMP_Text consoleOutputText;
    public ScrollRect scrollRect;
    public TMP_Text autocompleteSuggestionsText; // For displaying autocomplete suggestions

    [Header("Console Settings")]
    public const int MAX_LINES = 100;

    private const float SCROLL_BOTTOM_THRESHOLD = 0.1f; // User is considered at the bottom if within 10% of the bottom

    private string _initalOutputText; // To keep the snarky ass line I put in when you clear the output
    private List<string> _history = new List<string>();
    private int _historyIndex = -1;

    // Autocomplete variables
    private List<string> _currentSuggestions = new List<string>(); // Stores filtered suggestions
    private int _suggestionIndex = -1; // Index of the currently highlighted suggestion

    public void FocusOnInput()
    {
        commandInputField.Select();
        commandInputField.ActivateInputField();
    }

    private void Awake()
    {
        if (commandInputField == null)
        {
            Debug.LogError("ConsoleUI: commandInputField is not assigned!", this);
            return;
        }
        if (consoleOutputText == null)
        {
            Debug.LogError("ConsoleUI: consoleOutputText is not assigned!", this);
            return;
        }
        if (scrollRect == null)
        {
            Debug.LogWarning("ConsoleUI: scrollRect is not assigned. Autoscrolling will not work.", this);
        }
        if (autocompleteSuggestionsText == null)
        {
            Debug.LogWarning("ConsoleUI: autocompleteSuggestionsText is not assigned. Autocomplete suggestions will not be displayed.", this);
        }

        _initalOutputText = consoleOutputText.text;
        commandInputField.onEndEdit.AddListener(OnInputEndEdit);
        commandInputField.onValueChanged.AddListener(OnInputFieldChanged);
        ClearAutocompleteSuggestions(); // Clear suggestions initially
    }

    private void OnEnable()
    {
        ConsoleManager.OnConsoleMessage += HandleConsoleMessage;
    }

    private void OnDisable()
    {
        ConsoleManager.OnConsoleMessage -= HandleConsoleMessage;
    }

    private void Update()
    {
        if (commandInputField.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                CycleHistory(true); // Move back in history
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                CycleHistory(false); // Move forward in history
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleSuggestions();
                // Consume the event to prevent tabbing to other UI elements.
                // Event.current.Use() is typically used in OnGUI, but leaving a comment
                // here as a reminder of intent. For Update, direct input blocking
                // or correct EventSystem setup is more reliable if needed.
            }
        }
    }

    private void OnInputEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessInputField(input);
            commandInputField.ActivateInputField(); // Keep input field focused after pressing Enter.
            commandInputField.text = ""; // Clear the input field.
        }
        else
        {
            ClearAutocompleteSuggestions(); // Clear suggestions when input field loses focus
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

        // Add command to history
        if (_history.Count == 0 || _history[_history.Count - 1] != input)
        {
            _history.Add(input);
        }
        _historyIndex = _history.Count; // Reset history index to end

        // Log the input command to the console output.
        AppendToConsole($"<color=grey>> {input}</color>");

        if (ConsoleManager.Instance != null)
        {
            ConsoleManager.Instance.ProcessCommand(input);
        }
        else
        {
            AppendToConsole("<color=red>Error: ConsoleCommandManager not found!</color>");
        }

        ClearAutocompleteSuggestions(); // Clear suggestions after processing command
    }

    private void AppendToConsole(string message)
    {
        if (consoleOutputText == null) return;

        bool atBottom = false;
        if (scrollRect != null)
        {
            atBottom = scrollRect.verticalNormalizedPosition <= SCROLL_BOTTOM_THRESHOLD;
        }

        consoleOutputText.text += message + "\n";

        string[] lines = consoleOutputText.text.Split('\n');
        if (lines.Length > MAX_LINES)
        {
            consoleOutputText.text = string.Join("\n", lines.Skip(lines.Length - MAX_LINES).ToArray());
        }

        if (scrollRect != null && atBottom)
        {
            Canvas.ForceUpdateCanvases(); // Ensures layout is updated before setting scroll position
            scrollRect.verticalNormalizedPosition = 0f; // Scroll to the very bottom
        }
    }

    private void HandleConsoleMessage(string message)
    {
        if (message == "<CMD_CLEAR_CONSOLE>")
        {
            consoleOutputText.text = "";
            AppendToConsole(
                _initalOutputText +
                "<color=yellow>Console cleared.</color>"
                );
        }
        else
        {
            AppendToConsole(message);
        }
    }

    private void CycleHistory(bool backward)
    {
        if (_history.Count == 0) return;

        if (backward)
        {
            _historyIndex--;
            if (_historyIndex < 0)
            {
                _historyIndex = 0;
            }
        }
        else // forward
        {
            _historyIndex++;
            if (_historyIndex >= _history.Count)
            {
                // If moving past the end of history, treat it as an empty input
                // This allows the user to clear the field after cycling through all history
                if (_historyIndex == _history.Count)
                {
                    commandInputField.text = "";
                    ClearAutocompleteSuggestions(); // Clear suggestions too
                    return;
                }
                _historyIndex = _history.Count - 1; // Cap at the last item
            }
        }

        if (_historyIndex >= 0 && _historyIndex < _history.Count)
        {
            commandInputField.text = _history[_historyIndex];
            commandInputField.MoveTextEnd(true); // Place cursor at end of text for TMP_InputField
        }
        ClearAutocompleteSuggestions(); // Clear suggestions when cycling history
    }

    /// <summary>
    /// Populates and displays autocomplete suggestions based on the current input.
    /// This version only handles command word autocompletion.
    /// </summary>
    /// <param name="currentInput">The current text in the input field.</param>
    private void PopulateSuggestions(string currentInput)
    {
        if (autocompleteSuggestionsText == null || ConsoleManager.Instance == null)
        {
            return;
        }

        _currentSuggestions.Clear();
        _suggestionIndex = -1; // Reset highlight index

        if (string.IsNullOrWhiteSpace(currentInput))
        {
            ClearAutocompleteSuggestions();
            return;
        }

        string lowerInput = currentInput.ToLower();

        // Autocompleting only the command word (the first part of the input)
        string commandWordPartial = lowerInput.Split(' ')[0];

        _currentSuggestions = ConsoleManager.Instance.GetCommands().Keys
            .Where(cmd => cmd.StartsWith(commandWordPartial))
            .OrderBy(cmd => cmd)
            .ToList();

        DisplaySuggestions();
    }

    /// <summary>
    /// Cycles through the available autocomplete suggestions when the Tab key is pressed.
    /// This version replaces the entire input field with the selected command word.
    /// </summary>
    private void CycleSuggestions()
    {
        if (_currentSuggestions.Count == 0)
        {
            return;
        }

        _suggestionIndex = (_suggestionIndex + 1) % _currentSuggestions.Count;
        string selectedSuggestion = _currentSuggestions[_suggestionIndex];

        // Replace the entire input field text with the selected suggestion
        commandInputField.text = selectedSuggestion;
        commandInputField.MoveTextEnd(false); // Move cursor to end, without selecting text

        DisplaySuggestions(); // Update display to show highlight
    }

    /// <summary>
    /// Updates the autocomplete suggestions text element with the current suggestions.
    /// Highlights the currently selected suggestion if any.
    /// </summary>
    private void DisplaySuggestions()
    {
        if (autocompleteSuggestionsText == null) return;

        if (_currentSuggestions.Count == 0)
        {
            ClearAutocompleteSuggestions();
            return;
        }

        // Format suggestions for display, highlighting the selected one
        string suggestionsString = "";
        for (int i = 0; i < _currentSuggestions.Count; i++)
        {
            if (i > 0)
            {
                suggestionsString += "\n";
            }
            if (i == _suggestionIndex)
            {
                // Highlight the selected suggestion
                suggestionsString += $"<color=lime><b>{_currentSuggestions[i]}</b></color>";
            }
            else
            {
                suggestionsString += _currentSuggestions[i];
            }
        }
        autocompleteSuggestionsText.text = suggestionsString;
    }

    /// <summary>
    /// Clears the autocomplete suggestions display and internal list.
    /// </summary>
    private void ClearAutocompleteSuggestions()
    {
        if (autocompleteSuggestionsText != null)
        {
            autocompleteSuggestionsText.text = "";
        }
        _currentSuggestions.Clear();
        _suggestionIndex = -1;
    }
}
