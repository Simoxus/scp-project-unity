using System.Collections.Generic;
using System.Linq;

/* 
 * HEADERS: #00FFFFFF
 * INFO/LOGS: #ADD8E6FF
 * WARNINGS: #FFA500FF
 * ERRORS: #FF0000FF
 * SUCCESS: #33CC33
 * INPUT: #FFFFFF
*/

public abstract class ConsoleBase : IConsoleCommand
{
    public abstract string CommandWord { get; }
    public abstract string Description { get; }
    protected virtual string RawUsage => CommandWord;
    public string Usage => $"Usage: {RawUsage}";

    public abstract void Execute(string[] args);
}
