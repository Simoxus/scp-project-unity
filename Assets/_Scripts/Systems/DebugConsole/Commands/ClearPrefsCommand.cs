using UnityEditor;
using UnityEngine;

namespace Console.Commands
{
    public class ClearPrefsCommand : ConsoleBase
    {
        public override string CommandWord => "clearprefs";

#if UNITY_EDITOR
        public override string Description => "Deletes all PlayerPrefs and EditorPrefs (EDITOR ONLY) data.";
#else
        public override string Description => "Deletes all PlayerPrefs data.";
#endif

        protected override string RawUsage => "clearprefs";

        public override void Execute(string[] args)
        {
            PlayerPrefs.DeleteAll();
            ConsoleManager.LogToConsole($"<color=#33CC33>All PlayerPrefs data has been cleared.</color>");

#if UNITY_EDITOR
            EditorPrefs.DeleteAll();
            ConsoleManager.LogToConsole("<color=#33CC33>All EditorPrefs data has been cleared.</color>");
#endif
        }
    }
}