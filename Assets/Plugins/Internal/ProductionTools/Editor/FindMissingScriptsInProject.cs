using UnityEngine;
using UnityEditor;
using System.IO;

public class FindMissingScriptsInProject : MonoBehaviour
{
    [MenuItem("Tools/Production Tools/Find Missing Scripts in Project Prefabs")]
    public static void FindMissingScriptsInPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            foreach (Component c in components)
            {
                if (c == null)
                {
                    Debug.LogWarning($"Missing script in prefab: {path}", prefab);
                    break;
                }
            }
        }
    }
}
