using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for Enumerable.Empty
using UnityEngine;
using Cysharp.Threading.Tasks; // Ensure you have UniTask imported if using this.

public class MavCommand : ConsoleBase
{
    public override string CommandWord => "mav";
    public override string Description => "Error! Memory access violation (just kidding haha)";

    public override void Execute(string[] args)
    {
        if (args.Length > 0)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: mav (no arguments)</color>");
            return;
        }

        ConsoleManager.LogToConsole($"Memory access violation (JUST KIDDING GET PRANKED LOSER)");
        SpookyMessage().Forget(); // Discard UniTask
    }

    private async UniTask SpookyMessage()
    {
        await UniTask.WaitForSeconds(3f, ignoreTimeScale: true);
        ConsoleManager.LogToConsole($"Seriously though.. <i>I will</i> generate a memory access violation.");
    }
}
