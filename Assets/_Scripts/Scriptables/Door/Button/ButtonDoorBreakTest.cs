using UnityEngine;

public class ButtonDoorBreakTest : MonoBehaviour, IInteractable
{
    public ButtonDoorController doorController;

    public Transform GetTransform()
    {
        return transform;
    }
    public string GetInteractionType()
    {
        return "Flip";
    }

    public void Interact()
    {
        _ = doorController.BreakDoor(); // Do not use this kinda code in actual implementations
    }
}
