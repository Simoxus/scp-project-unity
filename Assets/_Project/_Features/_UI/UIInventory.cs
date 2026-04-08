using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    [Space]
    public Canvas Canvas;

    [Header("Inventory Panel")]
    public GameObject InventoryPanel;
    public Transform SlotsContainer;

    [Header("Held Item Display")]
    public GameObject HeldItemDisplay;
    public Image HeldItemImage;

    public bool IsVisible => _isVisible;
    public RectTransform InventoryPanelRect => InventoryPanel?.GetComponent<RectTransform>();

    private PlayerInputs _inputs;
    private bool _isVisible;
    private bool _wasCursorLocked;

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
        if (Core.Player != null)
        {
            _inputs = Core.Player.Inputs;
            _inputs.OnInventoryUI += Toggle;
        }

        Core.GameManager.OnPauseStateChanged += HandlePauseStateChanged;
    }

    private void OnDisable()
    {
        _inputs.OnInventoryUI -= Toggle;

        Core.GameManager.OnPauseStateChanged -= HandlePauseStateChanged;
        Core.GameManager.ReleasePauseIfRequested(this);
    }

    public void Toggle()
    {
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

        PlayerInventory inventory = Core.Player?.Inventory;
        if (inventory != null && inventory.EquippedItem != null) return;

        if (Canvas == null) return;

        _isVisible = true;

        if (InventoryPanel != null)
            InventoryPanel.SetActive(true);

        ClearAllSlotOutlines();

        if (Core.GameManager != null)
            Core.GameManager.RequestPause(this);

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

        ClearAllSlotOutlines();
        Core.GameManager.ReleasePauseIfRequested(this);

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

    private void ClearAllSlotOutlines()
    {
        if (SlotsContainer == null) return;

        InventorySlot[] slots = SlotsContainer.GetComponentsInChildren<InventorySlot>();
        foreach (var slot in slots)
        {
            slot.ClearOutline();
        }
    }

    private void ValidateCanvas()
    {
        if (Canvas == null)
        {
            Canvas = GetComponent<Canvas>();
            if (Canvas == null)
            {
                Canvas = gameObject.AddComponent<Canvas>();
            }
        }
    }

    private void HandlePauseStateChanged(bool isPaused, object requester)
    {
        if (!ReferenceEquals(requester, this))
        {
            if (_isVisible)
            {
                Hide();
            }

            PlayerInventory inventory = Core.Player?.Inventory;
            if (inventory != null && inventory.EquippedItem != null)
            {
                inventory.UnequipItem(false);
            }
        }
    }
}