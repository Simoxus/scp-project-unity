using UnityEngine;
using UnityEngine.UI;

public class UIAccess : Singleton<UIAccess>
{
    [Space]
    public UIConsole Console;
    public UIIndicators Indicators;
    public UIInspect Inspect;
    public UIInteract Interact;
    public UIInventory Inventory;
    public UIPauseMenu PauseMenu;
    public UISubtitles Subtitles;
    public UITooltips Tooltips;
    public UITutorials Tutorials;

    [Header("Other")]
    public CanvasGroup BlinkOverlay;
    public Image Crosshair;
    public FpsCounter FpsCounter;
}