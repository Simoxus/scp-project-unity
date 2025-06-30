using System.Collections.Generic;
using System.Linq;

public class HelpCommand : ConsoleBase
{
    public override string CommandWord => "help";
    public override string Description => "Lists all available commands.";

    public override void Execute(string[] args)
    {
        if (ConsoleManager.Instance == null)
        {
            ConsoleManager.LogToConsole("<color=red>ConsoleManager not initialized. Cannot display 'help'.</color>");
            return;
        }

        ConsoleManager.LogToConsole("<color=lightblue>--- Available Commands ---</color>");
        foreach (var commandEntry in ConsoleManager.Instance.GetCommands().OrderBy(c => c.Key))
        {
            ConsoleManager.LogToConsole($"<b>{commandEntry.Value.CommandWord}</b>: {commandEntry.Value.Description}");
        }
        ConsoleManager.LogToConsole("<color=lightblue>--------------------------</color>");
    }
}
