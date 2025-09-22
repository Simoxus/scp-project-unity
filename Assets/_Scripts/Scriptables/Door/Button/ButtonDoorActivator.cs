using Cysharp.Threading.Tasks;
using UnityEngine;
using FMODUnity;

public class ButtonDoorActivator : MonoBehaviour, IInteractable
{
    [Header("Button Settings")]
    [SerializeField] private bool enableSecondButton;
    [SerializeField] private string interactionType = "Hand";
    [SerializeField] private Color brokenColor = new(233, 88, 87);

    [Header("Collider References")]
    public BoxCollider activatorCollider;
    public BoxCollider secondActivatorCollider;

    [Header("Script References")]
    public ButtonVisual buttonTweener;
    public ButtonDoorController targetDoorController;

    [Header("FMOD Events")]
    public EventReference buttonSoundEvent;
    public EventReference buttonFailSoundEvent;

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
        if (targetDoorController == null) return;

        buttonTweener.PlayTween().Forget();

        if (targetDoorController.currentState != ButtonDoorController.DoorState.Broken)
        {
            FMODUnity.RuntimeManager.PlayOneShot(buttonSoundEvent, transform.position);
            targetDoorController.ToggleDoor().Forget();
        }
        else
        {
            FMODUnity.RuntimeManager.PlayOneShot(buttonFailSoundEvent, transform.position);
        }
    }

    public void SetButtonState(bool enabled)
    {
        if (activatorCollider != null)
        {
            activatorCollider.enabled = enabled;
        }

        if (enableSecondButton && secondActivatorCollider != null)
        {
            secondActivatorCollider.enabled = enabled;
        }
    }

    public void BreakButton()
    {
        buttonTweener.ToggleLogo(false);
        buttonTweener.ToggleText(true);
        buttonTweener.ChangeScreenColor(brokenColor, true);
        buttonTweener.ChangeScreenText(
            "-- CODE 4 --" +
            "Technician dispatched"
        );
    }
}
