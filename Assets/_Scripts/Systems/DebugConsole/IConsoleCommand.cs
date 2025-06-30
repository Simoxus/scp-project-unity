using System.Collections.Generic;

public interface IConsoleCommand
{
    string CommandWord { get; }
    string Description { get; }
    void Execute(string[] args);
}
