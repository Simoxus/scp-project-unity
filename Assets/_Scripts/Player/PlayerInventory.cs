using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Space]
    [SerializeField] private bool showHeldItemOnEquip = true;

    [Header("Drop Settings")]
    [SerializeField] private float dropDistance = 2f;
    [SerializeField] private float dropHeightOffset = 0.1f;
    [SerializeField] private float dropRaycastDistance = 1f;

    // State queries
    public bool IsFull => _slots.All(s => !s.IsEmpty);
    public ItemData EquippedItem => _equippedItem;
    public int ItemCount => _itemCounts.Values.Sum();
    public int SlotCount => _slots.Count;
    public int EmptySlotCount => _slots.Count(s => s.IsEmpty);

    private List<InventorySlot> _slots = new List<InventorySlot>();
    private Dictionary<string, int> _itemCounts = new Dictionary<string, int>();
    private ItemData _equippedItem;

    // Cached references
    private Player _player;
    private UIInventory _inventoryUI;
    private UITooltips _tooltipsUI;
    private PlayerInteract _playerInteract;

    private void Start()
    {
        _player = Core.Player;

        if (_player != null)
            _playerInteract = _player.Interact;

        _inventoryUI = Core.UI.Inventory;
        _tooltipsUI = Core.UI.Tooltips;

        InitializeSlots();
    }

    private void OnEnable()
    {
        if (_player != null && _player.Inputs != null)
        {
            _player.Inputs.OnUseItem += HandleUseInput;
            _player.Inputs.OnUnequipItem += HandleUnequipInput;
        }
    }

    private void OnDisable()
    {
        if (_player != null && _player.Inputs != null)
        {
            _player.Inputs.OnUseItem -= HandleUseInput;
            _player.Inputs.OnUnequipItem -= HandleUnequipInput;
        }
    }

    private void InitializeSlots()
    {
        if (_inventoryUI != null && _inventoryUI.SlotsContainer != null)
        {
            _slots.AddRange(_inventoryUI.SlotsContainer.GetComponentsInChildren<InventorySlot>());
        }
    }

    public bool AddItem(ItemData itemData)
    {
        if (itemData == null) return false;

        InventorySlot emptySlot = _slots.FirstOrDefault(s => s.IsEmpty);
        if (emptySlot == null)
            return false;

        return emptySlot.AddItem(itemData);
    }

    public void TrackItem(ItemData itemData)
    {
        if (itemData == null) return;

        if (_itemCounts.ContainsKey(itemData.itemID))
            _itemCounts[itemData.itemID]++;
        else
            _itemCounts[itemData.itemID] = 1;
    }

    public void UntrackItem(ItemData itemData)
    {
        if (itemData == null) return;

        if (_itemCounts.ContainsKey(itemData.itemID))
        {
            _itemCounts[itemData.itemID]--;
            if (_itemCounts[itemData.itemID] <= 0)
                _itemCounts.Remove(itemData.itemID);
        }
    }

    public bool HasItem(string itemID)
    {
        return _itemCounts.ContainsKey(itemID) && _itemCounts[itemID] > 0;
    }

    public int GetItemCount(string itemID)
    {
        return _itemCounts.ContainsKey(itemID) ? _itemCounts[itemID] : 0;
    }

    public T GetBehavior<T>(ItemData item) where T : ItemBehavior
    {
        if (item == null) return null;

        foreach (var behavior in item.behaviors)
        {
            if (behavior is T typed)
                return typed;
        }

        return null;
    }

    public bool HasBehavior<T>(ItemData item) where T : ItemBehavior
    {
        return GetBehavior<T>(item) != null;
    }

    public T GetEquippedBehavior<T>() where T : ItemBehavior
    {
        return GetBehavior<T>(_equippedItem);
    }

    private void HandleUseInput()
    {
        if (_equippedItem == null) return;

        if (_equippedItem.CanBeUsed())
        {
            _equippedItem.Use();
        }
    }

    private void HandleUnequipInput()
    {
        UnequipItem(true);
    }

    public void EquipItem(ItemData itemData, bool playSound = true)
    {
        if (itemData == null) return;

        _equippedItem = itemData;

        if (showHeldItemOnEquip && _inventoryUI != null)
        {
            _inventoryUI.ShowHeldItem(itemData.icon);
        }

        HandleKeycardInteraction(itemData);

        itemData.Equip(playSound);

        _tooltipsUI?.Hide();
        _inventoryUI?.Hide();
    }

    public void UnequipItem(bool playSound = true)
    {
        if (_equippedItem == null) return;

        _equippedItem.Unequip(playSound);

        if (_inventoryUI != null)
            _inventoryUI.HideHeldItem();

        _equippedItem = null;
    }

    public bool DropItemIntoWorld(ItemData itemData)
    {
        if (itemData == null || itemData.worldPrefab == null)
            return false;

        Camera cam = _player?.CameraBrain;
        if (cam == null)
            return false;

        Vector3 dropPosition = CalculateDropPosition(cam);
        string itemName = itemData.GetItemName();

        Instantiate(itemData.worldPrefab, dropPosition, Quaternion.identity);
        Log.VerboseInfo($"Dropped item '{itemName}'");

        return true;
    }

    private Vector3 CalculateDropPosition(Camera cam)
    {
        Transform cameraTransform = cam.transform;
        Vector3 dropPosition = cameraTransform.position + cameraTransform.forward * dropDistance;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, dropRaycastDistance))
        {
            dropPosition = hit.point + Vector3.up * dropHeightOffset;
        }

        return dropPosition;
    }

    public void ClearInventory()
    {
        foreach (var slot in _slots)
            slot.RemoveItem();

        _itemCounts.Clear();
        UnequipItem(false);
    }

    private void HandleKeycardInteraction(ItemData itemData)
    {
        if (_playerInteract == null) return;

        KeycardBehavior keycardBehavior = GetBehavior<KeycardBehavior>(itemData);
        if (keycardBehavior == null) return;

        if (_playerInteract.CurrentTarget is KeycardDoorActivator keycardDoor &&
            keycardDoor.IsCorrectKeycardLevel(keycardBehavior.keycardLevel))
        {
            keycardDoor.Interact();
        }
    }
}