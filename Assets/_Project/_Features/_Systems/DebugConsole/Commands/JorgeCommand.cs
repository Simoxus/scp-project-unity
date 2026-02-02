namespace Console.Commands
{
    public class JorgeCommand : BaseConsole
    {
        public override string CommandWord => "jorge";
        public override string Description => "jorgejorgejorgejorgejorgejorge";
        protected override string RawUsage => "jorge";

        public override void Execute(string[] args)
        {
            FMODHelper.PlayOneShot(Core.AudioDataAccess.Special.FcvenySound);
            ConsoleManager.LogToConsole("JORGE HAS BEEN EXPECTING YOU".AsJorge());
        }
    }
}