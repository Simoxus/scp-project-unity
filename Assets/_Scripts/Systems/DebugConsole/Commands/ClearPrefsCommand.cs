using UnityEditor;
using UnityEngine;

namespace Console.Commands
{
    public class ClearPrefsCommand : BaseConsole
    {
        public override string CommandWord => "clearprefs";
        public override string Description => "Deletes all PlayerPrefs data.";

        protected override string RawUsage => "clearprefs";

        public override void Execute(string[] args)
        {
            PlayerPrefs.DeleteAll();
            ConsoleManager.LogToConsole($"<color=#33CC33>All PlayerPrefs data has been cleared.</color>");
        }
    }
}