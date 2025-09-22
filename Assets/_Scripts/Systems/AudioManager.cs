using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private Bus _gameplayBus;
    private Bus _uiBus;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _gameplayBus = RuntimeManager.GetBus("bus:/Gameplay");
        _uiBus = RuntimeManager.GetBus("bus:/UI");
    }

    public void ToggleSounds(bool doPause)
    {
        _gameplayBus.setPaused(doPause);
    }

    public void SetBusVolume(float volume, string busName)
    {
        Bus requestedBus = RuntimeManager.GetBus(busName);
        requestedBus.setVolume(volume);
    }
}
