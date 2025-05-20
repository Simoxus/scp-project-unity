using UnityEngine;

public class MeshData
{
    public Mesh mesh;
    public Material[] materials;

    public MeshData()
    {}

    public MeshData(Mesh mesh, Material[] materials)
    {
        this.mesh = mesh;
        this.materials = materials;
    }
}