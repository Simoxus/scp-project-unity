using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildCleaner : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        bool isDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;
        if (isDevelopmentBuild) return;

        string buildFolder = Path.GetDirectoryName(report.summary.outputPath);
        string settingsFilePath = Path.Combine(buildFolder, "settings.json");
        string savesFolderPath = Path.Combine(buildFolder, "Saves");

        if (File.Exists(settingsFilePath))
        {
            File.Delete(settingsFilePath);
        }

        if (Directory.Exists(savesFolderPath))
        {
            Directory.Delete(savesFolderPath, true);
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
            }
            catch (IOException)
            {
            }
        }
    }
}