using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildCleaner : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        bool isDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;
        if (isDevelopmentBuild) return;

        string buildFolder = Path.GetDirectoryName(report.summary.outputPath);
        string settingsFilePath = Path.Combine(buildFolder, "settings.json");

        if (File.Exists(settingsFilePath))
        {
            Debug.Log($"[Prebuild] Deleting old settings.json file from previous build output.");
            File.Delete(settingsFilePath);
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        bool isDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;
        if (isDevelopmentBuild) return;

        string buildFolder = Path.GetDirectoryName(report.summary.outputPath);
        string burstFolder = Path.Combine(
            buildFolder,
            $"{Path.GetFileNameWithoutExtension(report.summary.outputPath)}_BurstDebugInformation_DoNotShip"
        );

        if (Directory.Exists(burstFolder))
        {
            try
            {
                Directory.Delete(burstFolder, true);
                Debug.Log($"[Postbuild] Deleted Burst debug folder from new build output.");
            }
            catch (IOException)
            {

            }
        }
    }
}