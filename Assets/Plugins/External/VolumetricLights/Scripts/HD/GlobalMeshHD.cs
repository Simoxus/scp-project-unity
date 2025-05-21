using UnityEngine;

namespace VLB
{
    public static class GlobalMeshHD
    {
        public static Mesh Get()
        {
            if (ms_Mesh == null)
            {
                Destroy();

                ms_Mesh = MeshGenerator.GenerateConeZ_Radii_DoubleCaps(
                    lengthZ: 1f,
                    radiusStart: 1f,
                    radiusEnd: 1f,
                    numSides: Config.Instance.sharedMeshSides,
                    inverted: true);

                ms_Mesh.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
            }

            return ms_Mesh;
        }

        public static void Destroy()
        {
            if (ms_Mesh != null)
            {
                GameObject.DestroyImmediate(ms_Mesh);
                ms_Mesh = null;
            }
        }

        static Mesh ms_Mesh = null;
    }
}
