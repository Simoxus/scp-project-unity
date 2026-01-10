using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Console.Commands
{
    public class NoLightsCommand : BaseConsole
    {
        public override string CommandWord => "nolights";
        public override string Description => "Disables all lighting.";
        protected override string RawUsage => "nolights";

        private static bool isEnabled = false;

        static NoLightsCommand()
        {
            Application.quitting += ResetOnQuit;
        }

        private static void ResetOnQuit()
        {
            UniversalRenderPipelineDebugDisplaySettings debugDisplaySettings = UniversalRenderPipelineDebugDisplaySettings.Instance;
            debugDisplaySettings.lightingSettings.lightingDebugMode = DebugLightingMode.None;
        }

        public override void Execute(string[] args)
        {
            UniversalRenderPipelineDebugDisplaySettings debugDisplaySettings = UniversalRenderPipelineDebugDisplaySettings.Instance;

            isEnabled = !isEnabled;

            debugDisplaySettings.lightingSettings.lightingDebugMode = isEnabled
                ? DebugLightingMode.Reflections
                : DebugLightingMode.None;

            ConsoleManager.LogToConsole($"Lighting has been {(isEnabled ? "disabled" : "enabled")}.".AsSuccess());
        }
    }
}