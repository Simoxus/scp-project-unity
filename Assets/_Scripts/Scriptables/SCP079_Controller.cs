using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class SCP079_Controller : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    [SerializeField] private AudioSync audioSyncUtility;

    public int materialIndex = 0; // The index of the material to change
    public Material screenSpeaking;
    public Material screenPissed;

    void Start()
    {
        // Subscribe to audio events
        audioSyncUtility.OnAudioValueChanged += OnAudioChanged;
        _ = ChangeScreen();
    }

    void OnAudioChanged(float audioValue)
    {
        if (targetLight != null)
        {
            targetLight.intensity = audioValue;
        }
    }

    private async UniTaskVoid ChangeScreen()
    {
        await UniTask.WaitForSeconds(54f, ignoreTimeScale: false);
        Renderer renderer = GetComponent<Renderer>();

        Material[] mats = renderer.materials;
        mats[materialIndex] = screenPissed;
        renderer.materials = mats; // Apply the modified array back
    }

    void OnDestroy()
    {
        // Unsubscribe to audio events to prevent any memory leaks
        if (audioSyncUtility != null)
            audioSyncUtility.OnAudioValueChanged -= OnAudioChanged;
    }
}
