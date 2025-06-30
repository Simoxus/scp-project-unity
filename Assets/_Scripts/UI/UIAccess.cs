using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIAccess : MonoBehaviour
{
    [Header("Canvas Parents")]
    public GameObject canvasOverlays;
    public GameObject canvasIndicators;
    public GameObject canvasDebuggers;

    [Header("Main UI Scripts")]
    // put indicator stuff here
    public ConsoleUI consoleUI;
    public DebugUI debugUI;

    [Header("Sounds")]
    public EventReference uiPressEvent;
    public EventReference uiPressFailEvent;

    private void Reset() // Auto assignment
    {
        canvasOverlays = GameObject.Find("Overlays");
        canvasIndicators = GameObject.Find("Indicators");
        canvasDebuggers = GameObject.Find("Debuggers");

        consoleUI = GetComponentInChildren<ConsoleUI>();
        debugUI = GetComponentInChildren<DebugUI>();
    }
}