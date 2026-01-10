#if UNITY_EDITOR // so it can use Log
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ModMoveBuildFolder : IPostprocessBuildWithReport
{
    private static readonly string[] excludedFolderNames = { "ExampleCommand" };

    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        string buildPath = report.summary.outputPath;
        string buildDirectory = Path.GetDirectoryName(buildPath);
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string projectModsPath = Path.Combine(projectRoot, "Mods");
        string destinationModsPath = Path.Combine(buildDirectory, "Mods");

        if (Directory.Exists(projectModsPath))
        {
            string[] modDirs = Directory.GetDirectories(projectModsPath);
            Log.Editor($"Found {modDirs.Length} mod folders");

            var filteredModDirs = modDirs.Where(dir =>
            {
                string dirName = Path.GetFileName(dir);
                bool isExcluded = excludedFolderNames.Any(excluded =>
                    dirName.Equals(excluded, System.StringComparison.OrdinalIgnoreCase));

                if (isExcluded)
                {
                    Log.Editor($"Skipping excluded folder '{dirName}'");
                }

                return !isExcluded;
            }).ToArray();

            if (filteredModDirs.Length > 0)
            {
                int fileCount = 0;
                int dirCount = 0;

                foreach (string modDir in filteredModDirs)
                {
                    string modName = Path.GetFileName(modDir);
                    string destModDir = Path.Combine(destinationModsPath, modName);
                    CopyDirectory(modDir, destModDir, ref fileCount, ref dirCount);
                }

                Log.Editor($"Successfully copied {fileCount} files in {dirCount} directories to: {destinationModsPath}");
            }
        }
    }

    private void CopyDirectory(string sourceDir, string destDir, ref int fileCount, ref int dirCount)
    {
        Directory.CreateDirectory(destDir);
        dirCount++;

        // Copy all files
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);

            if (fileName.EndsWith(".meta") || fileName.EndsWith(".code-workspace"))
                continue;

            string destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, true);
            fileCount++;
        }

        // Copy all subdirectories
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destDir, dirName);
            CopyDirectory(subDir, destSubDir, ref fileCount, ref dirCount);
        }
    }
}
#endif