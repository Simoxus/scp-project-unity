using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for Enumerable.Empty
using UnityEngine;
using Cysharp.Threading.Tasks;

public class RuntimeInfoCommand : ConsoleBase
{
    public override string CommandWord => "runtimeinfo";
    public override string Description => "Fetches information about the client's hardware, game, etc.";

    public override void Execute(string[] args)
    {
        if (args.Length > 0)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: runtimeinfo (no arguments)</color>");
            return;
        }

        ConsoleManager.LogToConsole($"<b>Game</b>: {Application.productName}");
        ConsoleManager.LogToConsole($"<b>Version</b>: {Application.version}");
        ConsoleManager.LogToConsole("----------------------------------------");
        ConsoleManager.LogToConsole($"<b>Framerate~</b>: {(int)(1.0f / Time.smoothDeltaTime)}");
        ConsoleManager.LogToConsole($"<b>Deltatime</b>: {Time.deltaTime}");
        ConsoleManager.LogToConsole($"<b>Timescale</b>: {Time.timeScale}");
        ConsoleManager.LogToConsole("----------------------------------------");
        ConsoleManager.LogToConsole($"<b>Device Type</b>: {SystemInfo.deviceType}");
        ConsoleManager.LogToConsole($"<b>Device OS</b>: {SystemInfo.operatingSystem}");
        ConsoleManager.LogToConsole($"<b>Device GPU</b>: {SystemInfo.graphicsDeviceName} (VRAM: {SystemInfo.graphicsMemorySize}MB)");
        ConsoleManager.LogToConsole($"<b>Device CPU</b>: {SystemInfo.processorType} (Cores: {SystemInfo.processorCount})");
        ConsoleManager.LogToConsole($"<b>Device Supports Audio?</b>: {SystemInfo.supportsAudio}"); // Corrected logic: Use SystemInfo.supportsAudio directly
    }
}
