using System.Runtime.CompilerServices;
using UnityEngine;

public static class Log
{
    // percentage
    private const int HEADER_SIZE = 110;
    private const int VERBOSE_HEADER_SIZE = 90;
    private const int VERBOSE_SIZE = 85;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool VerboseEnabled { get; set; } = true;
#else
    public static bool VerboseEnabled { get; set; } = false;
#endif

    #region Regular logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Header(object message)
    {
        Debug.Log($"<size={HEADER_SIZE}%><u>{message.ToString()}</size></u>".AsHeader(consoleColors: true));
    }

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
    public static void VerboseHeader(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_HEADER_SIZE}%><u>[VERBOSE] {message.ToString()}</size></u>".AsHeader(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseInfo(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%>[VERBOSE] {message}</size>".ToString().AsInfo(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseInfo(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%>[VERBOSE] {message}</size>".ToString().AsInfo(verbose: true, consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseWarning(object message)
    {
        if (!VerboseEnabled) return;
        Debug.LogWarning($"<size={VERBOSE_SIZE}%>[VERBOSE] {message}</size>".AsWarning(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseWarning(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.LogWarning($"<size={VERBOSE_SIZE}%>[VERBOSE] {message}</size>".AsWarning(verbose: true, consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseSuccess(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%>[VERBOSE] {message}</size>".AsSuccess(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseSuccess(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%>[VERBOSE] {message}</size>".AsSuccess(verbose: true, consoleColors: true), context);
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