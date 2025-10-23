using System.Collections.Generic;
using UnityEngine;
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
                ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                return;
            }

            string sceneIdentifier = args[0];

            if (int.TryParse(sceneIdentifier, out int sceneIndex))
            {
                if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(sceneIndex);
                    ConsoleManager.LogToConsole($"<color=#33CC33>Loading scene using index method: '{sceneIndex}'...</color>");
                }
                else
                {
                    ConsoleManager.LogToConsole($"<color=#FF0000FF>Scene index {sceneIndex} is out of build settings range (0 to {SceneManager.sceneCountInBuildSettings - 1}).</color>");
                }
            }
            else
            {
                if (_availableScenes.Contains(sceneIdentifier))
                {
                    try
                    {
                        SceneManager.LoadScene(sceneIdentifier);
                        ConsoleManager.LogToConsole($"<color=#33CC33>Loading scene using name method: '{sceneIdentifier}'...</color>");
                    }
                    catch (System.Exception exception)
                    {
                        ConsoleManager.LogToConsole($"<color=#FF0000FF>Error loading scene '{sceneIdentifier}': {exception.Message}.</color>");
                    }
                }
                else
                {
                    ConsoleManager.LogToConsole($"<color=#FF0000FF>Scene '{sceneIdentifier}' not found or not in available scenes list. Please check spelling or use a valid build index.</color>");
                }
            }
        }
    }
}