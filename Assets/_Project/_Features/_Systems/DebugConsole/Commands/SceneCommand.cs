using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Console.Commands
{
    public class SceneCommand : BaseConsole
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
        }

        public override void Execute(string[] args)
        {
            if (args.Length != 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string sceneIdentifier = args[0];

            if (int.TryParse(sceneIdentifier, out int sceneIndex))
            {
                if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(sceneIndex);
                    ConsoleManager.LogToConsole($"Loading scene using index method: '{sceneIndex}'...".AsWarning());
                }
                else
                {
                    ConsoleManager.LogToConsole($"Scene index {sceneIndex} is out of build settings range (0 to {SceneManager.sceneCountInBuildSettings - 1}).".AsError());
                }
            }
            else
            {
                if (_availableScenes.Contains(sceneIdentifier))
                {
                    try
                    {
                        SceneManager.LoadScene(sceneIdentifier);
                        ConsoleManager.LogToConsole($"Loading scene using name method: '{sceneIdentifier}'...".AsWarning());
                    }
                    catch (System.Exception exception)
                    {
                        ConsoleManager.LogToConsole($"Error loading scene '{sceneIdentifier}': {exception.Message}.".AsError());
                    }
                }
                else
                {
                    ConsoleManager.LogToConsole($"Scene '{sceneIdentifier}' not found or not in available scenes list.".AsError());
                }
            }
        }
    }
}