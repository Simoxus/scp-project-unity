using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

namespace Console.Commands
{
    public class FmodCommand : BaseConsole
    {
        public override string CommandWord => "fmod";
        public override string Description => "Debug FMOD events.";
        protected override string RawUsage => "fmod <play|playinst|stop|list[optional: refresh]> <eventPath>";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string action = args[0].ToLowerInvariant();

            Vector3 soundPlayPosition = Vector3.zero;
            soundPlayPosition = Core.Player.transform.position;

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
                        ConsoleManager.LogToConsole(Usage.AsError());
                        return;
                    }

                    string rawName = args[1];
                    string eventPath = rawName.StartsWith("event:/") ? rawName : $"event:/{rawName}";
                    EventReference evRef = RuntimeManager.PathToEventReference(eventPath);

                    switch (action)
                    {
                        case "play":
                            FMODHelper.PlayOneShot3D(evRef, Vector3.zero);
                            ConsoleManager.LogToConsole($"Played FMOD sound oneshot '{eventPath}'".AsSuccess());
                            break;

                        case "playinst":
                            FMODHelper.PlayInstance(evRef, eventPath, Vector3.zero);
                            ConsoleManager.LogToConsole($">Started FMOD sound instance '{eventPath}'".AsSuccess());
                            break;

                        case "stop":
                            FMODHelper.StopInstance(eventPath);
                            ConsoleManager.LogToConsole($"Stopped FMOD sound instance '{eventPath}'".AsSuccess());
                            break;
                    }
                    break;

                default:
                    ConsoleManager.LogToConsole($"Unknown FMOD method '{action}'. Use 'play', 'playinst', 'stop' or 'list[optional: refresh]'.".AsError());
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
                ConsoleManager.LogToConsole($">FMOD events list has been refreshed.".AsSuccess());

                return;
            }

            if (cachedEventPaths == null)
            {
                GetAllEvents();
            }

            if (cachedEventPaths.Count == 0)
            {
                ConsoleManager.LogToConsole("No FMOD events found :( (make sure banks are initalized).".AsError());
                return;
            }

            ConsoleManager.LogToConsole($"--- FMOD Events ({cachedEventPaths.Count} events) ---".AsHeader());
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
                    ConsoleManager.LogToConsole($"[{(string.IsNullOrEmpty(folder) ? "Root" : folder)}]".AsInfo());
                    lastFolder = folder;
                }

                ConsoleManager.LogToConsole($"  {cleanPath}");
            }

            ConsoleManager.LogToConsole("--------------------------".AsHeader());
        }
    }
}