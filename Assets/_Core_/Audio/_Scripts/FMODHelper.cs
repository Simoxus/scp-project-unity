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

    private static readonly Dictionary<int, ManagedInstance> _activeInstances = new Dictionary<int, ManagedInstance>();
    private static int _nextInstanceHandle = 1;

    public static void PlayOneShot(EventReference fmodEvent)
    {
        RuntimeManager.PlayOneShot(fmodEvent);
    }

    public static void PlayOneShot3D(EventReference fmodEvent, Vector3 position, (string name, float value)[] parameters = null, bool useOcclusion = false, float occlusionMaxDistance = -1f, float occlusionMinDuration = 0.5f)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        if (parameters != null)
        {
            foreach (var (name, value) in parameters)
            {
                instance.setParameterByName(name, value);
            }
        }

        int occlusionId = -1;
        if (useOcclusion && Core.AudioManager != null)
        {
            occlusionId = Core.AudioManager.RegisterSound(instance, position, occlusionMaxDistance, useOcclusion: true);
        }

        instance.start();

        if (useOcclusion && occlusionId >= 0)
        {
            MonitorAndCleanup(instance, occlusionId, occlusionMinDuration).Forget();
        }
        else
        {
            instance.release();
        }
    }

    public static int PlayInstance(EventReference fmodEvent, GameObject gameObject, Rigidbody rigidbody = null, bool useOcclusion = false, float occlusionMaxDistance = -1f)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);

        if (rigidbody != null)
            RuntimeManager.AttachInstanceToGameObject(instance, gameObject, rigidbody);
        else
            RuntimeManager.AttachInstanceToGameObject(instance, gameObject);

        instance.start();

        var managed = new ManagedInstance { instance = instance };

        if (useOcclusion && Core.AudioManager != null)
        {
            managed.occlusionId = Core.AudioManager.RegisterSound(instance, gameObject.transform.position, occlusionMaxDistance, useOcclusion: true);
        }

        int handle = _nextInstanceHandle++;
        _activeInstances[handle] = managed;

        return handle;
    }

    public static void StopInstance(int handle, bool allowFadeout = true)
    {
        if (_activeInstances.TryGetValue(handle, out var managed))
        {
            if (managed.occlusionId >= 0 && Core.AudioManager != null)
            {
                Core.AudioManager.UnregisterSound(managed.occlusionId);
            }

            managed.instance.stop(allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            managed.instance.release();
            _activeInstances.Remove(handle);
        }
    }

    public static void UpdateInstancePosition(int handle, Vector3 position)
    {
        if (_activeInstances.TryGetValue(handle, out var managed))
        {
            if (managed.occlusionId >= 0 && Core.AudioManager != null)
            {
                Core.AudioManager.UpdateSoundPosition(managed.occlusionId, position);
            }
        }
    }

    public static EventInstance GetInstance(int handle)
    {
        if (_activeInstances.TryGetValue(handle, out var managed))
        {
            return managed.instance;
        }

        return default;
    }

    public static void SetInstanceParameter(int handle, string paramName, float value)
    {
        if (_activeInstances.TryGetValue(handle, out var managed) && managed.instance.isValid())
        {
            managed.instance.setParameterByName(paramName, value);
        }
    }

    public static float GetInstanceParameter(int handle, string paramName)
    {
        if (_activeInstances.TryGetValue(handle, out var managed) && managed.instance.isValid())
        {
            managed.instance.getParameterByName(paramName, out float value);
            return value;
        }
        return 0f;
    }

    public static EventReference PickRandomEvent(params EventReference[] fmodEvents)
    {
        if (fmodEvents == null || fmodEvents.Length == 0)
        {
            return default;
        }

        return fmodEvents[UnityEngine.Random.Range(0, fmodEvents.Length)];
    }

    public static bool IsInstanceValid(int handle)
    {
        return _activeInstances.TryGetValue(handle, out var managed) && managed.instance.isValid();
    }

    public static void SetGlobalParameter(string parameterName, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
    }

    public static void PlayOneShotWithSubtitle(EventReference fmodEvent, string tableName, string key, string speaker = null)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);

        if (Core.UI?.Subtitles != null)
        {
            Core.UI.Subtitles.ShowLocalizedSubtitleForSound(tableName, key, instance, speaker);
        }

        instance.start();
        instance.release();
    }

    public static void PlayOneShot3DWithSubtitle(EventReference fmodEvent, Vector3 position, string tableName, string key, string speaker = null)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        if (Core.UI?.Subtitles != null)
        {
            Core.UI.Subtitles.ShowLocalizedSubtitleForSound(tableName, key, instance, speaker);
        }

        instance.start();
        instance.release();
    }

    public static void PlayOneShotWithSubtitles(EventReference fmodEvent)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        FMODSubtitles.RegisterEvent(instance);
        instance.start();
        instance.release();
    }

    public static void PlayOneShot3DWithSubtitles(EventReference fmodEvent, Vector3 position)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        FMODSubtitles.RegisterEvent(instance);
        instance.start();
        instance.release();
    }

    public static int PlayInstanceWithSubtitle(EventReference fmodEvent, GameObject gameObject, string tableName, string key, string speaker = null, Rigidbody rigidbody = null, bool useOcclusion = false, float occlusionMaxDistance = -1f)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);

        if (rigidbody != null)
            RuntimeManager.AttachInstanceToGameObject(instance, gameObject, rigidbody);
        else
            RuntimeManager.AttachInstanceToGameObject(instance, gameObject);

        if (Core.UI?.Subtitles != null)
        {
            Core.UI.Subtitles.ShowLocalizedSubtitleForSound(tableName, key, instance, speaker);
        }

        instance.start();

        var managed = new ManagedInstance { instance = instance };

        if (useOcclusion && Core.AudioManager != null)
        {
            managed.occlusionId = Core.AudioManager.RegisterSound(instance, gameObject.transform.position, occlusionMaxDistance, useOcclusion: true);
        }

        int handle = _nextInstanceHandle++;
        _activeInstances[handle] = managed;

        return handle;
    }

    public static int PlayInstanceWithSubtitles(EventReference fmodEvent, GameObject gameObject, Rigidbody rigidbody = null, bool useOcclusion = false, float occlusionMaxDistance = -1f)
    {
        var instance = RuntimeManager.CreateInstance(fmodEvent);

        if (rigidbody != null)
            RuntimeManager.AttachInstanceToGameObject(instance, gameObject, rigidbody);
        else
            RuntimeManager.AttachInstanceToGameObject(instance, gameObject);

        FMODSubtitles.RegisterEvent(instance);

        instance.start();

        var managed = new ManagedInstance { instance = instance };

        if (useOcclusion && Core.AudioManager != null)
        {
            managed.occlusionId = Core.AudioManager.RegisterSound(instance, gameObject.transform.position, occlusionMaxDistance, useOcclusion: true);
        }

        int handle = _nextInstanceHandle++;
        _activeInstances[handle] = managed;

        return handle;
    }

    private static async UniTaskVoid MonitorAndCleanup(EventInstance instance, int occlusionId, float minDuration)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(minDuration));

        while (instance.isValid())
        {
            instance.getPlaybackState(out var state);
            if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
                break;

            await UniTask.Yield();
        }

        // Cleanup
        if (occlusionId >= 0 && Core.AudioManager != null)
        {
            Core.AudioManager.UnregisterSound(occlusionId);
        }

        if (instance.isValid())
        {
            instance.release();
        }
    }
}