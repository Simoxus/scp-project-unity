using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    [Header("UI Base")]
    public Canvas Canvas;

    [Header("Inventory Panel")]
    public GameObject InventoryPanel;
    public Transform SlotsContainer;

    [Header("Held Item Display")]
    public GameObject HeldItemDisplay;
    public Image HeldItemImage;

    private PlayerInputs _inputs;
    private bool _isVisible;
    private bool _wasCursorLocked;

    public bool IsVisible => _isVisible;
    public RectTransform InventoryPanelRect => InventoryPanel?.GetComponent<RectTransform>();

    private void Awake()
    {
        ValidateCanvas();

        if (InventoryPanel != null)
            InventoryPanel.SetActive(false);

        if (HeldItemDisplay != null)
            HeldItemDisplay.SetActive(false);
    }

    private void OnEnable()
    {
        // Input events
        if (Core.Player != null)
        {
            _inputs = Core.Player.PlayerInputs;
            if (_inputs != null)
                _inputs.OnInventoryUI += Toggle;
        }

        // Pause state events
        if (Core.GameManager != null)
        {
            Core.GameManager.OnPauseStateChanged += HandlePauseStateChanged;
        }
    }

    private void OnDisable()
    {
        // Input events
        if (_inputs != null)
            _inputs.OnInventoryUI -= Toggle;

        // Pause state events
        if (Core.GameManager != null)
        {
            Core.GameManager.OnPauseStateChanged -= HandlePauseStateChanged;
        }

        ReleasePauseIfNeeded();
    }

    private void Update()
    {
        // Right-click to unequip held item
        if (InventoryManager.Instance != null &&
            InventoryManager.Instance.GetEquippedItem() != null &&
            Input.GetMouseButtonDown(1))
        {
            InventoryManager.Instance.UnequipItem();
        }
    }

    public void Toggle()
    {
        // Don't open if game is paused by something else
        if (Core.GameManager != null &&
            Core.GameManager.gamePaused &&
            !Core.GameManager.HasPauseRequest(this))
        {
            return;
        }

        if (_isVisible)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        if (_isVisible) return;

        // Prevent opening inventory while holding an equipped item
        if (InventoryManager.Instance != null && InventoryManager.Instance.GetEquippedItem() != null)
            return;

        if (Canvas == null)
        {
            Log.Error("UIInventory: Cannot show - Canvas is null");
            return;
        }

        _isVisible = true;

        if (InventoryPanel != null)
            InventoryPanel.SetActive(true);

        if (Core.GameManager != null)
            Core.GameManager.RequestPause(this);

        // Unlock and show cursor
        _wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        if (!_isVisible) return;

        _isVisible = false;

        if (InventoryPanel != null)
            InventoryPanel.SetActive(false);

        if (Core.UI.Tooltips != null)
            Core.UI.Tooltips.Hide();

        ReleasePauseIfNeeded();

        // Restore cursor state
        if (_wasCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ShowHeldItem(Sprite icon)
    {
        if (HeldItemDisplay != null && HeldItemImage != null)
        {
            HeldItemImage.sprite = icon;
            HeldItemDisplay.SetActive(true);
        }
    }

    public void HideHeldItem()
    {
        if (HeldItemDisplay != null)
            HeldItemDisplay.SetActive(false);
    }

    private void ValidateCanvas()
    {
        if (Canvas == null)
        {
            Canvas = GetComponent<Canvas>();
            if (Canvas == null)
            {
                Canvas = gameObject.AddComponent<Canvas>();
                Log.Warning("UIInventory: Canvas was missing and has been added automatically.");
            }
        }
    }

    private void HandlePauseStateChanged(bool isPaused, object requester)
    {
        // If something else paused, close inventory
        if (!ReferenceEquals(requester, this))
        {
            if (_isVisible)
            {
                Hide();
            }

            // Unequip held item when paused by something else
            if (InventoryManager.Instance != null && InventoryManager.Instance.GetEquippedItem() != null)
            {
                InventoryManager.Instance.UnequipItem();
            }
        }
    }

    private void ReleasePauseIfNeeded()
    {
        if (Core.GameManager != null && Core.GameManager.HasPauseRequest(this))
            Core.GameManager.ReleasePause(this);
    }
}