using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private List<InventorySlot> _slots = new List<InventorySlot>();
    private Dictionary<string, int> _itemCounts = new Dictionary<string, int>();
    private ItemData _equippedItem;

    public bool IsFull => _slots.All(s => !s.IsEmpty);

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (Core.UI.Inventory != null && Core.UI.Inventory.SlotsContainer != null)
        {
            _slots.AddRange(Core.UI.Inventory.SlotsContainer.GetComponentsInChildren<InventorySlot>());
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
        if (_itemCounts.ContainsKey(itemData.itemID))
            _itemCounts[itemData.itemID]++;
        else
            _itemCounts[itemData.itemID] = 1;
    }

    public void UntrackItem(ItemData itemData)
    {
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

    // Helper methods
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

    public void EquipItem(ItemData itemData)
    {
        if (itemData == null) return;

        _equippedItem = itemData;

        if (Core.UI.Inventory != null)
        {
            Core.UI.Inventory.ShowHeldItem(itemData.icon);
        }

        // Handle keycard interaction
        if (Core.Player?.PlayerInteract != null)
        {
            KeycardBehavior keycardBehavior = GetBehavior<KeycardBehavior>(itemData);
            if (keycardBehavior != null)
            {
                if (Core.Player.PlayerInteract.CurrentTarget is KeycardDoorActivator keycardDoor &&
                    keycardDoor.IsCorrectKeycardLevel(keycardBehavior.keycardLevel))
                {
                    keycardDoor.Interact();
                    return;
                }
            }
        }

        itemData.Equip();

        if (Core.UI.Tooltips != null)
            Core.UI.Tooltips.Hide();

        if (Core.UI.Inventory != null)
            Core.UI.Inventory.Hide();
    }

    public void UnequipItem()
    {
        if (_equippedItem == null) return;

        _equippedItem.Unequip();

        if (Core.UI.Inventory != null)
            Core.UI.Inventory.HideHeldItem();

        _equippedItem = null;
    }

    public ItemData GetEquippedItem() => _equippedItem;

    public void ClearInventory()
    {
        foreach (var slot in _slots)
            slot.RemoveItem();

        _itemCounts.Clear();
        UnequipItem();
    }
}