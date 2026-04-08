using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;

[StaticModAPI("UniTask")]
[MoonSharpUserData]
public static class UniTaskAPI
{
    [MoonSharpVisible(true)]
    [LuaDoc("Waits for the given number of seconds before continuing. Must be awaited.")]
    [LuaParam("seconds", "Time to wait in seconds")]
    public static UniTask Delay(float seconds)
    {
        return UniTask.Delay(TimeSpan.FromSeconds(seconds));
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Waits for the given number of seconds before continuing. Must be awaited.")]
    [LuaParam("seconds", "Time to wait in seconds")]
    public static UniTask WaitForSeconds(float seconds)
    {
        return UniTask.WaitForSeconds(seconds);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Waits for the given number of milliseconds before continuing. Must be awaited.")]
    [LuaParam("ms", "Time to wait in milliseconds")]
    public static UniTask DelayMs(int ms)
    {
        return UniTask.Delay(ms);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Waits for a given number of frames before continuing. Must be awaited.")]
    [LuaParam("frames", "Number of frames to wait")]
    public static UniTask WaitForFrames(int frames)
    {
        return UniTask.DelayFrame(frames);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Yields execution until the next frame. Must be awaited.")]
    public static UniTask NextFrame()
    {
        return UniTask.NextFrame();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Yields execution until the end of the current frame. Must be awaited.")]
    public static async UniTask WaitForEndOfFrame()
    {
        await UniTask.WaitForEndOfFrame();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Yields execution once, allowing other coroutines to run. Must be awaited.")]
    public static async UniTask Yield()
    {
        await UniTask.Yield();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Waits until the given condition function returns true, checked once per frame. Must be awaited.")]
    [LuaParam("condition", "Function returning true when the wait should end")]
    public static UniTask WaitUntil(Closure condition)
    {
        return UniTask.WaitUntil(() =>
        {
            try
            {
                return condition.Call().CastToBool();
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex);
                return true;
            }
        });
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Waits while the given condition function returns true, checked once per frame. Must be awaited.")]
    [LuaParam("condition", "Function returning true while the wait should continue")]
    public static UniTask WaitWhile(Closure condition)
    {
        return UniTask.WaitWhile(() =>
        {
            try
            {
                return condition.Call().CastToBool();
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex);
                return false;
            }
        });
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Waits until the condition returns true, or the timeout elapses; whichever comes first. Returns true if the condition was met, false if timed out. Must be awaited.")]
    [LuaParam("condition", "Function returning true when the wait should end")]
    [LuaParam("timeoutSeconds", "Maximum time to wait in seconds")]
    public static async UniTask<bool> WaitUntilTimeout(Closure condition, float timeoutSeconds)
    {
        float elapsed = 0f;
        while (elapsed < timeoutSeconds)
        {
            try
            {
                if (condition.Call().CastToBool()) return true;
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex);
                return false;
            }

            await UniTask.NextFrame();
            elapsed += UnityEngine.Time.deltaTime;
        }
        return false;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Runs a callback after a delay without needing to await. Useful for timers.")]
    [LuaParam("seconds", "Delay in seconds")]
    [LuaParam("callback", "Function to call after the delay")]
    public static void RunAfterDelay(float seconds, Closure callback)
    {
        RunAfterDelayInternal(seconds, callback).Forget();
    }

    private static async UniTaskVoid RunAfterDelayInternal(float seconds, Closure callback)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(seconds));
        try
        {
            callback.Call();
        }
        catch (ScriptRuntimeException ex)
        {
            Log.Exception(ex);
        }
    }
}