using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModGlobals
{
    public static List<GlobalFunction> GetAll()
    {
        return new List<GlobalFunction>
        {
            new GlobalFunction
            {
                Name = "OnAwake",
                Description = "Called once when the mod is first loaded, before OnStart.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "OnStart",
                Description = "Called once after OnAwake when the mod becomes active for the first time.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "OnEnable",
                Description = "Called each time the mod is enabled.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "OnDisable",
                Description = "Called each time the mod is disabled.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "OnDestroy",
                Description = "Called when the mod is unloaded. Clean up tweens, commands, and modified state here.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "OnUpdate",
                Description = "Called every frame.",
                Parameters = new List<(string, string, string)>
                {
                    ("deltaTime", "number", "Time in seconds since the last frame")
                },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "OnFixedUpdate",
                Description = "Called every physics step at a fixed interval, independent of frame rate.",
                Parameters = new List<(string, string, string)>
                {
                    ("fixedDeltaTime", "number", "Fixed physics timestep in seconds")
                },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "OnLateUpdate",
                Description = "Called every frame after all OnUpdate calls have finished. Useful for camera or follow logic.",
                Parameters = new List<(string, string, string)>
                {
                    ("deltaTime", "number", "Time in seconds since the last frame")
                },
                ReturnType = "void"
            },

            new GlobalFunction
            {
                Name = "wait",
                Description = "Runs a callback after a delay without blocking. Code after wait() continues immediately; use the callback for anything that should run after the delay.",
                Parameters = new List<(string, string, string)>
                {
                    ("seconds", "number", "Delay duration in seconds"),
                    ("callback", "function", "Called after the delay elapses")
                },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "sleep",
                Description = "Pauses a coroutine for the given number of seconds. Only valid inside a function started with Coroutine.start().",
                Parameters = new List<(string, string, string)>
                {
                    ("seconds", "number", "Time to pause in seconds")
                },
                ReturnType = "void"
            },

            new GlobalFunction
            {
                Name = "getTime",
                Description = "Returns the game time in seconds since the scene started. Affected by time scale.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getUnscaledTime",
                Description = "Returns the real time in seconds since the scene started. Not affected by time scale.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getDeltaTime",
                Description = "Returns the time in seconds since the last frame. Affected by time scale. Use in OnUpdate for frame-rate-independent movement.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getUnscaledDeltaTime",
                Description = "Returns the time in seconds since the last frame, unaffected by time scale. Use for UI or effects that should ignore slow-motion.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "number"
            },
            new GlobalFunction
            {
                Name = "getFrameCount",
                Description = "Returns the total number of frames that have elapsed since the game started.",
                Parameters = new List<(string, string, string)>(),
                ReturnType = "number"
            },

            new GlobalFunction
            {
                Name = "print",
                Description = "Prints an info message to the mod console.",
                Parameters = new List<(string, string, string)>
                {
                    ("message", "string", "Message to print")
                },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "warn",
                Description = "Prints a warning message to the mod console.",
                Parameters = new List<(string, string, string)>
                {
                    ("message", "string", "Message to print")
                },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "error",
                Description = "Prints an error message to the mod console.",
                Parameters = new List<(string, string, string)>
                {
                    ("message", "string", "Message to print")
                },
                ReturnType = "void"
            },
            new GlobalFunction
            {
                Name = "success",
                Description = "Prints a success message to the mod console.",
                Parameters = new List<(string, string, string)>
                {
                    ("message", "string", "Message to print")
                },
                ReturnType = "void"
            },

            new GlobalFunction
            {
                Name = "Vector3",
                Description = "Creates a new Vector3 with the given x, y, z components.",
                Parameters = new List<(string, string, string)>
                {
                    ("x", "number", "X component"),
                    ("y", "number", "Y component"),
                    ("z", "number", "Z component")
                },
                ReturnType = "Vector3"
            },
            new GlobalFunction
            {
                Name = "Vector2",
                Description = "Creates a new Vector2 with the given x, y components.",
                Parameters = new List<(string, string, string)>
                {
                    ("x", "number", "X component"),
                    ("y", "number", "Y component")
                },
                ReturnType = "Vector2"
            },
            new GlobalFunction
            {
                Name = "Color",
                Description = "Creates a new Color with the given RGBA components. All values are in the 0-1 range.",
                Parameters = new List<(string, string, string)>
                {
                    ("r", "number", "Red channel (0-1)"),
                    ("g", "number", "Green channel (0-1)"),
                    ("b", "number", "Blue channel (0-1)"),
                    ("a", "number", "Alpha channel (0-1)")
                },
                ReturnType = "Color"
            },
            new GlobalFunction
            {
                Name = "Quaternion",
                Description = "Creates a new Quaternion directly from its XYZW components. For rotation from angles, use Mathf or the Tween API instead.",
                Parameters = new List<(string, string, string)>
                {
                    ("x", "number", "X component"),
                    ("y", "number", "Y component"),
                    ("z", "number", "Z component"),
                    ("w", "number", "W component")
                },
                ReturnType = "Quaternion"
            },
        };
    }

    public static void Register(Script script, string modName)
    {
        script.Globals["Vector3"] = (Func<float, float, float, Vector3>)((x, y, z) => new Vector3(x, y, z));
        script.Globals["Vector2"] = (Func<float, float, Vector2>)((x, y) => new Vector2(x, y));
        script.Globals["Color"] = (Func<float, float, float, float, Color>)((r, g, b, a) => new Color(r, g, b, a));
        script.Globals["Quaternion"] = (Func<float, float, float, float, Quaternion>)((x, y, z, w) => new Quaternion(x, y, z, w));

        script.Globals["wait"] = (Action<float, Closure>)((seconds, cb) => CoroutineAPI.RunAfterDelayInternal(seconds, cb).Forget());
        script.Globals["sleep"] = (Func<float, DynValue>)((seconds) =>
        {
            var task = UniTask.Delay(TimeSpan.FromSeconds(seconds));
            return UserData.Create(task);
        });

        script.Globals["getTime"] = (Func<float>)(() => Time.time);
        script.Globals["getUnscaledTime"] = (Func<float>)(() => Time.unscaledTime);
        script.Globals["getDeltaTime"] = (Func<float>)(() => Time.deltaTime);
        script.Globals["getUnscaledDeltaTime"] = (Func<float>)(() => Time.unscaledDeltaTime);
        script.Globals["getFrameCount"] = (Func<int>)(() => Time.frameCount);

        string prefix = $"[MOD: {modName.ToUpper()}]";
        script.Globals["print"] = (Action<string>)((s) => Log.Info($"{prefix} {s}"));
        script.Globals["warn"] = (Action<string>)((s) => Log.Warning($"{prefix} {s}"));
        script.Globals["error"] = (Action<string>)((s) => Log.Error($"{prefix} {s}"));
        script.Globals["success"] = (Action<string>)((s) => Log.Success($"{prefix} {s}"));
    }
}