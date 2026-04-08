using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ModAPI("Tween", perMod: true)]
[MoonSharpUserData]
public class TweenAPI : IModAPICleanup
{
    private int _nextHandle = 1;
    private readonly Dictionary<int, Tween> _tweens = new Dictionary<int, Tween>();

    public void OnModUnloaded(string modId) => StopAll();

    private readonly string _modId;
    public TweenAPI(string modId) => _modId = modId;

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a Transform's local position to the target value. Returns a handle.")]
    [LuaParam("transform", "Transform to move")]
    [LuaParam("target", "Target local position")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int MoveLocal(Transform transform, Vector3 target, float duration, string ease, Closure onComplete = null)
    {
        var tween = Tween.LocalPosition(transform, target, duration, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a Transform's world position to the target value. Returns a handle.")]
    [LuaParam("transform", "Transform to move")]
    [LuaParam("target", "Target world position")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int MoveWorld(Transform transform, Vector3 target, float duration, string ease, Closure onComplete = null)
    {
        var tween = Tween.Position(transform, target, duration, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a Transform's local euler rotation to the target angles. Returns a handle.")]
    [LuaParam("transform", "Transform to rotate")]
    [LuaParam("target", "Target local euler angles")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int RotateLocal(Transform transform, Vector3 target, float duration, string ease, Closure onComplete = null)
    {
        var tween = Tween.LocalRotation(transform, Quaternion.Euler(target), duration, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a Transform's world euler rotation to the target angles. Returns a handle.")]
    [LuaParam("transform", "Transform to rotate")]
    [LuaParam("target", "Target world euler angles")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int RotateWorld(Transform transform, Vector3 target, float duration, string ease, Closure onComplete = null)
    {
        var tween = Tween.Rotation(transform, Quaternion.Euler(target), duration, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a Transform's local scale to the target value. Returns a handle.")]
    [LuaParam("transform", "Transform to scale")]
    [LuaParam("target", "Target local scale")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int Scale(Transform transform, Vector3 target, float duration, string ease, Closure onComplete = null)
    {
        var tween = Tween.Scale(transform, target, duration, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a Transform's local scale uniformly to the target float value. Returns a handle.")]
    [LuaParam("transform", "Transform to scale")]
    [LuaParam("target", "Target uniform scale")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int ScaleUniform(Transform transform, float target, float duration, string ease, Closure onComplete = null)
    {
        var tween = Tween.Scale(transform, Vector3.one * target, duration, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a CanvasGroup's alpha to the target value (0-1). Returns a handle.")]
    [LuaParam("canvasGroup", "CanvasGroup to fade")]
    [LuaParam("target", "Target alpha (0 = transparent, 1 = opaque)")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int FadeCanvasGroup(CanvasGroup canvasGroup, float target, float duration, string ease, Closure onComplete = null)
    {
        var tween = Tween.Alpha(canvasGroup, target, duration, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a Graphic's alpha to the target value (0-1). Returns a handle.")]
    [LuaParam("graphic", "Graphic component to fade")]
    [LuaParam("target", "Target alpha (0 = transparent, 1 = opaque)")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int FadeGraphic(Graphic graphic, float target, float duration, string ease, Closure onComplete = null)
    {
        var tween = Tween.Alpha(graphic, target, duration, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Tweens a float value from start to end over the duration. Use this for anything not covered by the other tween methods. Returns a handle.")]
    [LuaParam("from", "Start value")]
    [LuaParam("to", "End value")]
    [LuaParam("duration", "Duration in seconds")]
    [LuaParam("ease", "Easing type")]
    [LuaParam("onUpdate", "Function called every frame with the current float value")]
    [LuaParam("onComplete", "Optional callback fired when the tween finishes. Pass nil for none")]
    public int Float(float from, float to, float duration, string ease, Closure onUpdate, Closure onComplete = null)
    {
        var tween = Tween.Custom(from, to, duration, (value) =>
        {
            try
            {
                onUpdate.Call(value);
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex);
            }
        }, ParseEase(ease));
        tween.OnComplete(() => HandleComplete(onComplete));
        return Register(tween);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Stops a tween by handle. The tween is left at its current value.")]
    [LuaParam("handle", "Handle returned from a tween method")]
    public void Stop(int handle)
    {
        if (_tweens.TryGetValue(handle, out Tween tween))
        {
            tween.Stop();
            _tweens.Remove(handle);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Immediately completes a tween by handle, jumping to its end value.")]
    [LuaParam("handle", "Handle returned from a tween method")]
    public void Complete(int handle)
    {
        if (_tweens.TryGetValue(handle, out Tween tween))
        {
            tween.Complete();
            _tweens.Remove(handle);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Pauses a tween by handle.")]
    [LuaParam("handle", "Handle returned from a tween method")]
    public void Pause(int handle)
    {
        if (_tweens.TryGetValue(handle, out Tween tween))
        {
            tween.isPaused = true;
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Resumes a paused tween by handle.")]
    [LuaParam("handle", "Handle returned from a tween method")]
    public void Resume(int handle)
    {
        if (_tweens.TryGetValue(handle, out Tween tween))
        {
            tween.isPaused = false;
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the tween is still active and running.")]
    [LuaParam("handle", "Handle returned from a tween method")]
    public bool IsActive(int handle)
    {
        return _tweens.TryGetValue(handle, out Tween tween) && tween.isAlive;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Stops all tweens started by this mod.")]
    public void StopAll()
    {
        foreach (var tween in _tweens.Values)
        {
            if (tween.isAlive) tween.Stop();
        }
        _tweens.Clear();
    }

    private int Register(Tween tween)
    {
        int handle = _nextHandle++;
        _tweens[handle] = tween;
        return handle;
    }

    private static void HandleComplete(Closure onComplete)
    {
        if (onComplete == null) return;
        try
        {
            onComplete.Call();
        }
        catch (ScriptRuntimeException ex)
        {
            Log.Exception(ex);
        }
    }

    private static Ease ParseEase(string ease)
    {
        if (string.IsNullOrEmpty(ease)) return Ease.Default;
        if (Enum.TryParse<Ease>(ease, ignoreCase: true, out var result)) return result;
        return Ease.Default;
    }
}