using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ButtonDoorActivator : MonoBehaviour, IInteractable
{
    //public static event Action OnObjectInteracted;

    [Header("Button Settings")]
    [SerializeField] private string interactionType = "Hand";
    [SerializeField] private Color brokenColor = new(233, 88, 87);
    [SerializeField] private bool enableSecondButton;
    public BoxCollider secondActivatorCollider;
    public ButtonVisual buttonTweener;
    public ButtonDoorController targetDoorController;
    public BoxCollider activatorCollider;
    public FMODUnity.EventReference buttonSoundEvent;
    public FMODUnity.EventReference buttonFailSoundEvent;

    private void Awake()
    {
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"DoorActivator on '{gameObject.name}' is missing a Collider component. It will not be detectable.", this);
        }
    }
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
            //OnObjectInteracted?.Invoke();
            buttonTweener.PlayTween();

            if (targetDoorController.isBroken == false)
            {
                FMODUnity.RuntimeManager.PlayOneShot(buttonSoundEvent, transform.position);
                activatorCollider.enabled = false; // We reenable this in the door controller script :)
                if (enableSecondButton) { secondActivatorCollider.enabled = false; } // We reenable this in the door controller script :)

                targetDoorController.ToggleDoor().Forget(); // Discard cause we don't need to wait
            }
            else
            {
                FMODUnity.RuntimeManager.PlayOneShot(buttonFailSoundEvent, transform.position);
            }
        }
    }

    public void BreakButton()
    {
        /*
        activatorCollider.enabled = false;
        secondActivatorCollider.enabled = false;
        if (enableSecondButton && secondActivatorCollider != null)
        {
            secondActivatorCollider.enabled = false;
        }
        */

        buttonTweener.ToggleLogo(false);
        buttonTweener.ToggleText(true);
        buttonTweener.ChangeScreenColor(brokenColor, true);
        buttonTweener.ChangeScreenText(
            "-- CODE 4 --" +
            "Technician dispatched"
            );
    }
}
