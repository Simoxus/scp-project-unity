using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class RMeshLoader : MonoBehaviour
{
    public const float Scale = 0.006144f;
    public const int MaxRoomEmitters = 8;

    public static RMeshData LoadRMesh(string filename, CancellationTokenSource token)
    {
        using (FileStream stream = File.Open(filename, FileMode.Open))
        {
            bool hasTriggerBox = false;
            bool hasColMesh = true;
            string header = ReadString(stream);
            if (header == "RoomMesh")
            {
            }
            else if (header == "RoomMesh.HasTriggerBox")
            {
                hasTriggerBox = true;
            }
            else if (header == "RoomMesh.HasNoColl")
            {
                hasColMesh = false;
            }
            else
            {
                return null;
            }
            string path = Path.GetDirectoryName(filename);
            List<Material> materials = new List<Material>();
            List<CombineInstance> combines = new List<CombineInstance>();
            List<CombineInstance> combinesInvis = new List<CombineInstance>();
            int count = ReadInt(stream);
            for (int i = 0; i < count; i++)
            {
                string[] tex = new string[2];
                for (int j = 0; j < 2; j++)
                {
                    byte temp1b = ReadByte(stream);
                    if (temp1b != 0)
                    {
                        string temp1s = ReadString(stream);
                        tex[j] = temp1s;
                        // tex[j] = Path.Combine(path, temp1s);
                    }
                }

                Material mat = LoadMaterial(tex, token);
                if (mat == null)
                    Debug.Log(string.Join(',', tex));
                materials.Add(mat);

                // verts
                int count2 = ReadInt(stream);
                Vector3[] vertices = new Vector3[count2];
                Vector2[] uv = new Vector2[count2];
                Vector2[] uv2 = new Vector2[count2];
                Color32[] colors = new Color32[count2];

                for (int j = 0; j < count2; j++)
                {
                    // world coords
                    float x = ReadFloat(stream);
                    float y = ReadFloat(stream);
                    float z = ReadFloat(stream);
                    vertices[j] = new Vector3(x, y, z);
                    
                    // texture coords uv
                    float u = ReadFloat(stream);
                    float v = 1f - ReadFloat(stream);
                    uv[j] = new Vector2(u, v);

                    // texture coords uv2
                    float u2 = ReadFloat(stream);
                    float v2 = 1f - ReadFloat(stream);
                    uv2[j] = new Vector2(u2, v2);

                    // colors
                    byte r = ReadByte(stream);
                    byte g = ReadByte(stream);
                    byte b = ReadByte(stream);
                    colors[j] = new Color32(r, g, b, 255);
                }

                // tris
                count2 = ReadInt(stream);
                int[] tris = new int[count2 * 3];

                for (int j = 0; j < count2; j++)
                {
                    tris[j * 3] = ReadInt(stream);
                    tris[j * 3 + 1] = ReadInt(stream);
                    tris[j * 3 + 2] = ReadInt(stream);
                }

                Mesh tempmesh = new Mesh();
                tempmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                tempmesh.vertices = vertices;
                tempmesh.triangles = tris;
                tempmesh.uv = uv;
                tempmesh.uv2 = uv2;
                tempmesh.colors32 = colors;
                combines.Add(new CombineInstance()
                {
                    mesh = tempmesh,
                    transform = Matrix4x4.identity
                });
            }

            // invisible collision mesh
            count = ReadInt(stream);

            for (int i = 0; i < count; i++)
            {
                // vertices
                int count2 = ReadInt(stream);
                Vector3[] vertices = new Vector3[count2];

                for (int j = 0; j < count2; j++)
                {
                    // world coords
                    float x = ReadFloat(stream);
                    float y = ReadFloat(stream);
                    float z = ReadFloat(stream);
                    vertices[j] = new Vector3(x, y, z);
                }

                // tris
                count2 = ReadInt(stream);
                int[] tris = new int[count2 * 3];

                for (int j = 0; j < count2; j++)
                {
                    tris[j * 3] = ReadInt(stream);
                    tris[j * 3 + 1] = ReadInt(stream);
                    tris[j * 3 + 2] = ReadInt(stream);
                }

                Mesh tempmesh = new Mesh();
                tempmesh.vertices = vertices;
                tempmesh.triangles = tris;
                combinesInvis.Add(new CombineInstance()
                {
                    mesh = tempmesh,
                    transform = Matrix4x4.identity
                });
            }

            // no col mesh
            if (!hasColMesh)
            {
                count = ReadInt(stream);
                for (int i = 0; i < count; i++)
                {
                    string[] tex = new string[2];
                    for (int j = 0; j < 2; j++)
                    {
                        byte temp1b = ReadByte(stream);
                        if (temp1b != 0)
                        {
                            string temp1s = ReadString(stream);
                            tex[j] = temp1s;
                            // tex[j] = Path.Combine(path, temp1s);
                        }
                    }

                    Material mat = LoadMaterial(tex, token);
                    materials.Add(mat);

                    // verts
                    int count2 = ReadInt(stream);
                    Vector3[] vertices = new Vector3[count2];
                    Vector2[] uv = new Vector2[count2];
                    Vector2[] uv2 = new Vector2[count2];
                    Color32[] colors = new Color32[count2];

                    for (int j = 0; j < count2; j++)
                    {
                        // world coords
                        float x = ReadFloat(stream);
                        float y = ReadFloat(stream);
                        float z = ReadFloat(stream);
                        vertices[j] = new Vector3(x, y, z);
                        
                        // texture coords uv
                        float u = ReadFloat(stream);
                        float v = 1f - ReadFloat(stream);
                        uv[j] = new Vector2(u, v);

                        // texture coords uv2
                        float u2 = ReadFloat(stream);
                        float v2 = 1f - ReadFloat(stream);
                        uv2[j] = new Vector2(u2, v2);

                        // colors
                        byte r = ReadByte(stream);
                        byte g = ReadByte(stream);
                        byte b = ReadByte(stream);
                        colors[j] = new Color32(r, g, b, 255);
                    }

                    // tris
                    count2 = ReadInt(stream);
                    int[] tris = new int[count2 * 3];

                    for (int j = 0; j < count2; j++)
                    {
                        tris[j * 3] = ReadInt(stream);
                        tris[j * 3 + 1] = ReadInt(stream);
                        tris[j * 3 + 2] = ReadInt(stream);
                    }

                    Mesh tempmesh = new Mesh();
                    tempmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    tempmesh.vertices = vertices;
                    tempmesh.triangles = tris;
                    tempmesh.uv = uv;
                    tempmesh.uv2 = uv2;
                    tempmesh.colors32 = colors;
                    combines.Add(new CombineInstance()
                    {
                        mesh = tempmesh,
                        transform = Matrix4x4.identity
                    });
                }
            }

            // trigger box
            List<RMTriggerBox> boxes = new List<RMTriggerBox>();
            if (hasTriggerBox)
            {
                count = ReadInt(stream);
                for (int i = 0; i < count; i++)
                {
                    int count2 = ReadInt(stream);
                    for (int j = 0; j < count2; j++)
                    {
                        // vertices
                        int count3 = ReadInt(stream);
                        Vector3[] vertices = new Vector3[count3];
                        for (int k = 0; k < count3; k++)
                        {
                            // world coords
                            float x = ReadFloat(stream);
                            float y = ReadFloat(stream);
                            float z = ReadFloat(stream);
                            vertices[j] = new Vector3(x, y, z);
                        }

                        // tris
                        count3 = ReadInt(stream);
                        int[] tris = new int[count3 * 3];

                        for (int k = 0; k < count3; k++)
                        {
                            tris[j * 3] = ReadInt(stream);
                            tris[j * 3 + 1] = ReadInt(stream);
                            tris[j * 3 + 2] = ReadInt(stream);
                        }

                        Mesh tempmesh = new Mesh();
                        tempmesh.vertices = vertices;
                        tempmesh.triangles = tris;
                        combinesInvis.Add(new CombineInstance()
                        {
                            mesh = tempmesh,
                            transform = Matrix4x4.identity
                        });
                    }
                    string triggerName = ReadString(stream);
                    RMTriggerBox box = new RMTriggerBox()
                    {
                        mesh = new Mesh(),
                        name = triggerName
                    };
                    box.mesh.CombineMeshes(combinesInvis.ToArray(), false, true, false);
                    combinesInvis.Clear();
                    boxes.Add(box);
                }
            }

            // visible meshes
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.name = Path.GetFileNameWithoutExtension(filename);
            mesh.CombineMeshes(combines.ToArray(), false, true, false);
            foreach (var comb in combines)
            {
                DestroyImmediate(comb.mesh);
            }
            combines.Clear();

            // invis meshes
            Mesh invisMesh = new Mesh();
            invisMesh.name = Path.GetFileNameWithoutExtension(filename);
            invisMesh.CombineMeshes(combinesInvis.ToArray(), false, true, false);
            foreach (var comb in combines)
            {
                DestroyImmediate(comb.mesh);
            }
            combinesInvis.Clear();

            GameObject go = new GameObject(Path.GetFileNameWithoutExtension(filename));
            RMeshData final = go.AddComponent<RMeshData>();
            final.visibleData = new MeshData(mesh, materials.ToArray());
            final.invisibleMesh = invisMesh;
            final.triggerBoxes = boxes.ToArray();
            final.collisionMesh = new Mesh();
            for (int j = 0; j < mesh.subMeshCount; j++)
            {
                combines.Add(new CombineInstance()
                {
                    mesh = mesh,
                    transform = Matrix4x4.identity,
                    subMeshIndex = j
                });
            }
            if (invisMesh.vertices.Length > 0)
            {
                combines.Add(new CombineInstance()
                {
                    mesh = invisMesh,
                    transform = Matrix4x4.identity
                });
            }
            final.collisionMesh.CombineMeshes(combines.ToArray(), false, true, false);
            combines.Clear();

            // entities
            List<GameObject> entities = new List<GameObject>();
            count = ReadInt(stream);
            Debug.Log($"{count} - {stream.Position}");
            for (int i = 0; i < count; i++)
            {
                string temp1s = ReadString(stream);
                switch (temp1s)
                {
                    case "screen":
                    {
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        string temp2s = ReadString(stream);
                        
                        GameObject screenGo = new GameObject(temp1s + i);
                        screenGo.transform.SetParent(final.transform);
                        screenGo.transform.localPosition = new Vector3(temp1, temp2, temp3);
                        ScreenEntity scr = screenGo.AddComponent<ScreenEntity>();
                        scr.imgpath = temp2s;
                        scr.room = final;
                    }
                    break;
                    case "waypoint":
                    {
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        GameObject entGo = new GameObject(temp1s + i);
                        entGo.transform.SetParent(final.transform);
                        entGo.transform.localPosition = new Vector3(temp1, temp2, temp3);
                    }
                    break;
                    case "light":
                    {
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        GameObject entGo = new GameObject(temp1s + i);
                        entGo.transform.SetParent(final.transform);
                        entGo.transform.localPosition = new Vector3(temp1, temp2, temp3);
                        LightEntity li = entGo.AddComponent<LightEntity>();
                        li.room = final;
                        li.type = LightType.Point;
                        li.range = ReadFloat(stream);
                        string[] strColor = ReadString(stream).Split(' ');
                        li.intensity = ReadFloat(stream);
                        // we use floats to get HDR rendering
                        float r = int.Parse(strColor[0]) / 255f;
                        float g = int.Parse(strColor[1]) / 255f;
                        float b = int.Parse(strColor[2]) / 255f;
                        li.color = new Color(r, g, b, 1f);
                        li.RefreshData();

                        if (entGo.transform.localPosition == Vector3.zero)
                        {
                            entGo.SetActive(false);
                        }
                    }
                    break;
                    case "spotlight":
                    {
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        GameObject entGo = new GameObject(temp1s + i);
                        entGo.transform.SetParent(final.transform);
                        entGo.transform.localPosition = new Vector3(temp1, temp2, temp3);
                        LightEntity li = entGo.AddComponent<LightEntity>();
                        li.room = final;
                        li.type = LightType.Spot;
                        li.range = ReadFloat(stream);
                        string[] strColor = ReadString(stream).Split(' ');
                        li.intensity = ReadFloat(stream);
                        // we use floats to get HDR rendering
                        float r = int.Parse(strColor[0]) / 255f;
                        float g = int.Parse(strColor[1]) / 255f;
                        float b = int.Parse(strColor[2]) / 255f;
                        li.color = new Color(r, g, b, 1f);
                        string[] strAng = ReadString(stream).Split(' ');
                        float pitch = float.Parse(strAng[0]);
                        float yaw = float.Parse(strAng[1]);
                        entGo.transform.localEulerAngles = new Vector3(pitch, yaw, 0f);

                        li.innerSpotAngle = ReadInt(stream);
                        li.spotAngle = ReadInt(stream);

                        if (entGo.transform.localPosition == Vector3.zero)
                        {
                            entGo.SetActive(false);
                        }
                    }
                    break;
                    case "soundemitter":
                    {
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        GameObject entGo = new GameObject(temp1s + i);
                        entGo.transform.SetParent(final.transform);
                        entGo.transform.localPosition = new Vector3(temp1, temp2, temp3);
                        SoundEntity se = entGo.AddComponent<SoundEntity>();
                        se.room = final;
                        se.soundId = ReadInt(stream);
                        se.range = ReadFloat(stream);
                        se.RefreshData();

                        if (entGo.transform.localPosition == Vector3.zero)
                        {
                            entGo.SetActive(false);
                        }
                    }
                    break;
                    case "playerstart":
                    {
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        GameObject entGo = new GameObject(temp1s + i);
                        entGo.transform.SetParent(final.transform);
                        entGo.transform.localPosition = new Vector3(temp1, temp2, temp3);
                        string[] strAng = ReadString(stream).Split(' ');
                        float pitch = float.Parse(strAng[0]);
                        float yaw = float.Parse(strAng[1]);
                        float roll = float.Parse(strAng[2]);
                        entGo.transform.localEulerAngles = new Vector3(pitch, yaw, roll);

                        if (entGo.transform.localPosition == Vector3.zero)
                        {
                            entGo.SetActive(false);
                        }
                    }
                    break;
                    case "model":
                    case "model_nocoll":
                    {
                        string file = ReadString(stream);
                        if (string.IsNullOrEmpty(file))
                        {
                            ReadFloat(stream);
                            ReadFloat(stream);
                            ReadFloat(stream);
                            break;
                        }
                        // MeshData model = await AssetCache.LoadModel(file, token);

                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        GameObject entGo = new GameObject(temp1s + i);
                        entGo.transform.SetParent(final.transform);
                        entGo.transform.localPosition = new Vector3(temp1, temp2, temp3);
                        ModelEntity mdl = entGo.AddComponent<ModelEntity>();
                        mdl.room = final;
                        mdl.visibleData = new MeshData(null, new Material[0]);//model;
                        mdl.meshName = file;

                        temp1 = ReadFloat(stream);
                        temp2 = ReadFloat(stream);
                        temp3 = ReadFloat(stream);
                        entGo.transform.localEulerAngles = new Vector3(temp1, temp2, temp3);

                        temp1 = ReadFloat(stream);
                        temp2 = ReadFloat(stream);
                        temp3 = ReadFloat(stream);
                        entGo.transform.localScale = new Vector3(temp1, temp2, temp3);

                        mdl.RefreshData();
                    }
                    break;
                    case "fusebox":
                    case "generator":
                    {
                        ReadString(stream); // file

                        if (temp1s == "generator")
                        {
                            ReadInt(stream); // sfx
                            ReadFloat(stream); // sfxrange
                            ReadInt(stream); // id
                        }

                        // position
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        // rotation
                        temp1 = ReadFloat(stream);
                        temp2 = ReadFloat(stream);
                        temp3 = ReadFloat(stream);

                        // scale
                        temp1 = ReadFloat(stream);
                        temp2 = ReadFloat(stream);
                        temp3 = ReadFloat(stream);
                    }
                    break;
                    case "button_gen":
                    case "lever_gen":
                    {
                        ReadString(stream); // file

                        if (temp1s == "lever_gen")
                        {
                            ReadString(stream); // file_handle
                            ReadInt(stream); // angle
                        }

                        ReadInt(stream); // id

                        // position
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        // rotation
                        temp1 = ReadFloat(stream);
                        temp2 = ReadFloat(stream);
                        temp3 = ReadFloat(stream);

                        // scale
                        temp1 = ReadFloat(stream);
                        temp2 = ReadFloat(stream);
                        temp3 = ReadFloat(stream);
                    }
                    break;
                    case "particle_gen":
                    {
                        // position
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        ReadByte(stream); // ptype
                        ReadString(stream); // angles
                        ReadInt(stream); // id
                    }
                    break;
                    case "mp_damageboss_radius":
                    {
                        // position
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        ReadFloat(stream); // range
                        ReadInt(stream); // id
                    }
                    break;
                    case "mp_playerspawn":
                    {
                        // position
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        ReadByte(stream); // team
                        ReadFloat(stream); // yaw
                    }
                    break;
                    case "mp_enemyspawn":
                    {
                        // position
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        ReadString(stream); // enemyString
                    }
                    break;
                    case "mp_itemspawn":
                    {
                        // position
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        ReadByte(stream); // ittype
                        ReadInt(stream); // respawntime
                        ReadString(stream); // rndtime
                    }
                    break;
                    case "flu_light":
                    {
                        // position
                        float temp1 = ReadFloat(stream);
                        float temp2 = ReadFloat(stream);
                        float temp3 = ReadFloat(stream);

                        // rotation
                        temp1 = ReadFloat(stream);
                        temp2 = ReadFloat(stream);
                        temp3 = ReadFloat(stream);

                        ReadInt(stream); // id
                    }
                    break;
                    default:
                    {
                        GameObject nullgo = new GameObject(temp1s + i);
                        nullgo.transform.SetParent(final.transform);
                    }
                    break;
                }
            }
            final.entities = entities.ToArray();

            final.RefreshData();
            return final;
        }
    }

    public static Material LoadMaterial(string[] tex, CancellationTokenSource token)
    {
        #if UNITY_EDITOR
        var data = UnityEditor.AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(tex[1]) + " t:Material");
        string path = string.Empty;
        if (data.Length > 0)
        {
            path = UnityEditor.AssetDatabase.GUIDToAssetPath(data[0]);
            foreach (var item in data)
            {
                bool cmp = Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GUIDToAssetPath(item)).ToLower().StartsWith(Path.GetFileNameWithoutExtension(tex[1]).ToLower(), StringComparison.InvariantCultureIgnoreCase);
                if (cmp)
                {
                    path = UnityEditor.AssetDatabase.GUIDToAssetPath(item);
                    break;
                }
            }
        }
        // Material mat = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(x => x?.mainTexture?.name == tex[1]);
        Material mat = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path) as Material;
        return mat;
        #else
        return null;
        #endif
        /*
        Texture2D mainTex = await AssetCache.LoadTexture(tex[1], token);
        mainTex.alphaIsTransparency = true;
        bool clear = false;
        foreach (var col in mainTex.GetPixels(0, 0, mainTex.width, mainTex.height))
        {
            if (col.a < 1f)
            {
                clear = true;
                break;
            }
        }
        if (clear)
        {
            mat = new Material(Resources.Load<Material>("RoomMaterialAlpha"));
        }
        else
        {
            mat = new Material(Resources.Load<Material>("RoomMaterial"));
        }
        mat.SetTexture("_MainTex", mainTex);
        mat.SetTexture("_BumpMap", await AssetCache.LoadTextureBump(tex[1], token));
        mat.SetTexture("_AltTex", await AssetCache.LoadTexture(tex[0], token));
        mat.name = Path.GetFileNameWithoutExtension(tex[1]);
        return mat;
        */
    }

    public static string ReadString(Stream stream)
    {
        int len = ReadInt(stream);
        byte[] bytes = new byte[len];
        stream.Read(bytes, 0, bytes.Length);
        return Encoding.ASCII.GetString(bytes);
    }

    public static int ReadInt(Stream stream)
    {
        byte[] bytes = new byte[sizeof(int)];
        stream.Read(bytes, 0, bytes.Length);
        return BitConverter.ToInt32(bytes, 0);
    }

    public static float ReadFloat(Stream stream)
    {
        byte[] bytes = new byte[sizeof(float)];
        stream.Read(bytes, 0, bytes.Length);
        return BitConverter.ToSingle(bytes, 0);
    }

    public static byte ReadByte(Stream stream)
    {
        return (byte)stream.ReadByte();
    }
}
