using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;

public class OfficeDoorActivator : MonoBehaviour, IInteractable
{
    [Header("Handle Settings")]
    [SerializeField] private string interactionType = "Hand";
    [SerializeField] private bool enableSecondButton;
    public HandleVisual handleTweener;
    public OfficeDoorController targetDoorController;
    public FMODUnity.EventReference buttonSoundEvent;

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
        if (targetDoorController != null)
        {
            handleTweener.PlayTween();

            FMODHelper.PlayOneShot3D(buttonSoundEvent, transform.position);

            targetDoorController.activatorCollider1.enabled = false; // We reenable this in the door controller script :)
            if (enableSecondButton) { targetDoorController.activatorCollider2.enabled = false; } // We reenable this in the door controller script :)

            targetDoorController.ToggleDoor().Forget(); // Discard cause we don't need to wait
        }
    }
}