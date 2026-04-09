using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections.Generic;
using UnityEngine;

using Coroutine = MoonSharp.Interpreter.Coroutine;

[ModAPI("Coroutine", perMod: true)]
[MoonSharpUserData]
public class CoroutineAPI : IModAPICleanup
{
    private readonly Script _script;
    private readonly List<Coroutine> _active = new List<Coroutine>();

    public CoroutineAPI(string modId, Script script)
    {
        _script = script;
    }

    public void OnModUnloaded(string modId) => _active.Clear();

    [MoonSharpVisible(true)]
    [LuaDoc("Starts a coroutine from the given function. Inside the function, use sleep(), waitUntil(), waitWhile(), and nextFrame() to pause execution without blocking.")]
    [LuaParam("function_", "Function to run as a coroutine")]
    public void Start(Closure function_)
    {
        var co = _script.CreateCoroutine(function_);
        _active.Add(co.Coroutine);
        Run(co.Coroutine).Forget();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Pauses the coroutine for the given number of seconds. Only valid inside a coroutine started with Coroutine.start().")]
    [LuaParam("seconds", "Time to pause in seconds")]
    [LuaReturn("void", "Yield this value with coroutine.yield()")]
    public DynValue Sleep(float seconds)
    {
        return DynValue.FromObject(_script, UniTask.Delay(TimeSpan.FromSeconds(seconds)));
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Pauses the coroutine until the condition returns true, checked once per frame. Only valid inside a coroutine started with Coroutine.start().")]
    [LuaParam("condition", "Function returning true when the coroutine should resume")]
    public DynValue WaitUntil(Closure condition)
    {
        return DynValue.FromObject(_script, UniTask.WaitUntil(() =>
        {
            try { return condition.Call().CastToBool(); }
            catch (ScriptRuntimeException ex) { Log.Exception(ex); return true; }
        }));
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Pauses the coroutine while the condition returns true, checked once per frame. Only valid inside a coroutine started with Coroutine.start().")]
    [LuaParam("condition", "Function returning true while the coroutine should remain paused")]
    public DynValue WaitWhile(Closure condition)
    {
        return DynValue.FromObject(_script, UniTask.WaitWhile(() =>
        {
            try { return condition.Call().CastToBool(); }
            catch (ScriptRuntimeException ex) { Log.Exception(ex); return false; }
        }));
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Pauses the coroutine for exactly one frame. Only valid inside a coroutine started with Coroutine.start().")]
    public DynValue NextFrame()
    {
        return DynValue.FromObject(_script, UniTask.NextFrame());
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Pauses the coroutine for the given number of frames. Only valid inside a coroutine started with Coroutine.start().")]
    [LuaParam("frames", "Number of frames to wait")]
    public DynValue WaitFrames(int frames)
    {
        return DynValue.FromObject(_script, UniTask.DelayFrame(frames));
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Pauses the coroutine until the condition returns true, or the timeout elapses. Returns true if the condition was met, false if it timed out. Only valid inside a coroutine started with Coroutine.start().")]
    [LuaParam("condition", "Function returning true when the coroutine should resume")]
    [LuaParam("timeoutSeconds", "Maximum time to wait in seconds")]
    public DynValue WaitUntilTimeout(Closure condition, float timeoutSeconds)
    {
        return DynValue.FromObject(_script, WaitUntilTimeoutInternal(condition, timeoutSeconds));
    }

    private async UniTask<bool> WaitUntilTimeoutInternal(Closure condition, float timeoutSeconds)
    {
        float elapsed = 0f;
        while (elapsed < timeoutSeconds)
        {
            try { if (condition.Call().CastToBool()) return true; }
            catch (ScriptRuntimeException ex) { Log.Exception(ex); return false; }
            await UniTask.NextFrame();
            elapsed += Time.deltaTime;
        }
        return false;
    }

    private async UniTaskVoid Run(Coroutine co)
    {
        try
        {
            DynValue result = co.Resume();

            while (co.State != CoroutineState.Dead)
            {
                if (result.Type == DataType.UserData && result.UserData.Object is UniTask task)
                    await task;
                else if (result.Type == DataType.UserData && result.UserData.Object is UniTask<bool> taskBool)
                    result = DynValue.NewBoolean(await taskBool);
                else if (result.Type == DataType.Number)
                    await UniTask.Delay(TimeSpan.FromSeconds(result.Number));
                else
                    await UniTask.NextFrame();

                if (co.State == CoroutineState.Dead) break;

                // pass awaited result back in if it was a value task
                result = result.Type == DataType.Boolean
                    ? co.Resume(result)
                    : co.Resume();
            }
        }
        catch (ScriptRuntimeException ex)
        {
            Log.Exception(ex, message: ex.DecoratedMessage);
        }
        finally
        {
            _active.Remove(co);
        }
    }

    public static async UniTaskVoid RunAfterDelayInternal(float seconds, Closure cb)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(seconds));
        try { cb.Call(); }
        catch (ScriptRuntimeException ex) { Log.Exception(ex); }
    }
}