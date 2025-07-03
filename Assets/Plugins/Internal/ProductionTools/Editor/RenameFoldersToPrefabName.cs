using UnityEngine;
using UnityEditor;
using System.IO;

public class RenameFoldersToPrefabName
{
    [MenuItem("Tools/Production Tools/Rename Folders To Prefab Name")]
    public static void RenameSelectedFolders()
    {
        var selectedGuids = Selection.assetGUIDs;

        foreach (string guid in selectedGuids)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"'{folderPath}' is not a folder. Skipping.");
                continue;
            }

            string[] files = Directory.GetFiles(folderPath, "*.prefab");

            if (files.Length == 0)
            {
                Debug.LogWarning($"No prefab found in '{folderPath}'. Skipping.");
                continue;
            }

            string prefabPath = files[0].Replace("\\", "/");
            string prefabName = Path.GetFileNameWithoutExtension(prefabPath);

            string parentDir = Path.GetDirectoryName(folderPath).Replace("\\", "/");
            string newFolderPath = Path.Combine(parentDir, prefabName).Replace("\\", "/");

            if (folderPath == newFolderPath)
            {
                Debug.Log($"Folder already named '{prefabName}', skipping.");
                continue;
            }

            string error = AssetDatabase.RenameAsset(folderPath, prefabName);
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log($"Renamed folder to: {prefabName}");
            }
            else
            {
                Debug.LogError($"Failed to rename folder: {error}");
            }
        }

        AssetDatabase.Refresh();
    }
}
