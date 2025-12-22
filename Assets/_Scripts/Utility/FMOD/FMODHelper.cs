using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class FMODHelper
{
    private class ManagedInstance
    {
        public EventInstance instance;
        public int occlusionId = -1;
    }

    private static readonly Dictionary<string, ManagedInstance> activeInstances = new();

    public static void PlayOneShot(EventReference fmodEvent)
    {
        RuntimeManager.PlayOneShot(fmodEvent);
    }

    public static void PlayOneShot3D(EventReference fmodEvent, Vector3 position)
    {
        RuntimeManager.PlayOneShot(fmodEvent, position);
    }

    public static void PlayOneShotWithParameters(EventReference fmodEvent, Vector3 position, params (string name, float value)[] parameters)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        foreach (var (name, value) in parameters)
        {
            instance.setParameterByName(name, value);
        }

        instance.start();
        instance.release();
    }

    public static void PlayOneShotWithOcclusion(EventReference fmodEvent, Vector3 position, int raysPerSound = -1, float raySpread = -1f, float maxDistance = -1f)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterSound(instance, position, raysPerSound, raySpread, maxDistance);
        }

        instance.start();
        instance.release();
    }

    public static void PlayOneShotWithDynamicOcclusion(EventReference fmodEvent, Vector3 position, float minDuration = 0.5f, int raysPerSound = -1, float raySpread = -1f, float maxDistance = -1f)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        int occlusionId = -1;
        if (AudioManager.Instance != null)
        {
            occlusionId = AudioManager.Instance.RegisterSound(instance, position, raysPerSound, maxDistance);
        }

        instance.start();

        MonitorAndCleanup(instance, occlusionId, minDuration).Forget();
    }

    public static void PlayOneShotWithParametersAndOcclusion(EventReference fmodEvent, Vector3 position, float minDuration = 0.5f, int raysPerSound = -1, float raySpread = -1f, float maxDistance = -1f, params (string name, float value)[] parameters)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        foreach (var (name, value) in parameters)
        {
            instance.setParameterByName(name, value);
        }

        int occlusionId = -1;
        if (AudioManager.Instance != null)
        {
            occlusionId = AudioManager.Instance.RegisterSound(instance, position, raysPerSound, maxDistance);
        }

        instance.start();

        MonitorAndCleanup(instance, occlusionId, minDuration).Forget();
    }

    public static void PlayInstance(EventReference fmodEvent, string key, Vector3 position)
    {
        if (activeInstances.TryGetValue(key, out var existing))
        {
            if (existing.occlusionId >= 0 && AudioManager.Instance != null)
            {
                AudioManager.Instance.UnregisterSound(existing.occlusionId);
            }

            existing.instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            existing.instance.release();
            activeInstances.Remove(key);
        }

        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.start();

        var managed = new ManagedInstance { instance = instance };
        activeInstances[key] = managed;
    }

    public static void PlayInstanceWithOcclusion(EventReference fmodEvent, string key, Vector3 position, int raysPerSound = -1, float raySpread = -1f, float maxDistance = -1f)
    {
        if (activeInstances.TryGetValue(key, out var existing))
        {
            if (existing.occlusionId >= 0 && AudioManager.Instance != null)
            {
                AudioManager.Instance.UnregisterSound(existing.occlusionId);
            }

            existing.instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            existing.instance.release();
            activeInstances.Remove(key);
        }

        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.start();

        var managed = new ManagedInstance { instance = instance };

        if (AudioManager.Instance != null)
        {
            managed.occlusionId = AudioManager.Instance.RegisterSound(instance, position, raysPerSound, maxDistance);
        }

        activeInstances[key] = managed;
    }

    public static void StopInstance(string key, bool allowFadeout = true)
    {
        if (activeInstances.TryGetValue(key, out var managed))
        {
            if (managed.occlusionId >= 0 && AudioManager.Instance != null)
            {
                AudioManager.Instance.UnregisterSound(managed.occlusionId);
            }

            managed.instance.stop(allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            managed.instance.release();
            activeInstances.Remove(key);

            return;
        }
    }

    public static void UpdateInstancePosition(string key, Vector3 position)
    {
        if (activeInstances.TryGetValue(key, out var managed))
        {
            managed.instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

            if (managed.occlusionId >= 0 && AudioManager.Instance != null)
            {
                AudioManager.Instance.UpdateSoundPosition(managed.occlusionId, position);
            }
        }
    }

    public static FMOD.Studio.EventInstance GetInstance(string key)
    {
        if (activeInstances.TryGetValue(key, out var managed))
        {
            return managed.instance;
        }

        return default;
    }

    public static void SetGlobalParameter(string parameterName, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
    }

    private static async UniTaskVoid MonitorAndCleanup(FMOD.Studio.EventInstance instance, int occlusionId, float minDuration)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(minDuration));

        while (instance.isValid())
        {
            instance.getPlaybackState(out var state);
            if (state == FMOD.Studio.PLAYBACK_STATE.STOPPED || state == FMOD.Studio.PLAYBACK_STATE.STOPPING)
                break;

            await UniTask.Yield();
        }

        // Cleanup
        if (occlusionId >= 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterSound(occlusionId);
        }

        if (instance.isValid())
        {
            instance.release();
        }
    }
}