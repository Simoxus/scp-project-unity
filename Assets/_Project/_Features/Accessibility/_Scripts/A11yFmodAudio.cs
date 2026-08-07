using System;
using System.IO;
using System.Runtime.InteropServices;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Audio backend for the accessibility layer on top of FMOD's Core API.
/// The project disables Unity's built-in audio engine entirely (ProjectSettings:
/// m_DisableAudio, standard for FMOD games), so AudioSource is silent in builds —
/// all accessibility sounds must go through FMOD. The Core API needs no Studio banks,
/// which also keeps the layer working in test scenes where no banks are loaded.
/// Sounds come from StreamingAssets/A11y/*.ogg (CC0, see A11Y_AUDIO_CREDITS.md) with
/// procedurally generated PCM tones as fallback. Every failure degrades to silence.
/// </summary>
public static class A11yFmodAudio
{
    public struct A11ySound
    {
        public FMOD.Sound sound;
        public bool valid;
    }

    /// <summary>Load an .ogg from StreamingAssets/A11y; falls back to a generated tone.</summary>
    public static A11ySound LoadOrGenerate(string fileName, float fallbackFrequency, float fallbackDuration)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "A11y", fileName);
        if (File.Exists(path))
        {
            try
            {
                var result = RuntimeManager.CoreSystem.createSound(path, FMOD.MODE._3D | FMOD.MODE.CREATESAMPLE | FMOD.MODE.LOOP_OFF, out FMOD.Sound fileSound);
                if (result == FMOD.RESULT.OK)
                {
                    fileSound.set3DMinMaxDistance(1.5f, 60f);
                    return new A11ySound { sound = fileSound, valid = true };
                }
                Debug.LogWarning($"[Accessibility] FMOD could not load {fileName}: {result}. Falling back to generated tone.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Accessibility] FMOD file load failed for {fileName}: {e.Message}");
            }
        }

        return Generate(fallbackFrequency, fallbackDuration);
    }

    /// <summary>Create an FMOD sound from a procedurally generated tone (no assets needed).</summary>
    public static A11ySound Generate(float frequency, float duration, int sampleRate = 44100)
    {
        try
        {
            float[] samples = SonarAudio.GenerateBeepSamples(frequency, duration, sampleRate);
            byte[] pcm = SonarAudio.SamplesToPcm16(samples);

            var exinfo = new FMOD.CREATESOUNDEXINFO
            {
                cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO)),
                length = (uint)pcm.Length,
                numchannels = 1,
                defaultfrequency = sampleRate,
                format = FMOD.SOUND_FORMAT.PCM16
            };

            var result = RuntimeManager.CoreSystem.createSound(pcm,
                FMOD.MODE.OPENMEMORY | FMOD.MODE.OPENRAW | FMOD.MODE._3D | FMOD.MODE.LOOP_OFF,
                ref exinfo, out FMOD.Sound rawSound);

            if (result != FMOD.RESULT.OK)
            {
                Debug.LogWarning($"[Accessibility] FMOD createSound failed: {result}");
                return default;
            }

            rawSound.set3DMinMaxDistance(1.5f, 60f);
            return new A11ySound { sound = rawSound, valid = true };
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Accessibility] FMOD tone generation failed: {e.Message}");
            return default;
        }
    }

    /// <summary>Play a sound spatialized at a world position, optionally pitch-shifted
    /// (TLOU-style: pitch encodes the target's relative height).</summary>
    public static void PlayAt(A11ySound a11ySound, Vector3 position, float volume, float pitch = 1f)
    {
        if (!a11ySound.valid) return;
        try
        {
            RuntimeManager.CoreSystem.playSound(a11ySound.sound, default, true, out FMOD.Channel channel);
            FMOD.VECTOR pos = RuntimeUtils.ToFMODVector(position);
            FMOD.VECTOR vel = default;
            channel.set3DAttributes(ref pos, ref vel);
            channel.setVolume(Mathf.Clamp01(volume));
            if (!Mathf.Approximately(pitch, 1f)) channel.setPitch(pitch);
            channel.setPaused(false);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Accessibility] FMOD playback failed: {e.Message}");
        }
    }

    /// <summary>Play a sound flat (no spatialization) — for cues about the player's own body.</summary>
    public static void Play2D(A11ySound a11ySound, float volume)
    {
        if (!a11ySound.valid) return;
        try
        {
            RuntimeManager.CoreSystem.playSound(a11ySound.sound, default, true, out FMOD.Channel channel);
            channel.setMode(FMOD.MODE._2D);
            channel.setVolume(Mathf.Clamp01(volume));
            channel.setPaused(false);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Accessibility] FMOD 2D playback failed: {e.Message}");
        }
    }

    public static void Release(ref A11ySound a11ySound)
    {
        if (!a11ySound.valid) return;
        try { a11ySound.sound.release(); } catch { }
        a11ySound.valid = false;
    }
}
