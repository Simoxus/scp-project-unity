using UnityEngine;

public enum ItemType
{
    Generic,
    Consumable,
    Keycard,
    Document,
    Tool
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Custom Data/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea(3, 10)] public string itemDescription;
    public string itemID;
    public Sprite itemIcon;

    [Header("Item Properties")]
    public ItemType itemType = ItemType.Generic;
    [Min(1)] public int itemStackSize = 1;

    // Virtual method, so any classes inheriting ItemData are able to provide their own implementation
    public virtual void UseItem()
    {
        Debug.Log($"Using item: {itemName} with ID: {itemID}");
    }
}
