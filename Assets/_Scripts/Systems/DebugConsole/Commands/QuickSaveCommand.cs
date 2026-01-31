using Cysharp.Threading.Tasks;

namespace Console.Commands
{
    public class QuickSaveCommand : BaseConsole
    {
        public override string CommandWord => "quicksave";
        public override string Description => "Saves the game as a quick save.";
        public override string[] Aliases => new string[] { "save", "qsave" };
        protected override string RawUsage => "quicksave";

        public override void Execute(string[] args)
        {
            if (Core.PersistenceManager != null)
            {
                Core.PersistenceManager.QuickSave().Forget();
                ConsoleManager.LogToConsole($"Game quicksaved, save is located in {Core.ProgressManager.SaveFolderName}.".AsSuccess());
            }
            else
            {
                ConsoleManager.LogToConsole("PersistenceManager isn't available.".AsError());
            }
        }
    }
}