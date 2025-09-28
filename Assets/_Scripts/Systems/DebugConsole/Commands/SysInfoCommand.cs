using UnityEngine;

namespace Console.Commands
{
    public class SysInfoCommand : ConsoleBase
    {
        public override string CommandWord => "sysinfo";
        public override string Description => "Display system/runtime information.";
        protected override string RawUsage => "sysinfo <game|metrics|device|all>";

        public override void Execute(string[] args)
        {
            if (args.Length != 1)
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                return;
            }

            string category = args[0].ToLowerInvariant();

            switch (category)
            {
                case "game":
                    ConsoleManager.LogToConsole("<color=#00FFFFFF>--- Game Information ---</color>");

                    GetGameInfo();

                    break;

                case "metrics":
                    ConsoleManager.LogToConsole("<color=#00FFFFFF>--- Metrics Information ---</color>");

                    GetMetricsInfo();

                    break;

                case "device":
                    ConsoleManager.LogToConsole("<color=#00FFFFFF>--- Device Information ---</color>");

                    GetDeviceInfo();

                    break;

                case "all":
                    ConsoleManager.LogToConsole("<color=#00FFFFFF>--- Game/Metrics/Device Information ---</color>");

                    GetGameInfo();
                    ConsoleManager.LogToConsole("<color=#00FFFFFF>----------------------------------------</color>");
                    GetMetricsInfo();
                    ConsoleManager.LogToConsole("<color=#00FFFFFF>----------------------------------------</color>");
                    GetDeviceInfo();

                    break;

                default:
                    // If category is not recognized, log an error.
                    ConsoleManager.LogToConsole($"<color=#FF0000FF>Unknown info method '{category}'. Use 'game', 'metrics', 'device', or 'all'.</color>");
                    break;
            }
        }

        private void GetGameInfo()
        {
            ConsoleManager.LogToConsole($"<b>Game Name</b>: {Application.productName}");
            ConsoleManager.LogToConsole($"<b>Version</b>: {Application.version}");
            ConsoleManager.LogToConsole($"<b>Is In Editor?</b>: {Application.isEditor}");
        }

        private void GetMetricsInfo()
        {
            ConsoleManager.LogToConsole($"<b>Delta Time</b>: {Time.deltaTime:F4}s");
            ConsoleManager.LogToConsole($"<b>Time Scale</b>: {Time.timeScale}");
            ConsoleManager.LogToConsole($"<b>Estimated FPS</b>: {(int)(1.0f / Time.deltaTime)}");
        }

        private void GetDeviceInfo()
        {
            ConsoleManager.LogToConsole($"<b>Device Type</b>: {SystemInfo.deviceType}");
            ConsoleManager.LogToConsole($"<b>Operating System</b>: {SystemInfo.operatingSystem}");
            ConsoleManager.LogToConsole($"<b>Graphics Device Name</b>: {SystemInfo.graphicsDeviceName}");
            ConsoleManager.LogToConsole($"<b>Graphics Memory Size</b>: {SystemInfo.graphicsMemorySize} MB");
            ConsoleManager.LogToConsole($"<b>Processor Type</b>: {SystemInfo.processorType}");
            ConsoleManager.LogToConsole($"<b>Processor Count</b>: {SystemInfo.processorCount}");
            ConsoleManager.LogToConsole($"<b>System Memory Size</b>: {SystemInfo.systemMemorySize} MB");
        }
    }
}
