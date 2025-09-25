using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCommand : ConsoleBase
{
    public override string CommandWord => "scene";
    public override string Description => "Loads a specific scene by name or build index. Usage: scene <sceneName|sceneIndex> [optional: reload]";

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
            ConsoleManager.LogToConsole("<color=#FF0000FF>Usage: scene <sceneName|sceneIndex> [optional: reload]</color>");
            return;
        }

        bool reload = args.Contains("reload");
        string sceneIdentifier = args[0];

        if (sceneIdentifier.ToLower() == "reload")
        {
            // Just reload current scene
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
            ConsoleManager.LogToConsole($"<color=#33CC33>Reloading current scene '{currentScene}'...</color>");
            return;
        }

        if (int.TryParse(sceneIdentifier, out int sceneIndex))
        {
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(sceneIndex);
                ConsoleManager.LogToConsole($"<color=#33CC33>Loading scene by index: {sceneIndex}...</color>");

                if (reload)
                {
                    SceneManager.LoadScene(sceneIndex);
                    ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Reloading scene index {sceneIndex}...</color>");
                }
            }
            else
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>Error: Scene index {sceneIndex} is out of build settings range (0 to {SceneManager.sceneCountInBuildSettings - 1}).</color>");
            }
        }
        else
        {
            if (_availableScenes.Contains(sceneIdentifier))
            {
                try
                {
                    SceneManager.LoadScene(sceneIdentifier);
                    ConsoleManager.LogToConsole($"<color=#33CC33>Loading scene by name: '{sceneIdentifier}'...</color>");

                    if (reload)
                    {
                        SceneManager.LoadScene(sceneIdentifier);
                        ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Reloading scene '{sceneIdentifier}'...</color>");
                    }
                }
                catch (System.Exception e)
                {
                    ConsoleManager.LogToConsole($"<color=#FF0000FF>Error loading scene '{sceneIdentifier}': {e.Message}. Ensure it's in Build Settings and spelled correctly.</color>");
                    Debug.LogError($"Error loading scene '{sceneIdentifier}': {e}");
                }
            }
            else
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>Error: Scene '{sceneIdentifier}' not found or not in available scenes list. Please check spelling or use a valid build index.</color>");
            }
        }
    }
}
