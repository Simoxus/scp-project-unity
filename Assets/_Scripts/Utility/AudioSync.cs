using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class AudioSyncSettings
{
    [Header("Audio Analysis")]
    public float volumeThreshold = -27f;
    public float maxVolumeLevel = 100f;
    public float intensityMultiplier = 15f;

    [Header("Output Range")]
    public float minValue = 0.1f;
    public float maxValue = 0.2f;

    [Header("Smoothing")]
    public bool useSmoothing = false;
    public float smoothingSpeed = 5f;
}

[RequireComponent(typeof(StudioEventEmitter))]
public class AudioSync : MonoBehaviour
{
    [SerializeField] private AudioSyncSettings settings = new AudioSyncSettings();

    // Events that other scripts can subscribe to
    public event System.Action<float> OnAudioValueChanged;
    public event System.Action<float> OnRawAudioLevel;
    public event System.Action<float> OnNormalizedLevel;

    // Private audio components
    private StudioEventEmitter emitter;
    private EventInstance instance;
    private ChannelGroup channelGroup;
    private DSP dsp;
    private ChannelGroup masterChannelGroup;
    private DSP masterDSP;
    private bool useMasterChannel = false;

    // Current values
    private float currentValue = 0f;
    private float currentRawLevel = 0f;
    private float currentNormalizedLevel = 0f;

    // Public properties for easy access
    public float CurrentValue => currentValue;
    public float CurrentRawLevel => currentRawLevel;
    public float CurrentNormalizedLevel => currentNormalizedLevel;
    public AudioSyncSettings Settings => settings;

    void Awake()
    {
        emitter = GetComponent<StudioEventEmitter>();
    }

    async void Start()
    {
        if (!emitter.IsPlaying())
        {
            emitter.Play();
        }

        instance = emitter.EventInstance;
        await InitializeAudioMetering();
    }

    async UniTask InitializeAudioMetering()
    {
        await UniTask.WaitForSeconds(0.1f, ignoreTimeScale: false);

        bool success = TryInitializeEventDSP();
        if (!success)
        {
            success = TryInitializeMasterDSP();
        }
    }

    bool TryInitializeEventDSP()
    {
        RESULT result = instance.getChannelGroup(out channelGroup);
        if (result == RESULT.OK && channelGroup.hasHandle())
        {
            result = channelGroup.getDSP(0, out dsp);
            if (result == RESULT.OK && dsp.hasHandle())
            {
                result = dsp.setMeteringEnabled(true, true);
                if (result == RESULT.OK)
                {
                    useMasterChannel = false;
                    return true;
                }
            }
        }
        return false;
    }

    bool TryInitializeMasterDSP()
    {
        FMOD.System system = FMODUnity.RuntimeManager.CoreSystem;
        RESULT result = system.getMasterChannelGroup(out masterChannelGroup);

        if (result == RESULT.OK && masterChannelGroup.hasHandle())
        {
            result = masterChannelGroup.getDSP(0, out masterDSP);
            if (result == RESULT.OK && masterDSP.hasHandle())
            {
                result = masterDSP.setMeteringEnabled(true, true);
                if (result == RESULT.OK)
                {
                    useMasterChannel = true;
                    return true;
                }
            }
        }
        return false;
    }

    void Update()
    {
        float rms = 0f;
        bool gotAudio = false;

        if (useMasterChannel && masterDSP.hasHandle())
        {
            gotAudio = GetAudioLevel(masterDSP, out rms);
        }
        else if (!useMasterChannel && dsp.hasHandle())
        {
            gotAudio = GetAudioLevel(dsp, out rms);
        }

        if (gotAudio)
        {
            ProcessAudioLevel(rms);
        }
    }

    bool GetAudioLevel(DSP targetDSP, out float rms)
    {
        rms = 0f;
        RESULT result = targetDSP.getMeteringInfo(IntPtr.Zero, out DSP_METERING_INFO outputMetering);

        if (result == RESULT.OK && outputMetering.numchannels > 0)
        {
            // Get the loudest channel
            for (int i = 0; i < outputMetering.numchannels; i++)
            {
                if (outputMetering.rmslevel[i] > rms)
                {
                    rms = outputMetering.rmslevel[i];
                }
            }
            return true;
        }
        return false;
    }

    void ProcessAudioLevel(float rms)
    {
        currentRawLevel = rms;
        float db = LinearToDb(rms);

        // Trigger raw audio event
        OnRawAudioLevel?.Invoke(rms);

        if (db < settings.volumeThreshold)
        {
            SetProcessedValue(settings.minValue, 0f);
            return;
        }

        float normalized = Mathf.InverseLerp(settings.volumeThreshold, settings.maxVolumeLevel, db);
        normalized = Mathf.Clamp01(normalized * settings.intensityMultiplier);
        normalized = Mathf.Pow(normalized, 0.3f);

        currentNormalizedLevel = normalized;
        OnNormalizedLevel?.Invoke(normalized);

        float targetValue = Mathf.Lerp(settings.minValue, settings.maxValue, normalized);
        SetProcessedValue(targetValue, normalized);
    }

    void SetProcessedValue(float value, float normalizedLevel)
    {
        if (settings.useSmoothing)
        {
            currentValue = Mathf.Lerp(currentValue, value, Time.deltaTime * settings.smoothingSpeed);
        }
        else
        {
            currentValue = value;
        }

        // Trigger the main event that other scripts listen to
        OnAudioValueChanged?.Invoke(currentValue);
    }

    float LinearToDb(float linear)
    {
        if (linear <= 0.0001f) return -80f;
        return Mathf.Clamp(20f * Mathf.Log10(linear), -80f, 0f);
    }

    void OnDestroy()
    {
        if (dsp.hasHandle())
        {
            dsp.setMeteringEnabled(false, false);
        }
        if (masterDSP.hasHandle())
        {
            masterDSP.setMeteringEnabled(false, false);
        }
    }

    // Public utility methods
    public void UpdateSettings(AudioSyncSettings newSettings)
    {
        settings = newSettings;
    }

    public float GetMappedValue(float customMin, float customMax)
    {
        return Mathf.Lerp(customMin, customMax, currentNormalizedLevel);
    }

    public bool IsAudioActive()
    {
        return LinearToDb(currentRawLevel) > settings.volumeThreshold;
    }
}