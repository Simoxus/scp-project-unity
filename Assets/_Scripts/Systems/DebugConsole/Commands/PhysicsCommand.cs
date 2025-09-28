using UnityEngine;

namespace Console.Commands
{
    public class PhysicsCommand : ConsoleBase
    {
        public override string CommandWord => "physics";
        public override string Description => "Displays core physics settings.";
        protected override string RawUsage => "uptime";

        public override void Execute(string[] args)
        {
            ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Fixed Timestep: {Time.fixedDeltaTime}</color>");
            ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Gravity: {Physics.gravity}</color>");
            ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Queries Hit Triggers: {Physics.queriesHitTriggers}</color>");
            ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Solver Iterations: {Physics.defaultSolverIterations}</color>");
        }
    }
}
