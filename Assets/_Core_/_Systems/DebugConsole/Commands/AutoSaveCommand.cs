using Cysharp.Threading.Tasks;

namespace Console.Commands
{
    public class AutoSaveCommand : BaseConsole
    {
        public override string CommandWord => "autosave";
        public override string Description => "Saves the game as a manual autosave.";
        public override string[] Aliases => new string[] { "asave" };
        protected override string RawUsage => "autosave";

        public override void Execute(string[] args)
        {
            if (Core.PersistenceManager != null)
            {
                Core.PersistenceManager.ManualAutosave().Forget();
                ConsoleManager.LogToConsole($"Manually autosaved game, save is located in {Core.ProgressManager.SaveFolderName}.".AsSuccess());
            }
            else
            {
                ConsoleManager.LogToConsole("PersistenceManager isn't available.".AsError());
            }
        }
    }
}