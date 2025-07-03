using UnityEngine;
using FMODUnity;
using FMOD.Studio; // Important: Add this using statement

public static class FMODHelper
{
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

    public static void PlayOneShot3D(EventReference fmodEvent, Vector3 position)
    {
        RuntimeManager.PlayOneShot(fmodEvent, position);
    }
}