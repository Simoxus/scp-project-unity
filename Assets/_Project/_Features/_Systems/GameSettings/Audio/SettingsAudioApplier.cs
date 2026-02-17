using UnityEngine;

public class SettingsAudioApplier : BaseSettingsApplier
{
    [SerializeField] private SettingsAudio settingsAudio;

    protected override void InitializeReferences()
    {
        if (settingsAudio == null)
        {
            settingsAudio = GetComponent<SettingsAudio>();
        }
    }

    public void ApplyMasterVolume(float value)
    {
        if (Core.AudioManager != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            Core.AudioManager.SetMasterVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) settingsAudio.SaveSettingsWithDelay();
    }

    public void ApplySoundVolume(float value)
    {
        if (Core.AudioManager != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            Core.AudioManager.SetSFXVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) settingsAudio.SaveSettingsWithDelay();
    }

    public void ApplyMusicVolume(float value)
    {
        if (Core.AudioManager != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            Core.AudioManager.SetMusicVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) settingsAudio.SaveSettingsWithDelay();
    }

    public void ApplyVoiceoverVolume(float value)
    {
        if (Core.AudioManager != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            Core.AudioManager.SetVOVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) settingsAudio.SaveSettingsWithDelay();
    }

    public void ApplyInterfaceVolume(float value)
    {
        if (Core.AudioManager != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            Core.AudioManager.SetUIVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) settingsAudio.SaveSettingsWithDelay();
    }

    public void ApplyRealtimeOcclusion(bool enabled)
    {
        if (Core.AudioManager != null)
        {
            Core.AudioManager.SetOcclusionEnabled(enabled);
        }

        if (inBatchMode == false) settingsAudio.SaveSettings();
    }

    public void ApplyShowSubtitles(bool enabled)
    {
        if (Core.UI.Subtitles != null)
        {
            Core.UI.Subtitles.SetSubtitlesEnabled(
                enabled,
                clearExisting: !enabled
            );
        }

        if (inBatchMode == false) settingsAudio.SaveSettings();
    }
}
