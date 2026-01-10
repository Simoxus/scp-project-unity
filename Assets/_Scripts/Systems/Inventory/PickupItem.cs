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
        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.IsFull)
        {
            Debug.Log("Inventory is full!");
            return;
        }

        if (InventoryManager.Instance.AddItem(itemData))
        {
            Destroy(gameObject);
        }
    }
}