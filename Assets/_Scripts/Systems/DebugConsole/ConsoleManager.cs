using Console.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    public static ConsoleManager Instance { get; private set; }
    private Dictionary<string, IConsoleCommand> commands = new();

    public static event Action<string> OnConsoleMessage;

    private void Awake()
    {
        // Implement Singleton pattern.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCommands();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCommands()
    {
        // Scan for all commands in the commands namespace
        var commandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IConsoleCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Where(t => t.Namespace == "Console.Commands"); // <--- restricted to the command namespace

        foreach (var type in commandTypes)
        {
            try
            {
                IConsoleCommand command = (IConsoleCommand)Activator.CreateInstance(type);
                RegisterCommand(command);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ConsoleManager: Failed to register command {type.Name}: {ex}");
            }
        }

        if (commands.TryGetValue("scene", out var sceneCmd) && sceneCmd is SceneCommand sc)
        {
            SceneCommand.PopulateAvailableScenes();
        }
    }

    public void RegisterCommand(IConsoleCommand command)
    {
        string key = command.CommandWord.ToLower();

        if (commands.ContainsKey(key))
        {
            Debug.LogWarning($"ConsoleManager: Command '{key}' has already been registered. Overwriting.");
            commands[key] = command;
        }
        else
        {
            commands.Add(key, command);
            Debug.Log($"ConsoleManager: Registered command '{key}'.");
        }
    }

    public void UnregisterCommand(string commandWord)
    {
        if (commands.Remove(commandWord.ToLower()))
        {
            Debug.Log($"ConsoleManager: Unregistered command '{commandWord}'.");
        }
    }

    public void ProcessCommand(string input)
    {
        input = input.Trim();
        if (string.IsNullOrEmpty(input)) { return; }

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
                LogToConsole($"<color=#FF0000FF>Error executing '{commandWord}': {ex.Message}</color>");
            }
        }
        else
        {
            LogToConsole($"<color=#FFA500FF>Unknown command: '{commandWord}'. Type 'help' for a list of commands.</color>");
        }
    }

    public static void LogToConsole(string message) => OnConsoleMessage?.Invoke(message); // Helper method to log messages to the console UI.
    public IReadOnlyDictionary<string, IConsoleCommand> GetCommands() => commands; // Provides access to all registered commands for the HelpCommand (and others).
}
