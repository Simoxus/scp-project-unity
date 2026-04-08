using Cysharp.Threading.Tasks;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;

public class KeypadDoorActivator : BaseDoorActivator
{
    public override BaseDoorController DoorController => targetDoorController;

    [Space]
    public KeypadDoorVisual KeypadVisual;
    [SerializeField] private KeypadDoorController targetDoorController;
    [SerializeField] private GameObject[] keypadNumberVisuals;
    [SerializeField] private GameObject keypadEnterVisual;
    [SerializeField] private GameObject keypadClearVisual;

    [Header("Cinemachine/Camera Control")]
    [SerializeField] private CinemachineCamera keypadCamera;
    [SerializeField] private float cameraTransitionDuration = 1f;

    private string _currentInput = "";
    private float _previousTransitionTime = 0f;
    private bool _isProcessing = false;

    private string FormatCodeInput(string input)
    {
        return $"<u><size=30%><cspace=-0.05em>ACCESS CODE:</cspace></size></u>\n<line-height=-10%>\\n</line-height>\n{input}";
    }

    private void OnEnable()
    {
        if (Core.Player != null && Core.Player.Inputs != null)
        {
            Core.Player.Inputs.OnKeypadInput += HandleKeypadInput;
        }
    }

    private void OnDisable()
    {
        if (Core.Player != null && Core.Player.Inputs != null)
        {
            Core.Player.Inputs.OnKeypadInput -= HandleKeypadInput;
        }

        if (Core.GameManager != null && Core.GameManager.HasDisableControlsRequest(this))
        {
            ForceExitKeypad();
        }
    }

    public override Transform GetTransform()
    {
        return ActivatorCollider.transform;
    }

    public override void Interact()
    {
        if (targetDoorController == null || _isProcessing || targetDoorController.currentState == KeypadDoorController.DoorState.Broken)
        {
            RuntimeManager.PlayOneShot(Core.AudioDataAccess.Doors.ButtonErrorSound, transform.position);
            return;
        }

        if (targetDoorController.locked)
        {
            FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.ButtonErrorSound, transform.position);
            return;
        }

        _isProcessing = true;
        _currentInput = "";

        Core.Player.Inputs.DisableGameplayInputs();
        Core.Player.Inputs.DisableUIInputs();
        Core.Player.Inputs.EnableKeypadInputs();

        Core.Player.Controller.ForceRotate(keypadCamera.transform.rotation.eulerAngles);
        Core.Player.Inventory.UnequipItem(false);

        Core.GameManager.RequestDisableControls(this, shouldDisable: true);
        Core.GameManager.UpdateCursorVisiblity(forceDisable: true);

        _previousTransitionTime = Core.Player.CameraSettings.DefaultBlend.Time;
        Core.Player.CameraSettings.DefaultBlend.Time = cameraTransitionDuration;
        keypadCamera.Priority = 100;
        keypadCamera.enabled = true;

        KeypadVisual.ToggleLogo(false);
        KeypadVisual.ToggleText(true);
        KeypadVisual.ChangeScreenText(FormatCodeInput(""));
    }

    private void HandleKeypadInput(string keyName)
    {
        if (!_isProcessing) return;

        if (keyName == "Enter")
        {
            CheckCode(_currentInput).Forget();
            return;
        }
        if (keyName == "Clear")
        {
            RemoveLastInput();
            return;
        }

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
                Log.Warning($"Keypad visual for number {number} is missing from the array");
            }
        }
    }

    private async UniTask AppendInput(char number, GameObject buttonVisual)
    {
        if (_currentInput.Length < targetDoorController.maxCodeLength)
        {
            _currentInput += number;

            FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.ButtonSound, transform.position);
            VibrationHelper.VibrateTap();

            KeypadVisual.ChangeScreenText(FormatCodeInput(_currentInput));

            await KeypadVisual.PlayNumberKeyTween(buttonVisual);
        }
    }

    private void RemoveLastInput()
    {
        KeypadVisual.PlayClearKeyTween().Forget();

        FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.ButtonSound, transform.position);
        VibrationHelper.VibrateTap();

        if (_currentInput.Length > 0)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
            KeypadVisual.ChangeScreenText(FormatCodeInput(_currentInput));
        }
    }

    private async UniTask CheckCode(string input)
    {
        Core.Player.Inputs.DisableKeypadInputs();

        KeypadVisual.PlayEnterKeyTween().Forget();

        FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.ButtonSound, transform.position);
        VibrationHelper.Vibrate();

        bool success = input == targetDoorController.correctCode;

        if (success)
        {
            FMODHelper.PlayOneShot3D(
                Core.AudioDataAccess.Doors.ButtonKeypadSound,
                transform.position,
                parameters: new[] { ("Result", 0.0f) }
            );
            targetDoorController.ToggleDoor().Forget();
        }
        else
        {
            FMODHelper.PlayOneShot3D(
                Core.AudioDataAccess.Doors.ButtonKeypadSound,
                transform.position,
                parameters: new[] { ("Result", 1.0f) }
            );
        }

        targetDoorController.UpdateActivatorVisuals(success, "");

        await UniTask.WaitForSeconds(targetDoorController.codeResetDelay, ignoreTimeScale: false);

        _isProcessing = false;

        Core.Player.Inputs.DisableKeypadInputs();
        Core.Player.Inputs.EnableUIInputs();
        Core.Player.Inputs.EnableGameplayInputs();

        await ResetPlayerCamera();
    }

    public void ForceExitKeypad()
    {
        _isProcessing = false;

        Core.Player.Inputs.DisableKeypadInputs();
        Core.Player.Inputs.EnableUIInputs();
        Core.Player.Inputs.EnableGameplayInputs();

        ResetPlayerCamera(wasForceExit: true).Forget();
    }

    public async UniTask ResetPlayerCamera(bool? wasForceExit = false)
    {
        keypadCamera.Priority = -1;
        keypadCamera.enabled = false;

        Core.GameManager.RequestDisableControls(this, shouldDisable: false);

        if (wasForceExit == false)
        {
            KeypadVisual.ToggleLogo(true);
            KeypadVisual.ToggleText(false);
            KeypadVisual.ChangeScreenColor(targetDoorController.SuccessStateColor, true, 0.8f);
            _currentInput = "";
        }

        await UniTask.WaitForSeconds(cameraTransitionDuration, ignoreTimeScale: false);
        Core.Player.CameraSettings.DefaultBlend.Time = _previousTransitionTime;
        _previousTransitionTime = 0f;
    }

    public async UniTask ResetButtonDisplay()
    {
        await UniTask.WaitForSeconds(1.6f, ignoreTimeScale: false);

        KeypadVisual.ToggleLogo(true);
        KeypadVisual.ToggleText(false);
        KeypadVisual.ChangeScreenColor(targetDoorController.SuccessStateColor, true, 0.8f);

        await UniTask.WaitForSeconds(0.6f, ignoreTimeScale: false);

        SetButtonState(true);
    }

    public override void StartPulseEffect(Color startColor, float? customDuration = null, float? customIntensity = null)
    {
        if (KeypadVisual != null)
        {
            KeypadVisual.StartPulse(startColor, customDuration, customIntensity);
        }
    }

    public override void StopPulseEffect()
    {
        if (KeypadVisual != null)
        {
            KeypadVisual.StopPulse();
        }
    }

    public void TransitionToPulseEffect(Color targetColor, float transitionDuration, float pulseDuration, float pulseIntensity)
    {
        if (KeypadVisual != null)
        {
            KeypadVisual.TransitionToPulse(targetColor, transitionDuration, pulseDuration, pulseIntensity);
        }
    }
}