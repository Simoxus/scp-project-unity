using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Custom/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public ItemType itemType = ItemType.Normal;

    public enum ItemType
    {
        Normal,
        Keycard,
        Document,
        Consumable,
        Equipment,
        Tool
    }

    [Header("Visuals")]
    public Sprite icon;
    public GameObject worldPrefab;

    [Header("Sounds")]
    public EventReference pickupSound;
    public EventReference equipSound;
    public EventReference unequipSound;
    public EventReference useSound;

    [Header("Behavior")]
    public List<ItemBehavior> behaviors = new List<ItemBehavior>();

    [Header("Localization")]
    public LocalizedString localizedName;
    public LocalizedString localizedDescription;
    public LocalizedString itemUseMessage;

    public bool CanBeUsed()
    {
        return behaviors.Any(b => b != null && b.CanUse(this));
    }

    public void Use(bool playSound = true)
    {
        if (playSound && !useSound.IsNull)
        {
            FMODHelper.PlayOneShot(useSound);
        }

        foreach (var behavior in behaviors)
        {
            if (behavior != null && behavior.CanUse(this))
            {
                behavior.OnUse(this);
            }
        }
    }

    public void Equip(bool playSound = true)
    {
        if (playSound && !equipSound.IsNull)
        {
            FMODHelper.PlayOneShot(equipSound);
        }

        foreach (var behavior in behaviors)
        {
            behavior?.OnEquip(this);
        }
    }

    public void Unequip(bool playSound = true)
    {
        if (playSound && !unequipSound.IsNull)
        {
            FMODHelper.PlayOneShot(unequipSound);
        }

        foreach (var behavior in behaviors)
        {
            behavior?.OnUnequip(this);
        }
    }

    public void Pickup()
    {
        if (!pickupSound.IsNull)
        {
            FMODHelper.PlayOneShot(pickupSound);
        }

        foreach (var behavior in behaviors)
        {
            behavior?.OnPickup(this);
        }
    }

    public string GetItemName()
    {
        if (localizedName.IsEmpty) return string.Empty;
        return localizedName.GetLocalizedString();
    }

    public string GetDescription()
    {
        if (localizedDescription.IsEmpty) return string.Empty;
        return localizedDescription.GetLocalizedString();
    }

    public string GetTooltipText()
    {
        string name = GetItemName();
        string desc = GetDescription();

        if (string.IsNullOrEmpty(name))
            return $"<u><b>{itemID}</b></u>";

        if (string.IsNullOrEmpty(desc))
            return $"<u><b>{name}</b></u>";

        return $"<u><b>{name}</b></u><line-height=125%>\n</line-height>{desc}";
    }
}