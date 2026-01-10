using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Item Behaviors/Keycard Behavior")]
public class KeycardBehavior : ItemBehavior
{
    [Header("Keycard Settings")]
    public int keycardLevel = 1;

    public override bool CanUse(ItemData item)
    {
        // Keycards are equipped, not used
        return false;
    }

    public override void OnUse(ItemData item)
    {

    }
}