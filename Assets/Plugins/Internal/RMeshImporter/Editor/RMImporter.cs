using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "rmesh")]
public class RMImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var go = RMeshLoader.LoadRMesh(ctx.assetPath, default);
        UnwrapParam.SetDefaults(out var para);
        Unwrapping.GenerateSecondaryUVSet(go.visibleData.mesh, para);
        ctx.AddObjectToAsset("GameObject", go.gameObject);
        ctx.AddObjectToAsset("RMesh", go);
        ctx.AddObjectToAsset("Mesh", go.visibleData.mesh);
        ctx.AddObjectToAsset("InvisMesh", go.invisibleMesh);
        ctx.AddObjectToAsset("CollisionMesh", go.collisionMesh);
        // for (int i = 0; i < go.entities.Length; i++)
            // ctx.AddObjectToAsset($"Entity{i}", go.entities[i]);
        ctx.SetMainObject(go.gameObject);
    }
}
