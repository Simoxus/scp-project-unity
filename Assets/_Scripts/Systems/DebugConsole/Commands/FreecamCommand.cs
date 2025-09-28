using UnityEngine;

namespace Console.Commands
{
    public class FreecamCommand : ConsoleBase
    {
        public override string CommandWord => "freecam";
        public override string Description => "Detaches the player's camera from their character.";
        protected override string RawUsage => "freecam";

        public override void Execute(string[] args)
        {
            GameObject player = GameObject.FindWithTag("Player");
            PlayerFreecam playerFreecam = player.GetComponent<PlayerFreecam>();

            bool freecamIsEnabled = playerFreecam.ToggleFreecam();
            if (freecamIsEnabled) { ConsoleManager.LogToConsole("<color=#33CC33>Freecam has been activated.</color>"); }
            if (!freecamIsEnabled) { ConsoleManager.LogToConsole("<color=#33CC33>Freecam has been deactivated.</color>"); }
        }
    }
}