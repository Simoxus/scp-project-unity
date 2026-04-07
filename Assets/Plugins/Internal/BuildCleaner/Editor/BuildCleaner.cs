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

        DeleteFileIfExists(Path.Combine(buildFolder, "settings.json"));
        DeleteDirectoryIfExists(Path.Combine(buildFolder, "Saves"));
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        bool isDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;
        if (isDevelopmentBuild) return;

        string buildFolder = Path.GetDirectoryName(report.summary.outputPath);

        DeleteBurstDebugFolders(buildFolder);
        DeleteBackupFolders(buildFolder);
    }

    private static void DeleteBurstDebugFolders(string buildFolder)
    {
        var directories = Directory.GetDirectories(buildFolder);

        foreach (var dir in directories)
        {
            if (dir.EndsWith("_BurstDebugInformation_DoNotShip"))
            {
                DeleteDirectoryIfExists(dir);
            }
        }
    }

    private static void DeleteBackupFolders(string buildFolder)
    {
        var directories = Directory.GetDirectories(buildFolder);

        foreach (var dir in directories)
        {
            if (dir.EndsWith("_BackUpThisFolder_ButDontShipItWithYourGame"))
            {
                DeleteDirectoryIfExists(dir);
            }
        }
    }

    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}