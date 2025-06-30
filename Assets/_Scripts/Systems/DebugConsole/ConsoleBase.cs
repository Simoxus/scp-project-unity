using System.Collections.Generic;
using System.Linq;

public abstract class ConsoleBase : IConsoleCommand
{
    public abstract string CommandWord { get; }
    public abstract string Description { get; }
    public abstract void Execute(string[] args);
}
