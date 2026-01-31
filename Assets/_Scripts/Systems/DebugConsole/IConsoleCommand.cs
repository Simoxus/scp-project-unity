public interface IConsoleCommand
{
    string CommandWord { get; }
    string Description { get; }
    string[] Aliases { get; }
    string Usage { get; }

    void Execute(string[] args);
}