using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ModelEntity : MonoBehaviour
{
    public RMeshData room;
    public MeshData visibleData;
    public string meshName;

    public void RefreshData()
    {
        GetComponent<MeshFilter>().sharedMesh = visibleData.mesh;
        GetComponent<MeshRenderer>().sharedMaterials = visibleData.materials;
        GetComponent<MeshCollider>().sharedMesh = visibleData.mesh;
    }
}