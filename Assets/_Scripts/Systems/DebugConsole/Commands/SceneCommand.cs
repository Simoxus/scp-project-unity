using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Console.Commands
{
    public class SceneCommand : ConsoleBase
    {
        public override string CommandWord => "scene";
        public override string Description => "Loads a specific scene by name or build index.";
        protected override string RawUsage => "scene <sceneName|sceneIndex>";

        private static readonly List<string> _availableScenes = new List<string>();

        public static void PopulateAvailableScenes()
        {
            _availableScenes.Clear();

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (!string.IsNullOrEmpty(sceneName) && !_availableScenes.Contains(sceneName))
                {
                    _availableScenes.Add(sceneName);
                }
            }

            Debug.Log($"SceneCommand: Loaded {_availableScenes.Count} scenes for autocomplete.");
        }

        public override void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                return;
            }

            string sceneIdentifier = args[0];
        }
    }
}