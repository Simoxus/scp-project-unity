using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Custom Data/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea(3, 10)]
    public string itemDescription = "Generic item.";
    public Sprite itemIcon;

    [Header("Item Properties")]
    public ItemType itemType = ItemType.Generic;
    public int itemStackSize = 1;
    public string itemID;
    // You can add a unique ID for each item if you plan on saving/loading
    // Ensure this is truly unique if you use it for identification

    // It's a virtual method so derived classes can provide their own implementation.
    public virtual void UseItem()
    {
        
    }
}

public enum ItemType
{
    Generic,
    Consumable,
    Keycard,
    Document,
    Tool
}
