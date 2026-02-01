using UnityEngine;

namespace Console.Commands
{
    public class PhysicsCommand : BaseConsole
    {
        public override string CommandWord => "physics";
        public override string Description => "Displays core physics settings.";
        protected override string RawUsage => "physics";

        public override void Execute(string[] args)
        {
            ConsoleManager.LogToConsole($"Fixed Timestep: {Time.fixedDeltaTime}".AsInfo());
            ConsoleManager.LogToConsole($"Gravity: {Physics.gravity}".AsInfo());
            ConsoleManager.LogToConsole($"Queries Hit Triggers: {Physics.queriesHitTriggers}".AsInfo());
            ConsoleManager.LogToConsole($"Solver Iterations: {Physics.defaultSolverIterations}".AsInfo());
        }
    }
}
