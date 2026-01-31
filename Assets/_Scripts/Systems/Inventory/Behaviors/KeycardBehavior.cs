using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Item Behaviors/Keycard Behavior")]
public class KeycardBehavior : ItemBehavior
{
    public int keycardLevel = 1;

    public override bool CanUse(ItemData item) => false;
    public override void OnUse(ItemData item)
    {

    }
}