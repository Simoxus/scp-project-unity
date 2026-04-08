using FMODUnity;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

[ModAPI("Audio")]
[MoonSharpUserData]
public class AudioAPI
{
    private static EventReference ToRef(string path)
    {
        return RuntimeManager.PathToEventReference(path);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Plays a 2D sound with no world position.")]
    [LuaParam("eventPath", "FMOD event path")]
    public void PlayOneShot(string eventPath)
    {
        FMODHelper.PlayOneShot(ToRef(eventPath));
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Plays a 3D sound at a specific world position.")]
    [LuaParam("eventPath", "FMOD event path")]
    [LuaParam("position", "World position to emit the sound from")]
    public void PlayOneShot3D(string eventPath, Vector3 position)
    {
        FMODHelper.PlayOneShot3D(ToRef(eventPath), position);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Plays a managed sound attached to a GameObject, and returns a handle used to control it later.")]
    [LuaParam("eventPath", "FMOD event path")]
    [LuaParam("gameObject", "GameObject the sound will be attached to")]
    public int PlayAttached(string eventPath, GameObject gameObject)
    {
        return FMODHelper.PlayInstance(ToRef(eventPath), gameObject);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Stops a managed sound instance using its handle.")]
    [LuaParam("handle", "Handle returned from PlayAttached")]
    [LuaParam("allowFadeout", "If true, plays the FMOD stop transition. If false, cuts immediately")]
    public void Stop(int handle, bool allowFadeout = true)
    {
        FMODHelper.StopInstance(handle, allowFadeout);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the sound instance is still active and playing.")]
    [LuaParam("handle", "Handle returned from PlayAttached")]
    public bool IsPlaying(int handle)
    {
        return FMODHelper.IsInstanceValid(handle);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Updates the world position of a managed sound instance. Use this if you're moving a sound manually, rather than via an attached GameObject.")]
    [LuaParam("handle", "Handle returned from PlayAttached")]
    [LuaParam("position", "New world position for the sound")]
    public void UpdatePosition(int handle, Vector3 position)
    {
        FMODHelper.UpdateInstancePosition(handle, position);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Sets a local FMOD parameter on a specific sound instance.")]
    [LuaParam("handle", "Handle returned from PlayAttached")]
    [LuaParam("paramName", "Name of the FMOD parameter")]
    [LuaParam("value", "Value to set the parameter to")]
    public void SetParameter(int handle, string paramName, float value)
    {
        FMODHelper.SetInstanceParameter(handle, paramName, value);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Gets the current value of a local FMOD parameter on a specific sound instance.")]
    [LuaParam("handle", "Handle returned from PlayAttached")]
    [LuaParam("paramName", "Name of the FMOD parameter")]
    public float GetParameter(int handle, string paramName)
    {
        return FMODHelper.GetInstanceParameter(handle, paramName);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Sets a global FMOD parameter that affects all sounds listening to it.")]
    [LuaParam("paramName", "Name of the global FMOD parameter")]
    [LuaParam("value", "Value to set the parameter to")]
    public void SetGlobalParameter(string paramName, float value)
    {
        FMODHelper.SetGlobalParameter(paramName, value);
    }
}