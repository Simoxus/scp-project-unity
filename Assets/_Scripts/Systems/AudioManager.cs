using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("FMOD Pause Emitters")]
    public StudioEventEmitter[] excludedStudioEventEmitters; // This array contains which sounds you don't want to pause :)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ToggleSounds(bool doPause)
    {
        // Fetch all StudioEventEmitters in the scene
        StudioEventEmitter[] allEmitters = FindObjectsByType<StudioEventEmitter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var emitter in allEmitters)
        {
            if (excludedStudioEventEmitters.Any(e => e != null && e.gameObject == emitter.gameObject)) continue;

            var instance = emitter.EventInstance;

            if (instance.isValid())
            {
                instance.setPaused(doPause); // Pause or unpause the event
            }
        }
    }
}
