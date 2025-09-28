using UnityEngine;

namespace Console.Commands
{
    public class MidgetCommand : ConsoleBase
    {
        public override string CommandWord => "midget";
        public override string Description => "Transforms the player into/out of being a midget.";
        protected override string RawUsage => "midget";

        public override void Execute(string[] args)
        {
            GameObject player = GameObject.FindWithTag("Player");
            PlayerController playerController = player.GetComponent<PlayerController>();

            bool playerIsMidget = playerController.ToggleMidget();
            if (playerIsMidget) { ConsoleManager.LogToConsole("<color=#33CC33>Player has been midgetified :D</color>"); }
            if (!playerIsMidget) { ConsoleManager.LogToConsole("<color=#33CC33>Player has been unmidgetified D:</color>"); }
        }
    }
}
