using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

namespace Console.Commands
{
    public class FmodCommand : ConsoleBase
    {
        public override string CommandWord => "fmod";
        public override string Description => "Debug FMOD events.";
        protected override string RawUsage => "fmod <play|playinst|stop|list[optional: refresh]> <eventPath>";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                return;
            }

            string action = args[0].ToLowerInvariant();

            Vector3 soundPlayPosition = Vector3.zero;
            if (Player.Instance)
            {
                soundPlayPosition = Player.Instance.transform.position;
            }

            switch (action)
            {
                case "list":
                    bool refresh = args.Length > 1 && args[1].ToLowerInvariant() == "refresh";
                    PrintAllEvents(refresh);
                    break;

                case "play":
                case "playinst":
                case "stop":
                    if (args.Length < 2)
                    {
                        ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                        return;
                    }

                    string rawName = args[1];
                    string eventPath = rawName.StartsWith("event:/") ? rawName : $"event:/{rawName}";
                    EventReference evRef = FMODUnity.RuntimeManager.PathToEventReference(eventPath);

                    switch (action)
                    {
                        case "play":
                            FMODHelper.PlayOneShot3D(evRef, Vector3.zero);
                            ConsoleManager.LogToConsole($"<color=#33CC33>Played FMOD sound oneshot '{eventPath}'</color>");
                            break;

                        case "playinst":
                            FMODHelper.PlayInstance(evRef, eventPath, Vector3.zero);
                            ConsoleManager.LogToConsole($"<color=#33CC33>Started FMOD sound instance '{eventPath}'</color>");
                            break;

                        case "stop":
                            FMODHelper.StopInstance(eventPath);
                            ConsoleManager.LogToConsole($"<color=#33CC33>Stopped FMOD sound instance '{eventPath}'</color>");
                            break;
                    }
                    break;

                default:
                    ConsoleManager.LogToConsole($"<color=#FF0000FF>Unknown FMOD method '{action}'. Use 'play', 'playinst', 'stop' or 'list[optional: refresh]'.</color>");
                    break;
            }
        }

        private static List<string> cachedEventPaths;

        private void GetAllEvents()
        {
            cachedEventPaths = new List<string>();

            // Collect events from all (loaded) banks
            RuntimeManager.StudioSystem.getBankList(out var bankArray);
            foreach (var bank in bankArray)
            {
                bank.getEventList(out var events);
                foreach (var ev in events)
                {
                    ev.getPath(out string path);
                    if (!string.IsNullOrEmpty(path))
                        cachedEventPaths.Add(path);
                }
            }

            cachedEventPaths.Sort(System.StringComparer.OrdinalIgnoreCase);
        }

        private void PrintAllEvents(bool refresh = false)
        {
            if (refresh)
            {
                GetAllEvents();
                ConsoleManager.LogToConsole($"<color=#33CC33>FMOD events list has been refreshed.</color>");

                return;
            }

            if (cachedEventPaths == null)
            {
                GetAllEvents();
            }

            if (cachedEventPaths.Count == 0)
            {
                ConsoleManager.LogToConsole("<color=#FF0000FF>No FMOD events found :( (make sure banks are initalized).</color>");
                return;
            }

            ConsoleManager.LogToConsole("<color=#00FFFFFF>--- FMOD Events ---</color>");
            ConsoleManager.LogToConsole("<size=17><color=#808080>missing an event you know is loaded? do 'fmod list refresh'</color></size>");

            string lastFolder = "";

            foreach (var path in cachedEventPaths)
            {
                string cleanPath = path.Replace("event:/", "");
                int lastSlash = cleanPath.LastIndexOf('/');
                string folder = lastSlash >= 0 ? cleanPath.Substring(0, lastSlash) : "";

                // Print a folder header if moved into a new folder
                if (folder != lastFolder)
                {
                    ConsoleManager.LogToConsole($"<color=#CCCCCC>[{(string.IsNullOrEmpty(folder) ? "Root" : folder)}]</color>");
                    lastFolder = folder;
                }

                ConsoleManager.LogToConsole($"  {cleanPath}");
            }

            ConsoleManager.LogToConsole("<color=#00FFFFFF>--------------------------</color>");
        }
    }
}