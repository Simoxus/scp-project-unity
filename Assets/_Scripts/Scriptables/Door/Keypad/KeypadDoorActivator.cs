using Cysharp.Threading.Tasks;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadDoorActivator : MonoBehaviour, IInteractable
{
    [Header("Keypad Settings")]
    [SerializeField] private bool enableSecondKeypad;
    [SerializeField] private string interactionType = "Hand";
    public string correctCode = "6767";
    public int maxCodeLength = 4;
    public float resetDelay = 2f;

    [Header("Keypad Interaction")]
    public GameObject[] keypadNumberVisuals;
    public GameObject keypadEnterVisual;
    public GameObject keypadClearVisual;

    [Header("Script References")]
    public Player player;
    public KeypadVisual keypadTweener;
    public KeypadDoorController targetDoorController;

    [Header("Collider References")]
    public BoxCollider activatorCollider;
    public BoxCollider secondActivatorCollider;

    [Header("Cinemachine/Camera Control")]
    public CinemachineCamera keypadCamera;
    public float cameraTransitionDuration = 1f;

    [Header("Color States")]
    public Color defaultColor = new Color(191, 191, 191);
    public Color brokenColor = new Color(191, 72, 71);
    public Color grantedColor = new Color(116, 166, 115);
    public Color deniedColor = new Color(191, 107, 106);

    [Header("FMOD Events")]
    public EventReference keypadSoundEvent;
    public EventReference keypadFailSoundEvent;
    public EventReference buttonPressSoundEvent;

    private string _currentInput = "";
    private float _previousTransitionTime = 0f;
    private bool _isProcessing = false;

    private void Awake()
    {
        // Check for player and if there's no player, try to find the singleton/instance
        player = player != null ? player : Player.Instance;

        if (activatorCollider == null && secondActivatorCollider == null)
        {
            Debug.LogWarning($"{GetType()} on '{gameObject.name}' has no colliders assigned. It will not be detectable.", this);
        }
    }

    private void OnEnable()
    {
        if (player != null && player.playerInputs != null)
        {
            player.playerInputs.OnKeypadInput += HandleKeypadInput;
        }
    }

    private void OnDisable()
    {
        if (player != null && player.playerInputs != null)
        {
            player.playerInputs.OnKeypadInput -= HandleKeypadInput;
        }
    }

    public Transform GetTransform()
    {
        return activatorCollider.transform;
    }

    public string GetInteractionType()
    {
        return interactionType;
    }

    public void Interact()
    {
        if (targetDoorController == null || _isProcessing || targetDoorController.currentState == KeypadDoorController.DoorState.Broken)
        {
            RuntimeManager.PlayOneShot(keypadFailSoundEvent, transform.position);
            return;
        }

        _isProcessing = true;
        _currentInput = "";

        // Disable all other inputs and enable the keypad input map
        player.playerInputs.DisableGameplayInputs();
        player.playerInputs.DisableUIAssistedInputs();
        player.playerInputs.EnableKeypadInputs();

        // Rotate the player
        Player.Instance.playerController.ForceRotate(keypadCamera.transform.rotation.eulerAngles);

        GameManager.Instance.RequestDisableControls(shouldDisable: true);

        // Set the keypad virtual camera's priority higher than the player's
        _previousTransitionTime = CameraManager.Instance.cameraBrain.DefaultBlend.Time;
        CameraManager.Instance.cameraBrain.DefaultBlend.Time = cameraTransitionDuration;
        keypadCamera.Priority = 75; // A value higher than 10

        keypadTweener.ToggleLogo(false);
        keypadTweener.ToggleText(true);
        keypadTweener.ChangeScreenText("");
    }

    private void HandleKeypadInput(string keyName)
    {
        if (!_isProcessing) return;

        // Check for function keys first
        if (keyName == "Enter")
        {
            //keypadTweener.PlayFunctionKeyTween(keypadEnterVisual, ).Forget();
            CheckCode(_currentInput).Forget();
            return;
        }
        if (keyName == "Clear")
        {
            // keypadTweener.PlayFunctionKeyTween(keypadClearVisual).Forget();
            RemoveLastInput();
            return;
        }

        // Handle number keys
        if (keyName.StartsWith("numpad"))
        {
            keyName = keyName.Substring(6);
        }
        if (int.TryParse(keyName, out int number))
        {
            if (keypadNumberVisuals.Length > number)
            {
                AppendInput((char)('0' + number), keypadNumberVisuals[number]).Forget();
            }
            else
            {
                Debug.LogWarning($"Keypad visual for number {number} is missing from the array.");
            }
        }
    }

    private async UniTask AppendInput(char number, GameObject buttonVisual)
    {
        if (_currentInput.Length < maxCodeLength)
        {
            _currentInput += number;
            FMODHelper.PlayOneShot3D(buttonPressSoundEvent, transform.position);
            keypadTweener.ChangeScreenText(_currentInput);
            await keypadTweener.PlayTween(buttonVisual);
        }
    }

    private void RemoveLastInput()
    {
        if (_currentInput.Length > 0)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
            FMODHelper.PlayOneShot3D(buttonPressSoundEvent, transform.position);
            keypadTweener.ChangeScreenText(_currentInput);
        }
    }

    private async UniTask CheckCode(string input)
    {
        bool success = input == correctCode;

        if (success)
        {
            FMODHelper.PlayOneShotWithParameters(
                keypadSoundEvent,
                transform.position,
                ("Result", 0.0f)
            );
            targetDoorController.ToggleDoor().Forget();
        }
        else
        {
            FMODHelper.PlayOneShotWithParameters(
                keypadSoundEvent,
                transform.position,
                ("Result", 1.0f)
            );
        }

        // Update visuals
        targetDoorController.UpdateActivatorVisuals(success, "");

        // Wait for the reset delay
        await UniTask.WaitForSeconds(resetDelay, ignoreTimeScale: false);

        _isProcessing = false;

        // Restore action maps
        player.playerInputs.DisableKeypadInputs();
        player.playerInputs.EnableUIAssistedInputs();
        player.playerInputs.EnableGameplayInputs();

        ResetPlayerCamera().Forget();
    }

    public void ForceExitKeypad()
    {
        _isProcessing = false;

        // Restore input maps
        player.playerInputs.DisableKeypadInputs();
        player.playerInputs.EnableUIAssistedInputs();
        player.playerInputs.EnableGameplayInputs();

        // Restore camera and controls
        ResetPlayerCamera(wasForceExit: true).Forget();
        GameManager.Instance.RequestDisableControls(false);
    }

    public async UniTask ResetPlayerCamera(bool? wasForceExit = false)
    {
        // Restore player camera and controls
        keypadCamera.Priority = 1;
        GameManager.Instance.RequestDisableControls(shouldDisable: false);

        if (wasForceExit == false)
        {
            // Reset button display
            keypadTweener.ToggleLogo(true);
            keypadTweener.ToggleText(false);
            keypadTweener.ChangeScreenColor(defaultColor, true, 0.8f);
            _currentInput = "";
        }

        await UniTask.WaitForSeconds(cameraTransitionDuration, ignoreTimeScale: false);
        CameraManager.Instance.cameraBrain.DefaultBlend.Time = _previousTransitionTime;
        _previousTransitionTime = 0f;
    }

    public void SetButtonState(bool enabled)
    {
        if (activatorCollider != null)
        {
            activatorCollider.enabled = enabled;
        }

        if (enableSecondKeypad && secondActivatorCollider != null)
        {
            secondActivatorCollider.enabled = enabled;
        }
    }

    public async UniTask ResetButtonDisplay()
    {
        await UniTask.WaitForSeconds(1.6f, ignoreTimeScale: false);

        keypadTweener.ToggleLogo(true);
        keypadTweener.ToggleText(false);
        keypadTweener.ChangeScreenColor(defaultColor, true, 0.8f);
        // keypadTweener.ChangeScreenText

        await UniTask.WaitForSeconds(0.6f, ignoreTimeScale: false);

        SetButtonState(true);
    }

    public void BreakButton()
    {
        keypadTweener.ToggleLogo(false);
        keypadTweener.ToggleText(true);
        keypadTweener.ChangeScreenColor(brokenColor, true);
        keypadTweener.ChangeScreenText(
            "-- CODE 4 --" +
            "Technician dispatched"
        );
    }

    public void DisplayGranted(string clearanceLevel)
    {
        keypadTweener.ToggleLogo(false);
        keypadTweener.ToggleText(true);
        keypadTweener.ChangeScreenColor(grantedColor, true, 0.5f);
        keypadTweener.ChangeScreenText(
            "ACCESS\nGRANTED"
        );

        ResetButtonDisplay().Forget();
    }

    public void DisplayDenied(string clearanceLevel)
    {
        keypadTweener.ToggleLogo(false);
        keypadTweener.ToggleText(true);
        keypadTweener.ChangeScreenColor(deniedColor, true, 0.5f);
        keypadTweener.ChangeScreenText(
            "ACCESS\nDENIED"
        );

        ResetButtonDisplay().Forget();
    }
}