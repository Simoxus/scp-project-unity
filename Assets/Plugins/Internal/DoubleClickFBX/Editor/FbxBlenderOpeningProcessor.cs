// Place inside an "Editor" folder
#if PLATFORM_STANDALONE_WIN
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class FbxBlenderOpeningProcessor
{
    // Set this variable to your installation path. 
    // If the path was added to your "PATH" system environment variable, it should be 
    // possible to just use "blender" as path
    private static readonly string blenderPath = @"C:\Program Files\Blender Foundation\Blender 4.4\blender.exe";

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
        string assetPath = AssetDatabase.GetAssetPath(instanceId);
        if (string.Equals(Path.GetExtension(assetPath), ".fbx", StringComparison.OrdinalIgnoreCase)
            && obj is GameObject)
        {
            Debug.Log($"Open FBX file \"{assetPath}\" in Blender");
            OpenAsFbxInBlender(assetPath);
            return true; // Prevent Unity from further processing the opening task
        }
        return false; // Continue builtin handling 
    }

    private static void OpenAsFbxInBlender(string assetPath)
    {
        string assetPathPythonFull = Path.GetFullPath(assetPath).Replace("\\", "/");
        // Delete default cube and import fbx:
        string[] instructions = new string[]
        {
            "import bpy",
            //"bpy.data.objects['Cube'].select_set(True)",
            //"bpy.ops.object.delete()",
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
