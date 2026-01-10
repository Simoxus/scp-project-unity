using UnityEngine;
using UnityEngine.UI;

public class UIAccess : Singleton<UIAccess>
{
    [Header("UI Systems")]
    public UITooltips Tooltips;
    public UIIndicators Indicators;
    public UITutorials Tutorials;
    public UIInspect Inspect;
    public UIConsole Console;
    public UIInventory Inventory;
    public UIPauseMenu PauseMenu;
    public UIInteract Interact;

    [Header("Other Elements")]
    public CanvasGroup BlinkOverlay;
    public Image Crosshair;
    public FpsCounter FpsCounter;
}