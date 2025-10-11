#if PLATFORM_STANDALONE_WIN
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class FbxBlenderOpeningProcessor
{
    private static readonly string blenderPath = @"C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe";

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
        string assetPath = AssetDatabase.GetAssetPath(instanceId);
        if (string.Equals(Path.GetExtension(assetPath), ".fbx", StringComparison.OrdinalIgnoreCase)
            && obj is GameObject)
        {
            Debug.Log($"Opening FBX file \"{assetPath}\" in Blender");
            OpenAsFbxInBlender(assetPath);
            return true; // Prevent Unity from further processing the opening task
        }
        return false;
    }

    private static void OpenAsFbxInBlender(string assetPath)
    {
        string assetPathPythonFull = Path.GetFullPath(assetPath).Replace("\\", "/");

        // Delete default cube and import FBX file
        string[] instructions = new string[]
        {
            "import bpy",
            $"bpy.ops.import_scene.fbx( filepath = '{assetPathPythonFull}' )"
        };

        string pythonLoadFbxArgument = CreatePythonExpressionArgument(instructions);
        StartBlenderWithArguments(pythonLoadFbxArgument);
    }
    private static string CreatePythonExpressionArgument(string[] pythonLines)
    {
        // Multi line instructions possible using instruction separation with ;
        return $"--python-expr \"{string.Join(";", pythonLines)}\"";
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
