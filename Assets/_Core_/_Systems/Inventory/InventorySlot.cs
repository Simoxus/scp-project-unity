using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Space]
    [SerializeField] private Image slotImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlightBorder;

    [Header("Drag Settings")]
    [SerializeField] private Vector2 dragIconSize = new Vector2(80, 80);
    [SerializeField] private float dragIconAlpha = 1f;

    [Header("Interaction Settings")]
    [SerializeField] private float doubleClickTime = 0.3f;

    public ItemData ItemData
    {
        get => _itemData;
        private set => _itemData = value;
    }

    public bool IsEmpty => _itemData == null;

    private ItemData _itemData;
    private float _lastClickTime;
    private bool _isDragging;

    private static InventorySlot _draggedSlot;
    private static GameObject _dragIcon;
    private static Canvas _canvas;

    private void Awake()
    {
        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();

        UpdateVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsEmpty) return;

        if (eventData.button == PointerEventData.InputButton.Right && !_isDragging)
        {
            // Check if this item is currently equipped
            if (Core.Player?.Inventory != null && Core.Player.Inventory.EquippedItem == _itemData)
            {
                Core.Player.Inventory.UnequipItem();
            }
            else
            {
                DropIntoWorld();
            }
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.time - _lastClickTime < doubleClickTime)
            {
                EquipItem();
                _lastClickTime = 0f;
            }
            else
            {
                _lastClickTime = Time.time;
            }
        }
    }

    private void EquipItem()
    {
        if (IsEmpty || Core.Player?.Inventory == null) return;

        if (Core.UI.Tooltips != null)
            Core.UI.Tooltips.Hide();

        if (highlightBorder != null)
            highlightBorder.SetActive(false);

        if (_itemData.CanBeUsed())
        {
            _itemData.Use();
        }
        else
        {
            Core.Player.Inventory.EquipItem(_itemData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        _isDragging = true;
        _draggedSlot = this;

        CreateDragIcon();

        if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        if (Core.UI.Tooltips != null)
        {
            Core.UI.Tooltips.Hide();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragIcon != null)
        {
            _dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        DestroyDragIcon();

        if (iconImage != null)
            iconImage.enabled = !IsEmpty;

        if (ShouldDropIntoWorld(eventData))
        {
            DropIntoWorld();
        }

        _draggedSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_draggedSlot == null || _draggedSlot == this) return;

        SwapItems(_draggedSlot);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightBorder != null)
            highlightBorder.SetActive(true);

        if (!IsEmpty && Core.UI.Tooltips != null)
            Core.UI.Tooltips.Show(_itemData.GetTooltipText()).Forget();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightBorder != null)
            highlightBorder.SetActive(false);

        if (Core.UI.Tooltips != null)
            Core.UI.Tooltips.Hide();
    }

    private void CreateDragIcon()
    {
        if (_dragIcon != null)
            Destroy(_dragIcon);

        _dragIcon = new GameObject("DragIcon");
        _dragIcon.transform.SetParent(_canvas.transform, false);

        Image dragImage = _dragIcon.AddComponent<Image>();
        dragImage.sprite = _itemData.icon;
        dragImage.raycastTarget = false;

        CanvasGroup canvasGroup = _dragIcon.AddComponent<CanvasGroup>();
        canvasGroup.alpha = dragIconAlpha;
        canvasGroup.blocksRaycasts = false;

        RectTransform rectTransform = _dragIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = dragIconSize;
    }

    private void DestroyDragIcon()
    {
        if (_dragIcon != null)
        {
            Destroy(_dragIcon);
            _dragIcon = null;
        }
    }

    private bool ShouldDropIntoWorld(PointerEventData eventData)
    {
        if (Core.UI.Inventory == null) return false;

        return !RectTransformUtility.RectangleContainsScreenPoint(
            Core.UI.Inventory.InventoryPanelRect,
            eventData.position,
            eventData.pressEventCamera);
    }

    private void SwapItems(InventorySlot otherSlot)
    {
        ItemData temp = _itemData;
        _itemData = otherSlot._itemData;
        otherSlot._itemData = temp;

        UpdateVisuals();
        otherSlot.UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (iconImage != null)
        {
            iconImage.sprite = _itemData?.icon;
            iconImage.enabled = !IsEmpty;
        }
    }

    private void UseItem()
    {
        if (IsEmpty || Core.Player?.Inventory == null) return;

        if (Core.UI.Tooltips != null)
            Core.UI.Tooltips.Hide();

        if (highlightBorder != null)
            highlightBorder.SetActive(false);

        if (_itemData.CanBeUsed())
        {
            _itemData.Use();
        }
        else
        {
            Core.Player.Inventory.EquipItem(_itemData);
        }
    }

    private void DropIntoWorld()
    {
        if (IsEmpty || Core.Player?.Inventory == null) return;

        if (Core.Player.Inventory.DropItemIntoWorld(_itemData))
        {
            RemoveItem();
        }
    }

    public bool AddItem(ItemData item)
    {
        if (!IsEmpty) return false;

        _itemData = item;
        UpdateVisuals();

        if (Core.Player?.Inventory != null)
            Core.Player.Inventory.TrackItem(_itemData);

        _itemData.Pickup();

        return true;
    }

    public void RemoveItem()
    {
        if (IsEmpty) return;

        if (Core.Player?.Inventory != null)
            Core.Player.Inventory.UntrackItem(_itemData);

        _itemData = null;
        UpdateVisuals();
    }

    public void ClearOutline()
    {
        if (highlightBorder != null)
        {
            highlightBorder.SetActive(false);
        }
    }
}