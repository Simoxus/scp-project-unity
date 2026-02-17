using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Console.Commands
{
    public class LoadSceneCommand : BaseConsole
    {
        public override string CommandWord => "loadscene";
        public override string Description => "Loads a specific scene by name or build index using LoadingManager.";
        protected override string RawUsage => "loadscene <sceneName|sceneIndex>";

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
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                    LoadSceneAsync(sceneName, sceneIndex).Forget();
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
                    LoadSceneAsync(sceneIdentifier).Forget();
                }
                else
                {
                    ConsoleManager.LogToConsole($"Scene '{sceneIdentifier}' not found or not in available scenes list.".AsError());
                }
            }
        }

        private async UniTaskVoid LoadSceneAsync(string sceneName, int? sceneIndex = null)
        {
            try
            {
                if (sceneIndex.HasValue)
                {
                    ConsoleManager.LogToConsole($"Loading scene using index method: '{sceneIndex.Value}' ('{sceneName}')...".AsWarning());
                }
                else
                {
                    ConsoleManager.LogToConsole($"Loading scene using name method: '{sceneName}'...".AsWarning());
                }

                await Core.LoadingManager.LoadSceneWithPressAnyKey(sceneName);

                ConsoleManager.LogToConsole($"Scene '{sceneName}' loaded successfully!".AsSuccess());
            }
            catch (System.Exception exception)
            {
                ConsoleManager.LogToConsole($"Error loading scene '{sceneName}': {exception.Message}.".AsError());
            }
        }
    }
}