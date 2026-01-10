using System.Runtime.CompilerServices;
using UnityEngine;

public static class Log
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool VerboseEnabled { get; set; } = true;
#else
    public static bool VerboseEnabled { get; set; } = false;
#endif

    #region Regular logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(object message)
    {
        Debug.Log(message.ToString().AsInfo(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(object message, Object context)
    {
        Debug.Log(message.ToString().AsInfo(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(object message)
    {
        Debug.LogWarning(message.ToString().AsWarning(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(object message, Object context)
    {
        Debug.LogWarning(message.ToString().AsWarning(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(object message)
    {
        Debug.LogError(message.ToString().AsError(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(object message, Object context)
    {
        Debug.LogError(message.ToString().AsError(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Success(object message)
    {
        Debug.Log(message.ToString().AsSuccess(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Success(object message, Object context)
    {
        Debug.Log(message.ToString().AsSuccess(consoleColors: true), context);
    }

    #endregion

    #region Verbose logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseInfo(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"[VERBOSE] {message}".ToString().AsInfo(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseInfo(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"[VERBOSE] {message}".ToString().AsInfo(verbose: true, consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseWarning(object message)
    {
        if (!VerboseEnabled) return;
        Debug.LogWarning($"[VERBOSE] {message}".AsWarning(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseWarning(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.LogWarning($"[VERBOSE] {message}".AsWarning(verbose: true, consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseSuccess(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log(message.ToString().AsSuccess(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseSuccess(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log(message.ToString().AsSuccess(verbose: true, consoleColors: true), context);
    }

    #endregion

    #region Conditional logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InfoIf(bool condition, object message)
    {
        if (!condition) return;
        Debug.Log(message.ToString().AsInfo(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InfoIf(bool condition, object message, Object context)
    {
        if (!condition) return;
        Debug.Log(message.ToString().AsInfo(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WarningIf(bool condition, object message)
    {
        if (!condition) return;
        Debug.LogWarning(message.ToString().AsWarning(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WarningIf(bool condition, object message, Object context)
    {
        if (!condition) return;
        Debug.LogWarning(message.ToString().AsWarning(consoleColors: true), context);
    }

    #endregion

    #region Editor logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Editor(object message)
    {
        Debug.Log($"[EDITOR] {message}".ToString().AsEditor());
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Editor(object message, Object context)
    {
        Debug.Log($"[EDITOR] {message}".ToString().AsEditor(), context);
    }

    #endregion
}