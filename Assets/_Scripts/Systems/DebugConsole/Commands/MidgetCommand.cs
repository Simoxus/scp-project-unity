using UnityEngine;

public class MidgetCommand : ConsoleBase
{
    public override string CommandWord => "midget";
    public override string Description => "Transforms the player into/out of being a midget.";

    public override void Execute(string[] args)
    {
        GameObject player = GameObject.FindWithTag("Player");
        PlayerController playerController = player.GetComponent<PlayerController>();

        bool playerIsMidget = playerController.ToggleMidget(); // Returns a bool :)
        if (playerIsMidget) { ConsoleManager.LogToConsole("Player has been midgetified :D"); }
        if (!playerIsMidget) { ConsoleManager.LogToConsole("Player has been unmidgetified D:"); }
    }
}
