using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public static class LightFlickerPresets
{
    public static async UniTaskVoid Flicker(this RoomLight light, float duration, float intensityMin, float intensityMax, int flickerCount)
    {
        if (!light.CanFlicker) return;
        await light.FlickerAsync(duration, intensityMin, intensityMax, flickerCount);
    }

    public static async UniTask FlickerAsync(this RoomLight light, float duration, float intensityMin, float intensityMax, int flickerCount)
    {
        if (!light.CanFlicker) return;

        light.StopFlicker();
        CancellationTokenSource cts = light.GetFlickerCts();

        try
        {
            float timeBetweenFlickers = duration / flickerCount;

            for (int i = 0; i < flickerCount; i++)
            {
                if (cts.Token.IsCancellationRequested) break;

                float targetIntensity = Random.Range(intensityMin * light.OriginalIntensity, intensityMax * light.OriginalIntensity);
                await light.TweenIntensityAsync(targetIntensity, timeBetweenFlickers * 0.5f, cts.Token);
            }

            await light.TweenIntensityAsync(light.OriginalIntensity, timeBetweenFlickers * 0.5f, cts.Token);
        }
        catch (System.OperationCanceledException)
        {
            light.SetIntensity(light.OriginalIntensity);
        }
    }

    public static async UniTaskVoid HorrorPattern(this RoomLight light)
    {
        if (!light.CanFlicker) return;

        light.StopFlicker();
        CancellationTokenSource cts = light.GetFlickerCts();

        try
        {
            await light.SetIntensityInstant(0f);
            await UniTask.WaitForSeconds(0.05f, cancellationToken: cts.Token);

            await light.SetIntensityInstant(light.OriginalIntensity);
            await UniTask.WaitForSeconds(0.1f, cancellationToken: cts.Token);

            await light.SetIntensityInstant(0f);
            await UniTask.WaitForSeconds(0.08f, cancellationToken: cts.Token);

            await light.SetIntensityInstant(light.OriginalIntensity * 0.7f);
            await UniTask.WaitForSeconds(0.15f, cancellationToken: cts.Token);

            await light.SetIntensityInstant(light.OriginalIntensity);
        }
        catch (System.OperationCanceledException)
        {
            light.SetIntensity(light.OriginalIntensity);
        }
    }

    public static async UniTaskVoid Pulse(this RoomLight light, float duration = 2f, float intensityMin = 0.3f, float intensityMax = 1f, int pulseCount = 3)
    {
        if (!light.CanFlicker) return;

        light.StopFlicker();
        CancellationTokenSource cts = light.GetFlickerCts();

        try
        {
            float pulseDuration = duration / pulseCount;

            for (int i = 0; i < pulseCount; i++)
            {
                if (cts.Token.IsCancellationRequested) break;

                await light.TweenIntensityAsync(intensityMin * light.OriginalIntensity, pulseDuration * 0.5f, cts.Token);
                await light.TweenIntensityAsync(intensityMax * light.OriginalIntensity, pulseDuration * 0.5f, cts.Token);
            }

            await light.TweenIntensityAsync(light.OriginalIntensity, pulseDuration * 0.25f, cts.Token);
        }
        catch (System.OperationCanceledException)
        {
            light.SetIntensity(light.OriginalIntensity);
        }
    }

    public static async UniTaskVoid Stutter(this RoomLight light, float duration = 0.5f, int stutterCount = 10)
    {
        if (!light.CanFlicker) return;

        light.StopFlicker();
        CancellationTokenSource cts = light.GetFlickerCts();

        try
        {
            float delayTime = duration / stutterCount;

            for (int i = 0; i < stutterCount; i++)
            {
                if (cts.Token.IsCancellationRequested) break;

                light.SetIntensity(i % 2 == 0 ? 0f : light.OriginalIntensity);
                await UniTask.WaitForSeconds(delayTime, cancellationToken: cts.Token);
            }

            light.SetIntensity(light.OriginalIntensity);
        }
        catch (System.OperationCanceledException)
        {
            light.SetIntensity(light.OriginalIntensity);
        }
    }

    public static async UniTaskVoid FadeOut(this RoomLight light, float fadeOutDuration = 1f, float stayOffDuration = 0.5f, float fadeInDuration = 1f)
    {
        if (!light.CanFlicker) return;

        light.StopFlicker();
        CancellationTokenSource cts = light.GetFlickerCts();

        try
        {
            await light.TweenIntensityAsync(0f, fadeOutDuration, cts.Token);
            await UniTask.WaitForSeconds(stayOffDuration, cancellationToken: cts.Token);
            await light.TweenIntensityAsync(light.OriginalIntensity, fadeInDuration, cts.Token);
        }
        catch (System.OperationCanceledException)
        {
            light.SetIntensity(light.OriginalIntensity);
        }
    }

    public static async UniTaskVoid Strobe(this RoomLight light, float duration = 1f, float onTime = 0.05f, float offTime = 0.05f)
    {
        if (!light.CanFlicker) return;

        light.StopFlicker();
        CancellationTokenSource cts = light.GetFlickerCts();

        try
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (cts.Token.IsCancellationRequested) break;

                light.SetIntensity(light.OriginalIntensity);
                await UniTask.WaitForSeconds(onTime, cancellationToken: cts.Token);

                light.SetIntensity(0f);
                await UniTask.WaitForSeconds(offTime, cancellationToken: cts.Token);

                elapsed += onTime + offTime;
            }

            light.SetIntensity(light.OriginalIntensity);
        }
        catch (System.OperationCanceledException)
        {
            light.SetIntensity(light.OriginalIntensity);
        }
    }

    public static async UniTaskVoid Flare(this RoomLight light, float peakIntensityMultiplier = 3f, float riseTime = 0.2f, float fallTime = 0.8f)
    {
        if (!light.CanFlicker) return;

        light.StopFlicker();
        CancellationTokenSource cts = light.GetFlickerCts();

        try
        {
            await light.TweenIntensityAsync(light.OriginalIntensity * peakIntensityMultiplier, riseTime, cts.Token);
            await light.TweenIntensityAsync(light.OriginalIntensity, fallTime, cts.Token);
        }
        catch (System.OperationCanceledException)
        {
            light.SetIntensity(light.OriginalIntensity);
        }
    }

    public static async UniTaskVoid PowerFailure(this RoomLight light, float dimIntensity = 0.05f, float failureDuration = 1.5f)
    {
        if (!light.CanFlicker) return;

        light.StopFlicker();
        CancellationTokenSource cts = light.GetFlickerCts();

        try
        {
            await light.TweenIntensityAsync(dimIntensity * light.OriginalIntensity, 0.1f, cts.Token);
            await UniTask.WaitForSeconds(failureDuration, cancellationToken: cts.Token);

            await light.SetIntensityInstant(0f);
            await UniTask.WaitForSeconds(0.05f, cancellationToken: cts.Token);

            await light.SetIntensityInstant(light.OriginalIntensity * 0.3f);
            await UniTask.WaitForSeconds(0.08f, cancellationToken: cts.Token);

            await light.SetIntensityInstant(0f);
            await UniTask.WaitForSeconds(0.04f, cancellationToken: cts.Token);

            await light.TweenIntensityAsync(light.OriginalIntensity, 0.4f, cts.Token);
        }
        catch (System.OperationCanceledException)
        {
            light.SetIntensity(light.OriginalIntensity);
        }
    }
}