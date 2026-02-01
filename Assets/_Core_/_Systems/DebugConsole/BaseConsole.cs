public abstract class BaseConsole : IConsoleCommand
{
    public abstract string CommandWord { get; }
    public abstract string Description { get; }
    public virtual string[] Aliases => new string[0];
    protected virtual string RawUsage => CommandWord;
    public string Usage => $"Usage: {RawUsage}";

    public abstract void Execute(string[] args);
}