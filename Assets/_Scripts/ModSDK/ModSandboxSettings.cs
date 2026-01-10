using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
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

    public static List<GlobalFunction> GetGlobalFunctions()
    {
        return new List<GlobalFunction>
        {
            // Time functions
            new GlobalFunction
            {
                Name = "getTime",
                Description = "Get the current game time in seconds",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getUnscaledTime",
                Description = "Get the current unscaled game time in seconds",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getDeltaTime",
                Description = "Get the time in seconds since the last frame",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getUnscaledDeltaTime",
                Description = "Get the unscaled time in seconds since the last frame",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getFrameCount",
                Description = "Get the total number of frames that have passed",
                Parameters = new List<(string, string)>(),
                ReturnType = "number"
            },
            
            // Logging functions
            new GlobalFunction
            {
                Name = "print",
                Description = "Print an info message to the console",
                Parameters = new List<(string, string)> { ("message", "string") },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "warn",
                Description = "Print a warning message to the console",
                Parameters = new List<(string, string)> { ("message", "string") },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "error",
                Description = "Print an error message to the console",
                Parameters = new List<(string, string)> { ("message", "string") },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "success",
                Description = "Print a success message to the console",
                Parameters = new List<(string, string)> { ("message", "string") },
                ReturnType = "void"
            }
        };
    }

    public static void ConfigureSandbox(Script script)
    {
        // Remove scary globals
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

        // Qol time functions
        script.Globals["getTime"] = (Func<float>)(() => Time.time);
        script.Globals["getUnscaledTime"] = (Func<float>)(() => Time.unscaledTime);
        script.Globals["getDeltaTime"] = (Func<float>)(() => Time.deltaTime);
        script.Globals["getUnscaledDeltaTime"] = (Func<float>)(() => Time.unscaledDeltaTime);
        script.Globals["getFrameCount"] = (Func<int>)(() => Time.frameCount);

        // Substitutes
        script.Globals["print"] = (Action<string>)((s) => Log.Info($"[MOD] {s}"));
        script.Globals["warn"] = (Action<string>)((s) => Log.Warning($"[MOD] {s}"));
        script.Globals["error"] = (Action<string>)((s) => Log.Error($"[MOD] {s}"));
        script.Globals["success"] = (Action<string>)((s) => Log.Success($"[MOD] {s}"));
    }
}