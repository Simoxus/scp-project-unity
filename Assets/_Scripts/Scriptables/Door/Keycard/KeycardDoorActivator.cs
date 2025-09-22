using Cysharp.Threading.Tasks;
using UnityEngine;
using FMODUnity;

public class KeycardDoorActivator : MonoBehaviour, IInteractable
{
    [Header("Button Settings")]
    [SerializeField] private bool enableSecondButton;
    [SerializeField] private string interactionType = "Hand";
    [SerializeField] private Color defaultColor = new(191, 191, 191);
    [SerializeField] private Color brokenColor = new(233, 88, 87);
    [SerializeField] private Color grantedColor = new(88, 233, 87);
    [SerializeField] private Color deniedColor = new(157, 88, 87);

    [Header("Collider References")]
    public BoxCollider activatorCollider;
    public BoxCollider secondActivatorCollider;

    [Header("Script References")]
    public KeycardVisual buttonTweener;
    public KeycardDoorController targetDoorController;

    [Header("FMOD Events")]
    public EventReference keycardSoundEvent;
    public EventReference keycardFailSoundEvent;

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

        if (targetDoorController.currentState == KeycardDoorController.DoorState.Broken)
        {
            RuntimeManager.PlayOneShot(keycardFailSoundEvent, transform.position);
            return;
        }

        SetButtonState(false);

        // bool keycardCheckSuccessful = CheckKeycard(); -- Eventually :)

        bool keycardCheckSuccessful = true;
        int keycardClearanceLevel = 4;

        FMODHelper.PlayOneShotWithParameters(
            keycardSoundEvent,
            transform.position,
            ("Result", keycardCheckSuccessful ? 0.0f : 1.0f)
        );

        if (keycardCheckSuccessful)
        {
            targetDoorController.ToggleDoor().Forget();
        }

        targetDoorController.UpdateActivatorVisuals(keycardCheckSuccessful, keycardClearanceLevel.ToString());
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

    public async UniTask ResetButtonDisplay()
    {
        await UniTask.WaitForSeconds(1.6f, ignoreTimeScale: false);

        buttonTweener.ToggleLogo(true);
        buttonTweener.ToggleText(false);
        buttonTweener.ChangeScreenColor(defaultColor, true, 0.8f);
        buttonTweener.ChangeScreenText(
            "HI"
        );

        await UniTask.WaitForSeconds(0.15f, ignoreTimeScale: false);

        SetButtonState(true);
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

    public void DisplayGranted(string clearanceLevel)
    {
        buttonTweener.ToggleLogo(false);
        buttonTweener.ToggleText(true);
        buttonTweener.ChangeScreenColor(grantedColor, true, 1f);
        buttonTweener.ChangeScreenText(
            $"LEVEL {clearanceLevel} DETECTED"
        );

        ResetButtonDisplay().Forget();
    }

    public void DisplayDenied(string clearanceLevel)
    {
        buttonTweener.ToggleLogo(false);
        buttonTweener.ToggleText(true);
        buttonTweener.ChangeScreenColor(deniedColor, true, 1f);
        buttonTweener.ChangeScreenText(
            $"LEVEL {clearanceLevel} REQUIRED"
        );

        ResetButtonDisplay().Forget();
    }

    public void DisplayLocked(string optionalError)
    {
        buttonTweener.ToggleLogo(false);
        buttonTweener.ToggleText(true);

        if (optionalError != null)
        {
            buttonTweener.ChangeScreenText(
                "-- LOCKED: UNKNOWN --" +
                "PLEASE CONTACT NEAREST FACILITY TECHNICIAN"
            );
        }
        else
        {
            buttonTweener.ChangeScreenText(
                "-- LOCKED: --" +
                $"{optionalError}"
            );
        }

        ResetButtonDisplay().Forget();
    }
}
