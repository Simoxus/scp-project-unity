using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public static class FMODHelper
{
    private static readonly Dictionary<string, FMOD.Studio.EventInstance> activeInstances = new();

    public static void PlayOneShotWithParameters(EventReference fmodEvent, Vector3 position, params (string name, float value)[] parameters)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        foreach (var (name, value) in parameters)
        {
            instance.setParameterByName(name, value);
        }

        instance.start();
        instance.release(); // Fire-and-forget
    }

    public static void PlayOneShot(EventReference fmodEvent)
    {
        RuntimeManager.PlayOneShot(fmodEvent);
    }

    public static void PlayOneShot3D(EventReference fmodEvent, Vector3 position)
    {
        RuntimeManager.PlayOneShot(fmodEvent, position);
    }

    public static void PlayInstance(EventReference fmodEvent, string key, Vector3 position)
    {
        // Stop & release old instance if still running
        if (activeInstances.TryGetValue(key, out var existing))
        {
            existing.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            existing.release();
            activeInstances.Remove(key);
        }

        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.start();

        activeInstances[key] = instance;
    }

    public static void StopInstance(string key, bool allowFadeout = true)
    {
        if (activeInstances.TryGetValue(key, out var instance))
        {
            instance.stop(allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
            activeInstances.Remove(key);
        }
    }
}