using UnityEngine;

public class QuitCommand : ConsoleBase
{
    public override string CommandWord => "quit";
    public override string Description => "Quits the game.";

    public override void Execute(string[] args)
    {
        ConsoleManager.LogToConsole($"Quitting {Application.productName}....");

        if (Application.isEditor)
        {
            ConsoleManager.LogToConsole($"<color=red>Attempt to quit {Application.productName} failed because you are in the editor!</color>");
        } 
        else
        {
            Application.Quit();
        }
    }
}
