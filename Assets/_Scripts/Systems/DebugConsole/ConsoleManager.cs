using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    // Singleton instance.
    public static ConsoleManager Instance { get; private set; }

    // Dictionary to store all registered commands, mapping their command word to the command object.
    private Dictionary<string, IConsoleCommand> commands = new Dictionary<string, IConsoleCommand>();

    // Event for when a message should be logged to the console UI.
    public static event Action<string> OnConsoleMessage;

    private void Awake()
    {
        // Implement Singleton pattern.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the manager alive across scenes.
            InitializeCommands();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances.
        }
    }

    // Initialize and register all core commands.
    private void InitializeCommands()
    {
        // Register your default and core commands here.
        RegisterCommand(new HelpCommand());
        RegisterCommand(new ClearCommand());
        RegisterCommand(new HealthCommand());
        RegisterCommand(new SanityCommand());
        RegisterCommand(new KillCommand());
        RegisterCommand(new MavCommand());
        RegisterCommand(new RuntimeInfoCommand());
        RegisterCommand(new EffectCommand());
        RegisterCommand(new TimeCommand());
        RegisterCommand(new LogCommand());
        RegisterCommand(new MidgetCommand());
        RegisterCommand(new SceneCommand());

        SceneCommand.PopulateAvailableScenes();
    }

    // Registers a new command with the manager.
    public void RegisterCommand(IConsoleCommand command)
    {
        if (commands.ContainsKey(command.CommandWord.ToLower()))
        {
            Debug.LogWarning($"ConsoleManager: Command '{command.CommandWord}' already registered. Overwriting.");
            commands[command.CommandWord.ToLower()] = command;
        }
        else
        {
            commands.Add(command.CommandWord.ToLower(), command);
            Debug.Log($"ConsoleManager: Registered command '{command.CommandWord}'.");
        }
    }

    // Unregisters a command.
    public void UnregisterCommand(string commandWord)
    {
        if (commands.Remove(commandWord.ToLower()))
        {
            Debug.Log($"ConsoleManager: Unregistered command '{commandWord}'.");
        }
        else
        {
            Debug.LogWarning($"ConsoleManager: Command '{commandWord}' not found for unregistration.");
        }
    }

    // Processes the input string from the console.
    public void ProcessCommand(string input)
    {
        input = input.Trim();
        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        // Split the input into command word and arguments.
        // Example: "spawn enemy 10" -> commandWord="spawn", args=["enemy", "10"]
        string[] parts = input.Split(' ');
        string commandWord = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        if (commands.TryGetValue(commandWord, out IConsoleCommand command))
        {
            try
            {
                command.Execute(args);
            }
            catch (Exception ex)
            {
                LogToConsole($"<color=red>Error executing '{commandWord}': {ex.Message}</color>");
                Debug.LogError($"Error executing command '{commandWord}': {ex}");
            }
        }
        else
        {
            LogToConsole($"<color=orange>Unknown command: '{commandWord}'. Type 'help' for a list of commands.</color>");
            Debug.LogWarning($"Unknown command: '{commandWord}'");
        }
    }

    // Helper method to log messages to the console UI.
    public static void LogToConsole(string message)
    {
        OnConsoleMessage?.Invoke(message);
    }

    // Provides access to all registered commands for the HelpCommand.
    public IReadOnlyDictionary<string, IConsoleCommand> GetCommands()
    {
        return commands;
    }
}
