// thanks to FleshMobProductions https://gist.github.com/FleshMobProductions/f598096b705f6a9c96beb58e284303f1

#if PLATFORM_STANDALONE_WIN
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class FbxBlenderOpeningProcessor
{
    private static readonly string blenderPath = @"C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe";

    private static string PythonScriptPath
    {
        get
        {
            string editorPath = Path.Combine(Application.dataPath, "Plugins", "Internal", "DoubleClickFBX", "Editor", "blender_unity_bridge.py");
            if (!File.Exists(editorPath)) return null;
            return editorPath;
        }
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
        string assetPath = AssetDatabase.GetAssetPath(instanceId);

        if (string.Equals(Path.GetExtension(assetPath), ".fbx", StringComparison.OrdinalIgnoreCase)
            && obj is GameObject)
        {
            Debug.Log($"Opening FBX file '{assetPath}' in Blender");
            OpenAsFbxInBlender(assetPath);
            return true;
        }
        return false;
    }

    private static void OpenAsFbxInBlender(string assetPath)
    {
        if (!File.Exists(blenderPath))
        {
            Debug.LogError($"Blender not found at\nUpdate blenderPath in FbxBlenderOpeningProcessor");
            return;
        }

        string pythonScript = PythonScriptPath;
        if (pythonScript == null) return;

        string fbxFullPath = Path.GetFullPath(assetPath);
        string arguments = $"--python \"{pythonScript}\" -- \"{fbxFullPath}\"";

        StartBlenderWithArguments(arguments);
    }

    private static void StartBlenderWithArguments(string arguments)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = blenderPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            Arguments = arguments
        };
        process.StartInfo = startInfo;
        process.Start();
    }
}
#endif