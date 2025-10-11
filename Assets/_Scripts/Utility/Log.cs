using UnityEngine;
using System.Runtime.CompilerServices;

public static class Log
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool VerboseEnabled { get; set; } = true;
#else
    public static bool VerboseEnabled { get; set; } = false;
#endif

    // Regular logging methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(object message)
    {
        Debug.Log(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(object message, Object context)
    {
        Debug.Log(message, context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(object message)
    {
        Debug.LogWarning(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(object message, Object context)
    {
        Debug.LogWarning(message, context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(object message)
    {
        Debug.LogError(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(object message, Object context)
    {
        Debug.LogError(message, context);
    }

    // Verbose logging methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseInfo(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"[VERBOSE] {message}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseInfo(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"[VERBOSE] {message}", context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseWarning(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"[VERBOSE] {message}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseWarning(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"[VERBOSE] {message}", context);
    }

    // Conditional logging methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InfoIf(bool condition, object message)
    {
        if (!condition) return;
        Debug.Log(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InfoIf(bool condition, object message, Object context)
    {
        if (!condition) return;
        Debug.Log(message, context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WarningIf(bool condition, object message)
    {
        if (!condition) return;
        Debug.LogWarning(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WarningIf(bool condition, object message, Object context)
    {
        if (!condition) return;
        Debug.LogWarning(message, context);
    }

    // Editor-only logging methods (which don't compile in normal builds)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Editor(object message)
    {
        Debug.Log($"[EDITOR] {message}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Editor(object message, Object context)
    {
        Debug.Log($"[EDITOR] {message}", context);
    }
}