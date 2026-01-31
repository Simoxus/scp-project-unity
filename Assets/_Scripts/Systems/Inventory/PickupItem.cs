using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private string interactionType = "Grab";

    private Outline _outline;
    private Collider _collider;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
        _collider = GetComponent<Collider>();

        if (_collider == null)
        {
            Debug.LogError($"PickupItem on '{gameObject.name}' has no Collider! It will not be detectable by PlayerInteract.");
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Outline GetOutline()
    {
        return _outline;
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