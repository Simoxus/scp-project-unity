using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Console.Commands
{
    public class WireframeCommand : BaseConsole
    {
        public override string CommandWord => "wireframe";
        public override string Description => "Toggles wireframe rendering mode.";
        protected override string RawUsage => "wireframe";

        private static bool isWireframe = false;

        static WireframeCommand()
        {
            Application.quitting += ResetOnQuit;
        }

        private static void ResetOnQuit()
        {
            UniversalRenderPipelineDebugDisplaySettings debugDisplaySettings = UniversalRenderPipelineDebugDisplaySettings.Instance;
            debugDisplaySettings.renderingSettings.wireframeMode = DebugWireframeMode.None;
        }

        public override void Execute(string[] args)
        {
            UniversalRenderPipelineDebugDisplaySettings debugDisplaySettings = UniversalRenderPipelineDebugDisplaySettings.Instance;

            isWireframe = !isWireframe;

            if (isWireframe)
            {
                debugDisplaySettings.renderingSettings.wireframeMode = DebugWireframeMode.Wireframe;
                ConsoleManager.LogToConsole("Wireframe has been enabled.".AsSuccess());
            }
            else
            {
                debugDisplaySettings.renderingSettings.wireframeMode = DebugWireframeMode.None;
                ConsoleManager.LogToConsole("Wireframe has been disabled.".AsSuccess());
            }
        }
    }
}