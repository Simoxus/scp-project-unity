using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Item Behaviors/Severerd Hand Behavior")]
public class SeveredHandBehavior : ItemBehavior
{
    public int handType = 0;

    public override bool CanUse(ItemData item) => false;
    public override void OnUse(ItemData item)
    {

    }
}