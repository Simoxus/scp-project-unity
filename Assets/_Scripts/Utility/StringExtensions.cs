using UnityEngine;

public static class StringExtensions
{
    public static string Color(this string text, Color color, bool verbose = false)
    {
        if (verbose == true) color *= 0.9f;
        return $"<color={ColorScheme.ToHex(color)}>{text}</color>";
    }

    public static string AsHeader(this string text, bool verbose = false, bool consoleColors = false)
        => text.Color(consoleColors ? ColorScheme.ConsoleHeader : ColorScheme.Header, verbose);

    public static string AsInfo(this string text, bool verbose = false, bool consoleColors = false)
        => text.Color(consoleColors ? ColorScheme.ConsoleInfo : ColorScheme.Info, verbose);

    public static string AsWarning(this string text, bool verbose = false, bool consoleColors = false)
        => text.Color(consoleColors ? ColorScheme.ConsoleWarning : ColorScheme.Warning, verbose);

    public static string AsError(this string text, bool verbose = false, bool consoleColors = false)
        => text.Color(consoleColors ? ColorScheme.ConsoleError : ColorScheme.Error, verbose);

    public static string AsSuccess(this string text, bool verbose = false, bool consoleColors = false)
        => text.Color(consoleColors ? ColorScheme.ConsoleSuccess : ColorScheme.Success, verbose);

    public static string AsInput(this string text, bool verbose = false, bool consoleColors = false)
        => text.Color(ColorScheme.Input, verbose);

    public static string AsJorge(this string text, bool verbose = false, bool consoleColors = false)
        => text.Color(ColorScheme.Jorge, verbose);

    public static string AsEditor(this string text, bool verbose = false, bool consoleColors = false)
        => text.Color(ColorScheme.Editor, verbose);
}