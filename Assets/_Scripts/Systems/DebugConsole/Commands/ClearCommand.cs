using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for Enumerable.Empty

public class ClearCommand : ConsoleBase
{
    public override string CommandWord => "clear";
    public override string Description => "Clears the console output.";

    public override void Execute(string[] args)
    {
        ConsoleManager.LogToConsole("<CMD_CLEAR_CONSOLE>");
    }
}
