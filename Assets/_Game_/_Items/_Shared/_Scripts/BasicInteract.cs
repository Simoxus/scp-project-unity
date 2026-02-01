using System;
using UnityEngine;

public class BasicInteract : MonoBehaviour, IInteractable
{
    public static event Action OnObjectInteracted;

    [SerializeField] private string interactionType = "Flip";

    public Transform GetTransform()
    {
        return transform;
    }
    public string GetInteractionType()
    {
        return interactionType;
    }

    public void Interact()
    {
        Debug.Log($"Player interacted with: {gameObject.name}! Type: {interactionType}");

        // Invoke event when object is interacted with
        OnObjectInteracted?.Invoke();
    }
}
