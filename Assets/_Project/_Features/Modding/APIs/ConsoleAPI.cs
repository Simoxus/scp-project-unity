using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System.Collections.Generic;

[ModAPI("Console", perMod: true)]
[MoonSharpUserData]
public class ConsoleAPI : IModAPICleanup
{
    private Dictionary<string, LuaConsoleCommand> _luaCommands = new Dictionary<string, LuaConsoleCommand>();

    public void OnModUnloaded(string modId) => CleanupCommands();

    private readonly string _modId;
    public ConsoleAPI(string modId) => _modId = modId;

    [MoonSharpVisible(true)]
    [LuaDoc("Registers a new console command. If the command word already exists, it will be replaced. :(")]
    [LuaParam("commandWord", "The primary word used to invoke the command")]
    [LuaParam("description", "Short description shown in the help listing")]
    [LuaParam("aliases", "Table of alternative command words. Pass nil for none")]
    [LuaParam("usage", "Usage string shown using the help command")]
    [LuaParam("executeCallback", "Function called when the command runs. Receives a table of string arguments")]
    public void RegisterCommand(string commandWord, string description, Table aliases, string usage, Closure executeCallback)
    {
        string key = commandWord.ToLower();
        if (_luaCommands.ContainsKey(key))
        {
            UnregisterCommand(commandWord);
        }

        // Lua table to string array
        string[] aliasArray = null;
        if (aliases != null)
        {
            var aliasList = new System.Collections.Generic.List<string>();
            foreach (var pair in aliases.Pairs)
            {
                if (pair.Value.Type == DataType.String)
                {
                    aliasList.Add(pair.Value.String);
                }
            }
            aliasArray = aliasList.ToArray();
        }

        LuaConsoleCommand luaCommand = new LuaConsoleCommand(
            commandWord,
            description,
            aliasArray,
            usage,
            executeCallback
        );
        Core.ConsoleManager.RegisterCommand(luaCommand);

        _luaCommands[key] = luaCommand;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Unregisters a previously registered console command.")]
    [LuaParam("commandWord", "The command word used when registering")]
    public void UnregisterCommand(string commandWord)
    {
        string key = commandWord.ToLower();

        if (_luaCommands.ContainsKey(key))
        {
            Core.ConsoleManager.UnregisterCommand(commandWord);
            _luaCommands.Remove(key);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Prints a plain message to the in-game debug console.")]
    [LuaParam("message", "Message to print")]
    public void LogToConsole(string message)
    {
        ConsoleManager.LogToConsole(message);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Prints an info-styled message to the in-game console. If you'd like to print to the Unity console, use the global 'info'.")]
    [LuaParam("message", "Message to print")]
    public void LogInfo(string message)
    {
        ConsoleManager.LogToConsole(message.AsInfo());
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Prints a warning-styled message to the in-game console. If you'd like to print to the Unity console, use the global 'warn'.")]
    [LuaParam("message", "Message to print")]
    public void LogWarning(string message)
    {
        ConsoleManager.LogToConsole(message.AsWarning());
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Prints a error-styled message to the in-game console. If you'd like to print to the Unity console, use the global 'error'.")]
    [LuaParam("message", "Message to print")]
    public void LogError(string message)
    {
        ConsoleManager.LogToConsole(message.AsError());
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Prints a success-styled message to the in-game console. If you'd like to print to the Unity console, use the global 'success'.")]
    [LuaParam("message", "Message to print")]
    public void LogSuccess(string message)
    {
        ConsoleManager.LogToConsole(message.AsSuccess());
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns a table of all currently registered console commands. Each entry has 'word', 'description', and 'usage' fields.")]
    public Table GetAllCommands()
    {
        if (Core.ConsoleManager == null)
        {
            return new Table(new Script());
        }

        Script script = new Script();
        Table result = new Table(script);

        var commands = Core.ConsoleManager.GetCommands();
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
            Core.ConsoleManager?.UnregisterCommand(commandKey);
        }
        _luaCommands.Clear();
    }
}

public class LuaConsoleCommand : IConsoleCommand
{
    public string CommandWord { get; }
    public string Description { get; }
    public string[] Aliases { get; }
    public string Usage { get; }
    private Closure _executeCallback;

    public LuaConsoleCommand(string commandWord, string description, string[] aliases, string usage, Closure executeCallback)
    {
        CommandWord = commandWord;
        Description = description;
        Aliases = aliases ?? new string[0];
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
            Log.Exception(ex, message: ex.DecoratedMessage, header: CommandWord);
        }
    }
}