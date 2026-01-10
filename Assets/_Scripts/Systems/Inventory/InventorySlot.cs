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
    [Header("Visual")]
    [SerializeField] private Image slotImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlightBorder;

    public ItemData itemData;

    private static InventorySlot _draggedSlot;
    private static GameObject _dragIcon;
    private static Canvas _canvas;

    private float _lastClickTime;
    private bool _isDragging;

    public bool IsEmpty => itemData == null;

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
            DropIntoWorld();
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.time - _lastClickTime < 0.3f)
            {
                UseItem();
                _lastClickTime = 0f;
            }
            else
            {
                _lastClickTime = Time.time;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        _isDragging = true;
        _draggedSlot = this;

        _dragIcon = new GameObject("DragIcon");
        _dragIcon.transform.SetParent(_canvas.transform, false);

        Image dragImage = _dragIcon.AddComponent<Image>();
        dragImage.sprite = itemData.icon;
        dragImage.raycastTarget = false;

        CanvasGroup canvasGroup = _dragIcon.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;

        RectTransform rectTransform = _dragIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);

        if (iconImage != null)
            iconImage.enabled = false;

        if (Core.UI.Tooltips != null)
            Core.UI.Tooltips.Hide();
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

        if (_dragIcon != null)
            Destroy(_dragIcon);

        if (iconImage != null)
            iconImage.enabled = !IsEmpty;

        // Drop into world if dragged outside inventory
        if (Core.InventoryManager != null &&
            !RectTransformUtility.RectangleContainsScreenPoint(
                Core.UI.Inventory.InventoryPanelRect,
                eventData.position,
                eventData.pressEventCamera))
        {
            DropIntoWorld();
        }

        _draggedSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_draggedSlot == null || _draggedSlot == this) return;

        ItemData temp = itemData;
        itemData = _draggedSlot.itemData;
        _draggedSlot.itemData = temp;

        UpdateVisuals();
        _draggedSlot.UpdateVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightBorder != null)
            highlightBorder.SetActive(true);

        if (!IsEmpty && Core.UI.Tooltips != null)
            Core.UI.Tooltips.Show(itemData.GetTooltipText()).Forget();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightBorder != null)
            highlightBorder.SetActive(false);

        if (Core.UI.Tooltips != null)
            Core.UI.Tooltips.Hide();
    }

    public bool AddItem(ItemData item)
    {
        if (!IsEmpty) return false;

        itemData = item;
        UpdateVisuals();

        if (Core.InventoryManager != null)
            Core.InventoryManager.TrackItem(itemData);

        itemData.Pickup();

        return true;
    }

    public void RemoveItem()
    {
        if (IsEmpty) return;

        if (Core.InventoryManager != null)
            Core.InventoryManager.UntrackItem(itemData);

        itemData = null;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (iconImage != null)
        {
            iconImage.sprite = itemData?.icon;
            iconImage.enabled = !IsEmpty;
        }
    }

    private void UseItem()
    {
        if (IsEmpty || Core.InventoryManager == null) return;

        // Hide tooltip and outline
        if (Core.UI.Tooltips != null)
            Core.UI.Tooltips.Hide();

        if (highlightBorder != null)
            highlightBorder.SetActive(false);

        // Check if usable item
        if (itemData.CanBeUsed())
        {
            itemData.Use();
        }
        else
        {
            Core.InventoryManager.EquipItem(itemData);
        }
    }

    private void DropIntoWorld()
    {
        if (IsEmpty || itemData.worldPrefab == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 dropPosition = cam.transform.position + cam.transform.forward * 2f;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 4f))
            dropPosition = hit.point + Vector3.up * 0.1f;

        string itemName = itemData.GetItemName();

        Instantiate(itemData.worldPrefab, dropPosition, Quaternion.identity);
        RemoveItem();

        Log.VerboseInfo($"Dropped item '{itemName}'");
    }
}