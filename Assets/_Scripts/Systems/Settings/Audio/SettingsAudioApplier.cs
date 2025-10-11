using UnityEngine;

public class SettingsAudioApplier : MonoBehaviour
{
    public bool inBatchMode = false;

    [Header("References")]
    [SerializeField] private SettingsAudio settingsAudio;

    private void Awake()
    {
        if (settingsAudio == null)
        {
            settingsAudio = GetComponent<SettingsAudio>();
        }
    }

    private void Reset()
    {
        settingsAudio = GetComponent<SettingsAudio>();
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
}
