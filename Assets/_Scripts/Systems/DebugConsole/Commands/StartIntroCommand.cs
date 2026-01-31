using UnityEngine;

namespace Console.Commands
{
    public class StartIntroCommand : BaseConsole
    {
        public override string CommandWord => "startintro";
        public override string Description => "Starts the intro sequence.";
        protected override string RawUsage => "startintro";

        public override void Execute(string[] args)
        {
            Object.FindFirstObjectByType<IntroSequence>().StartIntro();
        }
    }
}