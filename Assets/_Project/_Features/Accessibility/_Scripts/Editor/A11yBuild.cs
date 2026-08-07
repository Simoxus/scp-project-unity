using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Reproducible QA build for the accessibility layer, driven from batchmode:
/// Unity.exe -batchmode -quit -projectPath (proj) -executeMethod A11yBuild.BuildTestingCore
/// - Declares its scene list explicitly (the project's EditorBuildSettings ships with every scene disabled).
/// - Refreshes and copies FMOD banks explicitly (FMOD's BuildStatusWatcher never fires in CLI builds).
/// - Builds Addressables content first (the game loads audio data through Addressables).
/// - Forces the Mono scripting backend for local QA builds: upstream targets IL2CPP, which
///   requires the Visual Studio C++ toolchain that QA machines may not have. Runtime behavior
///   of the accessibility layer is identical under both backends.
/// </summary>
public static class A11yBuild
{
    public static void BuildTestingCore()
    {
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);

        FMODUnity.EventManager.RefreshBanks();
        FMODUnity.EventManager.CopyToStreamingAssets(BuildTarget.StandaloneWindows64);
        AssetDatabase.SaveAssets();

        AddressableAssetSettings.BuildPlayerContent();

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/_Project/_Scenes/Testing/Testing_Core.unity" },
            locationPathName = "Builds/A11yTest/SCPPU-Dev-A11yTest.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"A11Y BUILD RESULT: {report.summary.result}, size {report.summary.totalSize / (1024 * 1024)} MB, errors {report.summary.totalErrors}");
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
