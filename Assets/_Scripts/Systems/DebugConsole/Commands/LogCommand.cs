using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for .Where, .OrderBy, .Skip, .ToArray
using UnityEngine; // Required for Debug.Log (indirectly via ConsoleManager)

public class LogCommand : ConsoleBase
{
    public override string CommandWord => "log";
    public override string Description => "Logs a custom message to the console with a specified type. Usage: log <type (error|warning|info)> <message>";

    public override void Execute(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: log <type (error|warning|info)> <message></color>");
            return;
        }

        string logType = args[0].ToLower();
        string message = string.Join(" ", args.Skip(1).ToArray());

        switch (logType)
        {
            case "error":
                Debug.LogError($"[Forced Error]: {message}");
                break;
            case "warning":
                Debug.LogWarning($"[Forced Warning]: {message}");
                break;
            case "info":
            case "log":
                Debug.Log($"[Forced Log]: {message}");
                break;
            default:
                ConsoleManager.LogToConsole("<color=red>Invalid log type. Use 'error', 'warning', or 'info'.</color>");
                break;
        }
    }
}
