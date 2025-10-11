using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private Bus _gameplayBus;
    private Bus _persistentBus;

    private VCA _masterVCA;
    private VCA _sfxVCA;
    private VCA _musicVCA;
    private VCA _voVCA;

    public void SetMasterVolume(float volume) => _masterVCA.setVolume(Mathf.Clamp01(volume));
    public void SetSFXVolume(float volume) => _sfxVCA.setVolume(Mathf.Clamp01(volume));
    public void SetMusicVolume(float volume) => _musicVCA.setVolume(Mathf.Clamp01(volume));
    public void SetVOVolume(float volume) => _voVCA.setVolume(Mathf.Clamp01(volume));

    public float GetMasterVolume() { _masterVCA.getVolume(out float v); return v; }
    public float GetSFXVolume()    { _sfxVCA.getVolume(out float v);    return v; }
    public float GetMusicVolume()  { _musicVCA.getVolume(out float v);  return v; }
    public float GetVOVolume() { _voVCA.getVolume(out float v); return v; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _gameplayBus = RuntimeManager.GetBus("bus:/Gameplay");
        _persistentBus = RuntimeManager.GetBus("bus:/Persistent");

        _masterVCA  = RuntimeManager.GetVCA("vca:/Master");
        _sfxVCA = RuntimeManager.GetVCA("vca:/SFX");
        _musicVCA = RuntimeManager.GetVCA("vca:/Music");
        _voVCA = RuntimeManager.GetVCA("vca:/VO");
    }

    public void ToggleGameSounds(bool doPause)
    {
        _gameplayBus.setPaused(doPause);
    }
}
