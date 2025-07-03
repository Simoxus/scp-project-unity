using UnityEditor;
using UnityEngine;
using System.IO;

public static class BatchRenameAssets
{
    [MenuItem("Assets/Production Tools/Rename Selected Assets to Source", false)]
    public static void RenameScriptableObjects()
    {
        Object[] selected = Selection.objects;

        if (selected.Length < 2)
        {
            Debug.LogWarning("Select at least two assets. The first will be the name base.");
            return;
        }

        // First selected item provides the base name
        string basePath = AssetDatabase.GetAssetPath(selected[0]);
        string baseName = Path.GetFileNameWithoutExtension(basePath);

        int renamedCount = 0;

        for (int i = 1; i < selected.Length; i++)
        {
            string assetPath = AssetDatabase.GetAssetPath(selected[i]);

            // Ensure it's a ScriptableObject
            ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (obj == null)
            {
                Debug.Log($"Skipped: {selected[i].name} (not a ScriptableObject)");
                continue;
            }

            string directory = Path.GetDirectoryName(assetPath);
            string newName = $"{baseName}_RoomData.asset";
            string newFullPath = Path.Combine(directory, newName);

            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(newFullPath);

            AssetDatabase.RenameAsset(assetPath, Path.GetFileNameWithoutExtension(uniquePath));
            renamedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Renamed {renamedCount} ScriptableObject(s) to format: {baseName}_RoomData");
    }


    [MenuItem("Assets/Rename Selected Assets to Source", true)]
    public static bool ValidateRenameScriptableObjects()
    {
        return Selection.objects.Length >= 2;
    }
}
