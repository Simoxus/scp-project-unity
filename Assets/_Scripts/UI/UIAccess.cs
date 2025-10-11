using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

public class UIAccess : MonoBehaviour
{
    public static UIAccess Instance { get; private set; }

    [Header("Canvas Parents")]
    public GameObject canvasOverlays;
    public GameObject canvasIndicators;
    public GameObject canvasInventory;
    public GameObject canvasTelemetry;
    public GameObject canvasPauseMenu;
    public GameObject canvasDebuggers;
    public GameObject canvasInteract;

    [Header("For Settings")]
    public Image crosshair;
    public FPSCounterUI fpsCounter;

    [Header("Main UI Scripts")]
    // put indicator stuff here
    public ConsoleUI consoleUI;
    public DebugUI debugUI;
    public IndicatorsUI indicatorsUI;
    public UIDebugPopup uiDebugPopup;

    [Header("Sounds")]
    public EventReference uiPressEvent;
    public EventReference uiPressFailEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Reset() // Auto assignment
    {
        canvasOverlays = GameObject.Find("Overlays");
        canvasIndicators = GameObject.Find("Indicators");
        canvasInventory = GameObject.Find("Inventory");
        canvasPauseMenu = GameObject.Find("Indicators");
        canvasDebuggers = GameObject.Find("Debuggers");
        canvasInteract = GameObject.Find("InteractScreen");

        consoleUI = GetComponentInChildren<ConsoleUI>();
        debugUI = GetComponentInChildren<DebugUI>();
        indicatorsUI = GetComponentInChildren<IndicatorsUI>();
    }
}