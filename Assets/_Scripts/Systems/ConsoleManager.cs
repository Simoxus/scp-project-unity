using Console.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ConsoleManager : Singleton<ConsoleManager>
{
    private Dictionary<string, IConsoleCommand> commands = new();
    private HashSet<string> aliasKeys = new();

    public static event Action<string> OnConsoleMessage;

    protected override void OnSingletonAwake()
    {
        InitializeCommands();
    }

    private void InitializeCommands()
    {
        // Scan for all commands in the commands namespace
        var commandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IConsoleCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Where(t => t.Namespace == Core.COMMAND_NAMESPACE); // <--- restricted to the command namespace

        foreach (var type in commandTypes)
        {
            try
            {
                IConsoleCommand command = (IConsoleCommand)Activator.CreateInstance(type);
                RegisterCommand(command);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to register console command {type.Name}: {ex}");
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
            Log.VerboseWarning($"Console command '{key}' has already been registered. Overwriting.");
            commands[key] = command;
        }
        else
        {
            commands.Add(key, command);
            Log.VerboseInfo($"Registered console command '{key}'.");
        }

        if (command.Aliases != null && command.Aliases.Length > 0)
        {
            foreach (string alias in command.Aliases)
            {
                string aliasKey = alias.ToLower();
                if (commands.ContainsKey(aliasKey))
                {
                    continue;
                }

                commands.Add(aliasKey, command);
                aliasKeys.Add(aliasKey);
            }
        }
    }

    public void UnregisterCommand(string commandWord)
    {
        string key = commandWord.ToLower();

        if (commands.TryGetValue(key, out IConsoleCommand command))
        {
            commands.Remove(key);

            if (command.Aliases != null)
            {
                foreach (string alias in command.Aliases)
                {
                    string aliasKey = alias.ToLower();
                    commands.Remove(aliasKey);
                    aliasKeys.Remove(aliasKey);
                }
            }

            Log.VerboseInfo($"Unregistered console command '{commandWord}'.", this);
        }
    }

    public void ProcessCommand(string input)
    {
        input = input.Trim();
        if (string.IsNullOrEmpty(input)) return;

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
                LogToConsole($"Error executing '{commandWord}': {ex.Message}".AsError());
            }
        }
        else
        {
            LogToConsole($"Unknown command: '{commandWord}'. Type 'help' for a list of commands.".AsWarning());
        }
    }

    public static void LogToConsole(string message) => OnConsoleMessage?.Invoke(message);

    public IReadOnlyDictionary<string, IConsoleCommand> GetCommands() => commands;

    // Returns all commands including aliases for autocomplete
    public IReadOnlyDictionary<string, IConsoleCommand> GetCommandsForAutocomplete() => commands;

    public bool IsAlias(string commandWord) => aliasKeys.Contains(commandWord.ToLower());
}