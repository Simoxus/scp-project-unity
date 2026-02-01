using UnityEngine;

namespace Console.Commands
{
    public class QuitCommand : BaseConsole
    {
        public override string CommandWord => "quit";
        public override string Description => "Quits the game.";
        protected override string RawUsage => "quit";

        public override void Execute(string[] args)
        {
            ConsoleManager.LogToConsole($"Quitting {Application.productName}.... :(".AsSuccess());

            if (Application.isEditor)
            {
                ConsoleManager.LogToConsole($"Quitting {Application.productName} failed because you are in the editor!".AsError());
            }
            else
            {
                Application.Quit();
            }
        }
    }
}