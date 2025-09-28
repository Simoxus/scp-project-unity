using System.Collections.Generic;

public interface IConsoleCommand
{
    string CommandWord { get; }
    string Description { get; }
    string Usage { get; }
    void Execute(string[] args);
}
