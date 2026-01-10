using Cysharp.Threading.Tasks;

namespace Console.Commands
{
    public class MavCommand : BaseConsole
    {
        public override string CommandWord => "mav";
        public override string Description => "Error! Memory access violation (just kidding haha)";
        protected override string RawUsage => "mav";

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            ConsoleManager.LogToConsole("Memory access violation (JUST KIDDING GET PRANKED LOSER)".AsError());
            SpookyMessage().Forget();
        }

        private async UniTask SpookyMessage()
        {
            await UniTask.WaitForSeconds(2f, ignoreTimeScale: true);
            ConsoleManager.LogToConsole("Seriously though.. <i>I will</i> generate a memory access violation.".AsJorge());
        }
    }
}
