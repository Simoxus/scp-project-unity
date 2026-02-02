using UnityEngine;
using UnityEngine.UI;

public class SettingsAudio : BaseSettings
{
    public override string CATEGORY => "Audio";

    [Space]
    public SettingsAudioApplier applier;

    [Header("UI Elements")]
    public Slider masterVolumeSlider;
    public Slider soundVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider voiceoverVolumeSlider;
    public Slider interfaceVolumeSlider;
    public Toggle realtimeOcclusionToggle;

    protected override void InitializeReferences()
    {
        if (applier == null)
        {
            applier = GetComponent<SettingsAudioApplier>();
        }
    }

    public override void SaveSettings()
    {
        SettingsManager settingsManager = Core.SettingsManager;
        if (settingsManager == null) return;

        settingsManager.SaveFloat(CATEGORY, "MasterVolume", masterVolumeSlider.value);
        settingsManager.SaveFloat(CATEGORY, "SoundVolume", soundVolumeSlider.value);
        settingsManager.SaveFloat(CATEGORY, "MusicVolume", musicVolumeSlider.value);
        settingsManager.SaveFloat(CATEGORY, "VoiceoverVolume", voiceoverVolumeSlider.value);
        settingsManager.SaveFloat(CATEGORY, "InterfaceVolume", interfaceVolumeSlider.value);
        settingsManager.SaveBool(CATEGORY, "RealtimeOcclusion", realtimeOcclusionToggle.isOn);

        settingsManager.Save();
    }

    public override void LoadSettings()
    {
        SettingsManager settingsManager = Core.SettingsManager;
        if (settingsManager == null) return;

        applier.inBatchMode = true;

        masterVolumeSlider.value = settingsManager.LoadFloat(CATEGORY, "MasterVolume", 1f);
        soundVolumeSlider.value = settingsManager.LoadFloat(CATEGORY, "SoundVolume", 1f);
        musicVolumeSlider.value = settingsManager.LoadFloat(CATEGORY, "MusicVolume", 1f);
        voiceoverVolumeSlider.value = settingsManager.LoadFloat(CATEGORY, "VoiceoverVolume", 1f);
        interfaceVolumeSlider.value = settingsManager.LoadFloat(CATEGORY, "InterfaceVolume", 1f);
        realtimeOcclusionToggle.isOn = settingsManager.LoadBool(CATEGORY, "RealtimeOcclusion", true);

        applier.ApplyMasterVolume(masterVolumeSlider.value);
        applier.ApplySoundVolume(soundVolumeSlider.value);
        applier.ApplyMusicVolume(musicVolumeSlider.value);
        applier.ApplyVoiceoverVolume(voiceoverVolumeSlider.value);
        applier.ApplyInterfaceVolume(interfaceVolumeSlider.value);
        applier.ApplyRealtimeOcclusion(realtimeOcclusionToggle.isOn);

        applier.inBatchMode = false;
    }
}