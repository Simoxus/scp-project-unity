using UnityEngine;

namespace Console.Commands
{
    public class SysInfoCommand : BaseConsole
    {
        public override string CommandWord => "sysinfo";
        public override string Description => "Display system/runtime information.";
        protected override string RawUsage => "sysinfo <game|metrics|device|all>";

        public override void Execute(string[] args)
        {
            if (args.Length != 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string category = args[0].ToLowerInvariant();

            switch (category)
            {
                case "game":
                    ConsoleManager.LogToConsole("--- Game Information ---".AsHeader());

                    GetGameInfo();

                    break;

                case "metrics":
                    ConsoleManager.LogToConsole("--- Metrics Information ---".AsHeader());

                    GetMetricsInfo();

                    break;

                case "device":
                    ConsoleManager.LogToConsole("--- Device Information ---".AsHeader());

                    GetDeviceInfo();

                    break;

                case "all":
                    ConsoleManager.LogToConsole("--- Game/Metrics/Device Information ---".AsHeader());

                    GetGameInfo();
                    ConsoleManager.LogToConsole("----------------------------------------".AsHeader());
                    GetMetricsInfo();
                    ConsoleManager.LogToConsole("----------------------------------------".AsHeader());
                    GetDeviceInfo();

                    break;

                default:
                    ConsoleManager.LogToConsole(Usage.AsError());
                    break;
            }
        }

        private void GetGameInfo()
        {
            ConsoleManager.LogToConsole($"<b>Game Name</b>: {Application.productName}".AsInfo());
            ConsoleManager.LogToConsole($"<b>Version</b>: {Application.version}".AsInfo());
            ConsoleManager.LogToConsole($"<b>Is In Editor?</b>: {Application.isEditor}".AsInfo());
        }

        private void GetMetricsInfo()
        {
            ConsoleManager.LogToConsole($"<b>Delta Time</b>: {Time.deltaTime:F4}s".AsInfo());
            ConsoleManager.LogToConsole($"<b>Time Scale</b>: {Time.timeScale}".AsInfo());
            ConsoleManager.LogToConsole($"<b>Estimated FPS</b>: {(int)(1.0f / Time.deltaTime)}".AsInfo());
        }

        private void GetDeviceInfo()
        {
            ConsoleManager.LogToConsole($"<b>Device Type</b>: {SystemInfo.deviceType}".AsInfo());
            ConsoleManager.LogToConsole($"<b>Operating System</b>: {SystemInfo.operatingSystem}".AsInfo());
            ConsoleManager.LogToConsole($"<b>Graphics Device Name</b>: {SystemInfo.graphicsDeviceName}".AsInfo());
            ConsoleManager.LogToConsole($"<b>Graphics Memory Size</b>: {SystemInfo.graphicsMemorySize} MB".AsInfo());
            ConsoleManager.LogToConsole($"<b>Processor Type</b>: {SystemInfo.processorType}".AsInfo());
            ConsoleManager.LogToConsole($"<b>Processor Count</b>: {SystemInfo.processorCount}".AsInfo());
            ConsoleManager.LogToConsole($"<b>System Memory Size</b>: {SystemInfo.systemMemorySize} MB".AsInfo());
        }
    }
}
