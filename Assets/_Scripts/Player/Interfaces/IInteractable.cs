using UnityEngine;

public interface IInteractable
{
    Transform GetTransform(); // Return the Transform of interactable
    string GetInteractionType(); // Return what type the interactable is (e.g. Hand or Grab)
    void Interact(); // Method that gets called when player interacts with object
}
