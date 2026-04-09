using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class GlobalFunction
{
    public string Name;
    public string Description;
    public List<(string name, string type, string description)> Parameters = new List<(string, string, string)>();
    public string ReturnType;
}

public static class ModSandboxSettings
{
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

    public static List<GlobalFunction> GetGlobalFunctions() => LuaModGlobals.GetAll();

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

        LuaModGlobals.Register(script, modName);

        foreach (var api in GetStaticAPIs())
        {
            UserData.RegisterType(api.Type);
            script.Globals[api.GlobalName] = UserData.CreateStatic(api.Type);
        }
    }
}