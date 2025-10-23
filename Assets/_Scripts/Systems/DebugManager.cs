using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    private List<string> logLines = new List<string>();
    public int maxLogLines = 200;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Log.VerboseWarning($"Duplicate instance of {GetType().Name} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string colorTag = GetColorForLogType(type);
        string formattedLog = $"{colorTag}[{type}] {logString}</color>";

        logLines.Add(formattedLog);

        while (logLines.Count > maxLogLines)
        {
            logLines.RemoveAt(0);
        }
    }

    public List<string> GetLogs()
    {
        return new List<string>(logLines);
    }

    public void ClearLogs()
    {
        logLines.Clear();
    }

    private string GetColorForLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                return "<color=#FF0000FF>";
            case LogType.Warning:
                return "<color=#FFA500FF>";
            case LogType.Assert:
                return "<color=#7FA0AB>";
            case LogType.Log:
                return "<color=#ADD8E6FF>";
            default:
                return "<color=#FFFFFF>";
        }
    }
}
