using UnityEngine;

namespace Console.Commands
{
    public class QuitCommand : ConsoleBase
    {
        public override string CommandWord => "quit";
        public override string Description => "Quits the game.";
        protected override string RawUsage => "quit";

        public override void Execute(string[] args)
        {
            ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Quitting {Application.productName}.... :(");

            if (Application.isEditor)
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>Attempt to quit {Application.productName} failed because you are in the editor!</color>");
            }
            else
            {
                Application.Quit();
            }
        }
    }
}