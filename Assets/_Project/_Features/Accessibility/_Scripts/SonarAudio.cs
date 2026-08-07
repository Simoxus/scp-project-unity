using UnityEngine;

/// <summary>
/// Procedural tone generation for the accessibility layer. Pure math, verified in
/// batchmode; A11yFmodAudio turns these samples into playable FMOD sounds at runtime.
/// </summary>
public static class SonarAudio
{
    public static float[] GenerateBeepSamples(float frequency, float duration, int sampleRate = 44100)
    {
        int sampleCount = Mathf.Max(2, (int)(duration * sampleRate));
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            // Half-sine envelope: smooth attack and release, avoids clicks at the clip edges
            float envelope = Mathf.Sin(Mathf.PI * i / (sampleCount - 1f));
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * envelope * 0.9f;
        }

        return samples;
    }

    /// <summary>Float samples [-1,1] to little-endian PCM16 bytes (FMOD OPENRAW format).</summary>
    public static byte[] SamplesToPcm16(float[] samples)
    {
        byte[] bytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
            bytes[i * 2] = (byte)(value & 0xFF);
            bytes[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }
        return bytes;
    }
}
