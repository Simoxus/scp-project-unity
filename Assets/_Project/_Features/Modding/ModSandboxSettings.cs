using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class ModSandboxSettings
{
    public class GlobalFunction
    {
        public string Name;
        public string Description;
        public List<(string name, string type)> Parameters = new List<(string, string)>();
        public string ReturnType;
    }

    public class StaticAPI
    {
        public string GlobalName;
        public Type Type;
    }

    private static readonly List<StaticAPI> _unityBuiltinAPIs = new List<StaticAPI>
    {
        new StaticAPI { GlobalName = "Mathf",   Type = typeof(Mathf) },
        new StaticAPI { GlobalName = "Physics",  Type = typeof(Physics) },
        new StaticAPI { GlobalName = "Input",    Type = typeof(Input) },
        new StaticAPI { GlobalName = "Screen",   Type = typeof(Screen) },
        new StaticAPI { GlobalName = "Random",   Type = typeof(UnityEngine.Random) },
    };

    public static List<StaticAPI> GetStaticAPIs()
    {
        var discovered = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
            .Where(t => t.GetCustomAttribute<StaticModAPIAttribute>() != null)
            .Select(t => new StaticAPI
            {
                GlobalName = t.GetCustomAttribute<StaticModAPIAttribute>().GlobalName,
                Type = t
            });

        return _unityBuiltinAPIs.Concat(discovered).ToList();
    }

    public static List<GlobalFunction> GetGlobalFunctions()
    {
        return new List<GlobalFunction>
        {
            new GlobalFunction
            {
                Name = "getTime",
                Description = "Gets the current game time in seconds.",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getUnscaledTime",
                Description = "Gets the current unscaled game time in seconds.",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getDeltaTime",
                Description = "Gets the time in seconds since the last frame.",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getUnscaledDeltaTime",
                Description = "Gets the unscaled time in seconds since the last frame.",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getFrameCount",
                Description = "Gets the total number of frames that have passed.",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "print",
                Description = "Prints an info message to the console.",
                Parameters = new List<(string, string)> { ("message", "string") },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "warn",
                Description = "Prints a warning message to the console.",
                Parameters = new List<(string, string)> { ("message", "string") },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "error",
                Description = "Print an error message to the console.",
                Parameters = new List<(string, string)> { ("message", "string") },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "success",
                Description = "Prints a success message to the console.",
                Parameters = new List<(string, string)> { ("message", "string") },
                ReturnType = "void"
            }
        };
    }

    public static void ConfigureSandbox(Script script, string modName)
    {
        script.Globals["io"] = DynValue.Nil;
        script.Globals["os"] = DynValue.Nil;
        script.Globals["dofile"] = DynValue.Nil;
        script.Globals["loadfile"] = DynValue.Nil;
        script.Globals["require"] = DynValue.Nil;
        script.Globals["package"] = DynValue.Nil;
        script.Globals["module"] = DynValue.Nil;
        script.Globals["load"] = DynValue.Nil;
        script.Globals["loadstring"] = DynValue.Nil;
        script.Globals["debug"] = DynValue.Nil;

        script.Globals["Vector3"] = (Func<float, float, float, Vector3>)((x, y, z) => new Vector3(x, y, z));
        script.Globals["Vector2"] = (Func<float, float, Vector2>)((x, y) => new Vector2(x, y));
        script.Globals["Color"] = (Func<float, float, float, float, Color>)((r, g, b, a) => new Color(r, g, b, a));
        script.Globals["Quaternion"] = (Func<float, float, float, float, Quaternion>)((x, y, z, w) => new Quaternion(x, y, z, w));

        script.Globals["getTime"] = (Func<float>)(() => Time.time);
        script.Globals["getUnscaledTime"] = (Func<float>)(() => Time.unscaledTime);
        script.Globals["getDeltaTime"] = (Func<float>)(() => Time.deltaTime);
        script.Globals["getUnscaledDeltaTime"] = (Func<float>)(() => Time.unscaledDeltaTime);
        script.Globals["getFrameCount"] = (Func<int>)(() => Time.frameCount);

        string prefix = $"[MOD: {modName}]";
        script.Globals["print"] = (Action<string>)((s) => Log.Info($"{prefix} {s}"));
        script.Globals["warn"] = (Action<string>)((s) => Log.Warning($"{prefix} {s}"));
        script.Globals["error"] = (Action<string>)((s) => Log.Error($"{prefix} {s}"));
        script.Globals["success"] = (Action<string>)((s) => Log.Success($"{prefix} {s}"));

        foreach (var api in GetStaticAPIs())
        {
            UserData.RegisterType(api.Type);
            script.Globals[api.GlobalName] = UserData.CreateStatic(api.Type);
        }
    }
}