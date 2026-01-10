using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System.Collections.Generic;

[MoonSharpUserData]
public class ConsoleAPI
{
    private Dictionary<string, LuaConsoleCommand> _luaCommands = new Dictionary<string, LuaConsoleCommand>();

    [MoonSharpVisible(true)]
    public void RegisterCommand(string commandWord, string description, string usage, Closure executeCallback)
    {
        string key = commandWord.ToLower();

        // Unregister existing Lua command if it exists
        if (_luaCommands.ContainsKey(key))
        {
            UnregisterCommand(commandWord);
        }

        LuaConsoleCommand luaCommand = new LuaConsoleCommand(
            commandWord,
            description,
            usage,
            executeCallback
        );

        ConsoleManager.Instance.RegisterCommand(luaCommand);
        _luaCommands[key] = luaCommand;
    }

    [MoonSharpVisible(true)]
    public void UnregisterCommand(string commandWord)
    {
        string key = commandWord.ToLower();

        if (_luaCommands.ContainsKey(key))
        {
            ConsoleManager.Instance.UnregisterCommand(commandWord);
            _luaCommands.Remove(key);
        }
    }

    [MoonSharpVisible(true)]
    public void LogToConsole(string message)
    {
        ConsoleManager.LogToConsole(message);
    }

    [MoonSharpVisible(true)]
    public void LogInfo(string message)
    {
        ConsoleManager.LogToConsole(message.AsInfo());
    }

    [MoonSharpVisible(true)]
    public void LogWarning(string message)
    {
        ConsoleManager.LogToConsole(message.AsWarning());
    }

    [MoonSharpVisible(true)]
    public void LogError(string message)
    {
        ConsoleManager.LogToConsole(message.AsError());
    }

    [MoonSharpVisible(true)]
    public void LogSuccess(string message)
    {
        ConsoleManager.LogToConsole(message.AsSuccess());
    }

    [MoonSharpVisible(true)]
    public Table GetAllCommands()
    {
        if (ConsoleManager.Instance == null)
        {
            return new Table(new Script());
        }

        Script script = new Script();
        Table result = new Table(script);

        var commands = ConsoleManager.Instance.GetCommands();
        int index = 1;

        foreach (var cmd in commands)
        {
            Table commandInfo = new Table(script);
            commandInfo["word"] = cmd.Key;
            commandInfo["description"] = cmd.Value.Description;
            commandInfo["usage"] = cmd.Value.Usage;
            result[index++] = commandInfo;
        }

        return result;
    }

    public void CleanupCommands()
    {
        foreach (var commandKey in _luaCommands.Keys)
        {
            ConsoleManager.Instance?.UnregisterCommand(commandKey);
        }
        _luaCommands.Clear();
    }
}

public class LuaConsoleCommand : IConsoleCommand
{
    public string CommandWord { get; }
    public string Description { get; }
    public string Usage { get; }

    private Closure _executeCallback;

    public LuaConsoleCommand(string commandWord, string description, string usage, Closure executeCallback)
    {
        CommandWord = commandWord;
        Description = description;
        Usage = usage;
        _executeCallback = executeCallback;
    }

    public void Execute(string[] args)
    {
        if (_executeCallback == null)
        {
            ConsoleManager.LogToConsole($"Command '{CommandWord}' has no execute callback!".AsError());
            return;
        }

        try
        {
            Script script = _executeCallback.OwnerScript;
            Table argsTable = new Table(script);
            for (int i = 0; i < args.Length; i++)
            {
                argsTable[i + 1] = args[i];
            }

            _executeCallback.Call(argsTable);
        }
        catch (ScriptRuntimeException ex)
        {
            ConsoleManager.LogToConsole($"Mod command '{CommandWord}' failed: {ex.DecoratedMessage}".AsError());
            Log.Error($"Mod command error: {ex.DecoratedMessage}");
        }
    }
}