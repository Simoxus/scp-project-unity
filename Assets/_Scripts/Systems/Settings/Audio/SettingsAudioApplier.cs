using UnityEngine;

public class SettingsAudioApplier : BaseSettingsApplier
{
    [Header("References")]
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
        if (AudioManager.Instance != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            AudioManager.Instance.SetMasterVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) { settingsAudio.SaveSettingsWithDelay(); }
    }

    public void ApplySoundVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            AudioManager.Instance.SetSFXVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) { settingsAudio.SaveSettingsWithDelay(); }
    }

    public void ApplyMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            AudioManager.Instance.SetMusicVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) { settingsAudio.SaveSettingsWithDelay(); }
    }

    public void ApplyVoiceoverVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            AudioManager.Instance.SetVOVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) { settingsAudio.SaveSettingsWithDelay(); }
    }

    public void ApplyInterfaceVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            float clampedVolumeValue = Mathf.Clamp(value, 0.0f, 1.0f);
            AudioManager.Instance.SetUIVolume(clampedVolumeValue);
        }

        if (inBatchMode == false) { settingsAudio.SaveSettingsWithDelay(); }
    }

    public void ApplyRealtimeOcclusion(bool enabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetOcclusionEnabled(enabled);
        }

        if (inBatchMode == false) { settingsAudio.SaveSettings(); }
    }
}
