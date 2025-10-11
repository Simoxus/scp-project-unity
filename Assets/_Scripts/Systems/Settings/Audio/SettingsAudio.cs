using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SettingsAudio : MonoBehaviour
{
    public const string CATEGORY = "Audio";

    [Header("References")]
    public SettingsAudioApplier applier;

    public Slider masterVolumeSlider;
    public Slider soundVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider voiceoverVolumeSlider;

    private bool _isWaitingToSave = false;

    private void Start()
    {
        if (applier == null)
        {
            applier = GetComponent<SettingsAudioApplier>();
        }

        LoadSettings();
    }

    public void SaveSettings()
    {
        var sm = SettingsManager.Instance;

        sm.SaveFloat(CATEGORY, "MasterVolume", masterVolumeSlider.value);
        sm.SaveFloat(CATEGORY, "SoundVolume", soundVolumeSlider.value);
        sm.SaveFloat(CATEGORY, "MusicVolume", musicVolumeSlider.value);
        sm.SaveFloat(CATEGORY, "VoiceoverVolume", voiceoverVolumeSlider.value);

        sm.Save();
    }

    public void LoadSettings()
    {
        var sm = SettingsManager.Instance;

        masterVolumeSlider.value = sm.LoadFloat(CATEGORY, "MasterVolume", 1f);
        soundVolumeSlider.value = sm.LoadFloat(CATEGORY, "SoundVolume", 1f);
        musicVolumeSlider.value = sm.LoadFloat(CATEGORY, "MusicVolume", 1f);
        voiceoverVolumeSlider.value = sm.LoadFloat(CATEGORY, "VoiceoverVolume", 1f);

        applier.inBatchMode = true;

        applier.ApplyMasterVolume(masterVolumeSlider.value);
        applier.ApplySoundVolume(soundVolumeSlider.value);
        applier.ApplyMusicVolume(musicVolumeSlider.value);
        applier.ApplyVoiceoverVolume(voiceoverVolumeSlider.value);

        applier.inBatchMode = false;

        SaveSettings();
    }

    public void ResetCategorySettings()
    {
        if (SettingsManager.Instance == null) return;

        SettingsManager.Instance.ResetCategory(CATEGORY);

        LoadSettings();
        SaveSettings();
    }

    public async void SaveSettingsWithDelay(float delay = 0.5f)
    {
        if (_isWaitingToSave) { return; }

        _isWaitingToSave = true;

        float elapsedTime = 0f;
        while (elapsedTime < delay)
        {
            await UniTask.Yield();
            elapsedTime += Time.unscaledDeltaTime;

            // If another call comes in, reset the saving timer
            if (!_isWaitingToSave) { return; }
        }

        SaveSettings();
        _isWaitingToSave = false;
    }
}