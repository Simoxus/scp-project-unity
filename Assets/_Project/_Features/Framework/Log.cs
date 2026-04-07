using System.Runtime.CompilerServices;
using UnityEngine;

public static class Log
{
    // percentage
    private const int HEADER_SIZE = 110;
    private const int VERBOSE_HEADER_SIZE = 85;
    private const int VERBOSE_SIZE = 80;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool VerboseEnabled { get; set; } = true;
#else
    public static bool VerboseEnabled { get; set; } = false;
#endif

    private static bool _isQuitting = false;

    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        Application.quitting += () => _isQuitting = true;
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Punctuate(object message)
    {
        string text = message.ToString();
        if (text.Length == 0) return text;

        char last = text[^1];

        if (text.EndsWith("..."))
        {
            return text;
        }

        if (last is '.' or '!' or '?' or ':' or ';')
        {
            return text;
        }

        return text + '.';
    }

    #region Regular logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Header(object message)
    {
        Debug.Log($"<size={HEADER_SIZE}%><b><u>{message}</size></u></b>".AsHeader(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(object message)
    {
        Debug.Log(Punctuate(message).AsInfo(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(object message, Object context)
    {
        Debug.Log(Punctuate(message).AsInfo(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(object message)
    {
        Debug.LogWarning(Punctuate(message).AsWarning(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(object message, Object context)
    {
        Debug.LogWarning(Punctuate(message).AsWarning(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(object message)
    {
        Debug.LogError(Punctuate(message).AsError(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(object message, Object context)
    {
        Debug.LogError(Punctuate(message).AsError(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Success(object message)
    {
        Debug.Log(Punctuate(message).AsSuccess(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Success(object message, Object context)
    {
        Debug.Log(Punctuate(message).AsSuccess(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Status(object message)
    {
        Debug.Log($"<b>[STATUS]</b> {message}".AsStatus(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Status(object message, Object context)
    {
        Debug.Log($"<b>[STATUS]</b> {message}".AsStatus(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Duration(object message)
    {
        Debug.Log($"<b>[DURATION]</b> {Punctuate(message)}".AsDuration(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Duration(object message, Object context)
    {
        Debug.Log($"<b>[DURATION]</b> {Punctuate(message)}".AsDuration(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Exception(string message, string header = null)
    {
        if (_isQuitting) return;
        string label = header != null ? $"EXCEPTION: {header.ToUpper()}" : "EXCEPTION";
        Debug.Log($"<b>[{label}]</b> {Punctuate(message)}".AsException(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Exception(System.Exception exception, string message = null, string header = null)
    {
        if (_isQuitting) return;
        string text = message != null ? Punctuate(message) : Punctuate(exception.Message);
        string label = header ?? exception.GetType().Name.ToUpper();
        string formatted = label == "EXCEPTION" ? "EXCEPTION" : $"EXCEPTION: {label}";
        Debug.Log($"<b>[{formatted}]</b> {text}\n{exception.StackTrace}".AsException(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Exception(System.Exception exception, Object context, string message = null, string header = null)
    {
        if (_isQuitting) return;
        string text = message != null ? Punctuate(message) : Punctuate(exception.Message);
        string label = header ?? exception.GetType().Name.ToUpper();
        string formatted = label == "EXCEPTION" ? "EXCEPTION" : $"EXCEPTION: {label}";
        Debug.Log($"<b>[{formatted}]</b> {text}\n{exception.StackTrace}".AsException(consoleColors: true), context);
    }

    #endregion

    #region Verbose logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseHeader(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_HEADER_SIZE}%><b><u>[VERBOSE]</b> {message}</size></u>".AsHeader(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseInfo(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%><b>[VERBOSE]</b> {Punctuate(message)}</size>".AsInfo(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseInfo(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%><b>[VERBOSE]</b> {Punctuate(message)}</size>".AsInfo(verbose: true, consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseWarning(object message)
    {
        if (!VerboseEnabled) return;
        Debug.LogWarning($"<size={VERBOSE_SIZE}%><b>[VERBOSE]</b> {Punctuate(message)}</size>".AsWarning(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseWarning(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.LogWarning($"<size={VERBOSE_SIZE}%><b>[VERBOSE]</b> {Punctuate(message)}</size>".AsWarning(verbose: true, consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseSuccess(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%><b>[VERBOSE]</b> {Punctuate(message)}</size>".AsSuccess(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseSuccess(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%><b>[VERBOSE]</b> {Punctuate(message)}</size>".AsSuccess(verbose: true, consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseStatus(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%><b>[VERBOSE] [STATUS]</b> {message}</size>".AsStatus(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseStatus(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%><b>[VERBOSE] [STATUS]</b> {message}</size>".AsStatus(verbose: true, consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseDuration(object message)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%><b>[VERBOSE] [DURATION]</b> {Punctuate(message)}</size>".AsDuration(verbose: true, consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VerboseDuration(object message, Object context)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"<size={VERBOSE_SIZE}%><b>[VERBOSE] [DURATION]</b> {Punctuate(message)}</size>".AsDuration(verbose: true, consoleColors: true), context);
    }

    #endregion

    #region Conditional logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InfoIf(bool condition, object message)
    {
        if (!condition) return;
        Debug.Log(Punctuate(message).AsInfo(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InfoIf(bool condition, object message, Object context)
    {
        if (!condition) return;
        Debug.Log(Punctuate(message).AsInfo(consoleColors: true), context);
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WarningIf(bool condition, object message)
    {
        if (!condition) return;
        Debug.LogWarning(Punctuate(message).AsWarning(consoleColors: true));
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WarningIf(bool condition, object message, Object context)
    {
        if (!condition) return;
        Debug.LogWarning(Punctuate(message).AsWarning(consoleColors: true), context);
    }

    #endregion

    #region Editor logging

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Editor(object message)
    {
        Debug.Log($"[EDITOR] {Punctuate(message)}".AsEditor());
    }

    [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Editor(object message, Object context)
    {
        Debug.Log($"[EDITOR] {Punctuate(message)}".AsEditor(), context);
    }

    #endregion
}