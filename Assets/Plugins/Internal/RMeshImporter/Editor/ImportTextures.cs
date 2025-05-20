using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ImportTextures
{
    [MenuItem("Tools/Import Textures")]
    public static void ImportSelected()
    {
        foreach (var obj in Selection.objects)
        {
            if (obj is Texture tex)
            {
                string path = AssetDatabase.GetAssetPath(tex) + ".mat";
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.mainTexture = tex;
                AssetDatabase.CreateAsset(mat, path);
            }
        }
    }
}
