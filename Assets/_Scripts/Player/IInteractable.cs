using UnityEngine;

public interface IInteractable
{
    Transform GetTransform(); // Return the Transform of interactable
    Outline GetOutline() => null; // Optional: Return outline component
    Collider GetInteractionCollider() => null; // Optional: Return collider for interaction
    Vector3 GetRaycastTarget() => GetTransform().position; // Optional: Return where to aim the raycast
    string GetInteractionType(); // Return what type the interactable is (e.g. Hand or Grab)
    void Interact(); // Method that gets called when player interacts with object
}
