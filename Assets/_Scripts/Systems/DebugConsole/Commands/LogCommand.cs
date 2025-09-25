using System.Linq;
using UnityEngine;

public class LogCommand : ConsoleBase
{
    public override string CommandWord => "log";
    public override string Description => "Logs a custom message to the console with a specified type. Usage: log <type (error|warning|log|info)> <message>";

    public override void Execute(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Usage: log <type (error|warning|log|info)> <message></color>");
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
            case "log":
                Debug.Log($"[Forced Log]: {message}");
                break;
            case "info":
                Debug.Log($"[Forced Info]: {message}");
                break;
            default:
                ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid log type. Use 'error', 'warning', 'log', or 'info'.</color>");
                break;
        }
    }
}
