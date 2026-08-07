using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Minimal bridge to the official NVDA Controller Client (LGPL 2.1, NV Access).
/// The DLL lives in Assets/_Scripts/Accessibility/Plugins/x86_64/ and keeps its own license.
/// All failures degrade silently: no NVDA (or no DLL) simply means no speech output.
/// Signatures follow NV Access's official C# example (extras/controllerClient/examples).
/// </summary>
public static class ScreenReaderOutput
{
    private static bool _available;
    private static bool _checked;

    public static bool IsAvailable
    {
        get
        {
            if (!_checked) CheckAvailability();
            return _available;
        }
    }

    public static void Speak(string text, bool interrupt = false)
    {
        if (string.IsNullOrEmpty(text) || !IsAvailable) return;

        try
        {
            if (interrupt) nvdaController_cancelSpeech();
            nvdaController_speakText(text);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Accessibility] NVDA speech failed: {e.Message}");
            _available = false;
        }
    }

    private static void CheckAvailability()
    {
        _checked = true;
        try
        {
            // Returns 0 when NVDA is running and reachable
            _available = nvdaController_testIfRunning() == 0;
            if (!_available) Debug.Log("[Accessibility] NVDA is not running; screen reader output disabled.");
        }
        catch (DllNotFoundException)
        {
            _available = false;
            Debug.LogWarning("[Accessibility] nvdaControllerClient.dll not found; screen reader output disabled.");
        }
        catch (Exception e)
        {
            _available = false;
            Debug.LogWarning($"[Accessibility] NVDA availability check failed: {e.Message}");
        }
    }

    [DllImport("nvdaControllerClient", CharSet = CharSet.Unicode)]
    private static extern int nvdaController_speakText(string text);

    [DllImport("nvdaControllerClient")]
    private static extern int nvdaController_cancelSpeech();

    [DllImport("nvdaControllerClient")]
    private static extern int nvdaController_testIfRunning();
}
