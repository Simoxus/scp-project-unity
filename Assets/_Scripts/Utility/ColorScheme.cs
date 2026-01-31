using UnityEngine;

public static class ColorScheme
{
    public static readonly Color Header = new Color32(0, 255, 255, 255);
    public static readonly Color Info = new Color32(216, 230, 255, 255);
    public static readonly Color Warning = new Color32(255, 165, 0, 255);
    public static readonly Color Error = new Color32(255, 0, 0, 255);
    public static readonly Color Success = new Color32(51, 204, 51, 255);
    public static readonly Color Jorge = new Color32(199, 21, 133, 255);
    public static readonly Color Input = new Color32(255, 255, 255, 255);

    public static readonly Color ConsoleHeader = new Color32(0, 255, 255, 255);
    public static readonly Color ConsoleInfo = new Color32(216, 230, 255, 255);
    public static readonly Color ConsoleWarning = new Color32(255, 193, 7, 255);
    public static readonly Color ConsoleError = new Color32(255, 83, 74, 255);
    public static readonly Color ConsoleSuccess = new Color32(51, 204, 51, 255);

    public static readonly Color Editor = new Color32(255, 153, 204, 255);

    public static string ToHex(Color color) => $"#{ColorUtility.ToHtmlStringRGBA(color)}";
}