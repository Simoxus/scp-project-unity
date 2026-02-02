#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Console.Commands
{
    public class OverdrawCommand : BaseConsole
    {
        public override string CommandWord => "overdraw";
        public override string Description => "Toggles overdraw visualization." + " (editor-only)".AsEditor();
        protected override string RawUsage => "overdraw";

        private static bool isOverdraw = false;

        static OverdrawCommand()
        {
            Application.quitting += ResetOnQuit;
        }

        private static void ResetOnQuit()
        {
            UniversalRenderPipelineDebugDisplaySettings debugDisplaySettings = UniversalRenderPipelineDebugDisplaySettings.Instance;
            debugDisplaySettings.renderingSettings.overdrawMode = DebugOverdrawMode.None;
        }

        public override void Execute(string[] args)
        {
            UniversalRenderPipelineDebugDisplaySettings debugDisplaySettings = UniversalRenderPipelineDebugDisplaySettings.Instance;

            isOverdraw = !isOverdraw;

            if (isOverdraw)
            {
                debugDisplaySettings.renderingSettings.overdrawMode = DebugOverdrawMode.All;
                ConsoleManager.LogToConsole("Overdraw has been enabled.".AsSuccess());
            }
            else
            {
                debugDisplaySettings.renderingSettings.overdrawMode = DebugOverdrawMode.None;
                ConsoleManager.LogToConsole("Overdraw has been disabled.".AsSuccess());
            }
        }
    }
}
#endif