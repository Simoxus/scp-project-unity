using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

[ModAPI("Unity")]
[MoonSharpUserData]
public class UnityAPI
{
    [MoonSharpVisible(true)]
    [LuaDoc("Shoots a ray from a world position in a given direction. Returns a table with: hit (bool), point (Vector3), normal (Vector3), distance (number), objectName (string), object (GameObject).")]
    [LuaParam("origin", "World position to start the ray from")]
    [LuaParam("direction", "Direction to shoot the ray")]
    [LuaParam("maxDistance", "Maximum ray distance in units")]
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
    [LuaDoc("Finds a GameObject in the scene by name. Returns nil if not found. Avoid calling every frame.")]
    [LuaParam("name", "Exact name of the GameObject to find")]
    public GameObject FindGameObject(string name) => GameObject.Find(name);

    [MoonSharpVisible(true)]
    [LuaDoc("Returns all active GameObjects in the scene with the given tag. Avoid calling every frame.")]
    [LuaParam("tag", "Tag to search for")]
    public GameObject[] FindGameObjectsWithTag(string tag) => GameObject.FindGameObjectsWithTag(tag);

    [MoonSharpVisible(true)]
    [LuaDoc("Creates and returns a new empty GameObject with the given name.")]
    [LuaParam("name", "Name for the new GameObject")]
    public GameObject CreateGameObject(string name) => new GameObject(name);

    [MoonSharpVisible(true)]
    [LuaDoc("Destroys a GameObject. The object is removed at the end of the current frame.")]
    [LuaParam("obj", "GameObject to destroy")]
    public void DestroyGameObject(GameObject obj)
    {
        if (obj != null) Object.Destroy(obj);
    }
}