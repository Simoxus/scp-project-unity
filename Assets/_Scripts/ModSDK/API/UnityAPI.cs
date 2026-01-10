using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

[MoonSharpUserData]
public class UnityAPI
{
    [MoonSharpVisible(true)]
    public Vector3 Vector3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

    [MoonSharpVisible(true)]
    public Vector2 Vector2(float x, float y)
    {
        return new Vector2(x, y);
    }

    [MoonSharpVisible(true)]
    public Quaternion Quaternion(float x, float y, float z, float w)
    {
        return new Quaternion(x, y, z, w);
    }

    [MoonSharpVisible(true)]
    public Quaternion QuaternionEuler(float x, float y, float z)
    {
        return UnityEngine.Quaternion.Euler(x, y, z);
    }

    [MoonSharpVisible(true)]
    public Color Color(float r, float g, float b, float a = 1f)
    {
        return new Color(r, g, b, a);
    }

    [MoonSharpVisible(true)]
    public GameObject FindGameObject(string name)
    {
        return GameObject.Find(name);
    }

    [MoonSharpVisible(true)]
    public GameObject[] FindGameObjectsWithTag(string tag)
    {
        return GameObject.FindGameObjectsWithTag(tag);
    }

    [MoonSharpVisible(true)]
    public GameObject CreateGameObject(string name)
    {
        return new GameObject(name);
    }

    [MoonSharpVisible(true)]
    public void DestroyGameObject(GameObject obj)
    {
        if (obj != null)
        {
            Object.Destroy(obj);
        }
    }

    [MoonSharpVisible(true)]
    public float Distance(Vector3 a, Vector3 b)
    {
        return UnityEngine.Vector3.Distance(a, b);
    }

    [MoonSharpVisible(true)]
    public Vector3 Vector3Lerp(Vector3 a, Vector3 b, float t)
    {
        return UnityEngine.Vector3.Lerp(a, b, t);
    }

    [MoonSharpVisible(true)]
    public float Clamp(float value, float min, float max)
    {
        return Mathf.Clamp(value, min, max);
    }

    [MoonSharpVisible(true)]
    public float Clamp01(float value)
    {
        return Mathf.Clamp01(value);
    }

    [MoonSharpVisible(true)]
    public float Lerp(float a, float b, float t)
    {
        return Mathf.Lerp(a, b, t);
    }

    [MoonSharpVisible(true)]
    public float Sin(float value)
    {
        return Mathf.Sin(value);
    }

    [MoonSharpVisible(true)]
    public float Cos(float value)
    {
        return Mathf.Cos(value);
    }

    [MoonSharpVisible(true)]
    public float Tan(float value)
    {
        return Mathf.Tan(value);
    }

    [MoonSharpVisible(true)]
    public float Sqrt(float value)
    {
        return Mathf.Sqrt(value);
    }

    [MoonSharpVisible(true)]
    public float Abs(float value)
    {
        return Mathf.Abs(value);
    }

    [MoonSharpVisible(true)]
    public float Floor(float value)
    {
        return Mathf.Floor(value);
    }

    [MoonSharpVisible(true)]
    public float Ceil(float value)
    {
        return Mathf.Ceil(value);
    }

    [MoonSharpVisible(true)]
    public float Round(float value)
    {
        return Mathf.Round(value);
    }

    [MoonSharpVisible(true)]
    public float Random(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    [MoonSharpVisible(true)]
    public int RandomInt(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    [MoonSharpVisible(true)]
    public void DrawRay(Vector3 start, Vector3 direction, float r, float g, float b, float duration = 0f)
    {
        Debug.DrawRay(start, direction, new Color(r, g, b), duration);
    }

    [MoonSharpVisible(true)]
    public Table Raycast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        Script script = new Script();
        Table result = new Table(script);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            result["hit"] = true;
            result["point"] = hit.point;
            result["normal"] = hit.normal;
            result["distance"] = hit.distance;

            if (hit.collider.gameObject != null)
            {
                result["objectName"] = hit.collider.gameObject.name;
                result["object"] = hit.collider.gameObject;
            }
        }
        else
        {
            result["hit"] = false;
        }

        return result;
    }

    [MoonSharpVisible(true)]
    public float GetPI()
    {
        return Mathf.PI;
    }

    [MoonSharpVisible(true)]
    public float GetDeg2Rad()
    {
        return Mathf.Deg2Rad;
    }

    [MoonSharpVisible(true)]
    public float GetRad2Deg()
    {
        return Mathf.Rad2Deg;
    }
}