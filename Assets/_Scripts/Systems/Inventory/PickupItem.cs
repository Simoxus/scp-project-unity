using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Space]
    [SerializeField] private ItemData itemData;
    [SerializeField] private Collider pickupCollider;
    [SerializeField] private Transform raycastTarget;
    [SerializeField] private string interactionType = "Grab";

    private Outline _outline;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Outline GetOutline()
    {
        return _outline;
    }

    public Collider GetInteractionCollider()
    {
        return pickupCollider;
    }

    public Vector3 GetRaycastTarget()
    {
        return raycastTarget != null ? raycastTarget.position : transform.position;
    }

    public string GetInteractionType()
    {
        return interactionType;
    }

    public void Interact()
    {
        if (itemData == null) return;
        if (Core.Player.Inventory == null) return;

        if (Core.Player.Inventory.IsFull)
        {
            Debug.Log("Inventory is full!");
            return;
        }

        if (Core.Player.Inventory.AddItem(itemData))
        {
            Destroy(gameObject);
        }
    }
}