using UnityEngine;

public abstract class ItemBehavior : ScriptableObject
{
    public virtual bool CanUse(ItemData item) => true;

    public abstract void OnUse(ItemData item);
    public virtual void OnEquip(ItemData item) { }
    public virtual void OnUnequip(ItemData item) { }
    public virtual void OnPickup(ItemData item) { }
}