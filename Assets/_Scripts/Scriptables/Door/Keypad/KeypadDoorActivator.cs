using Cysharp.Threading.Tasks;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;

public class KeypadDoorActivator : BaseDoorActivator
{
    [Header("Keypad Specific")]
    public GameObject[] keypadNumberVisuals;
    public GameObject keypadEnterVisual;
    public GameObject keypadClearVisual;

    [Header("Script References")]
    public Player player;
    public KeypadDoorVisual keypadTweener;
    public KeypadDoorController targetDoorController;

    [Header("Cinemachine/Camera Control")]
    public CinemachineCamera keypadCamera;
    public float cameraTransitionDuration = 1f;

    [Header("FMOD Events")]
    public EventReference keypadSoundEvent;
    public EventReference keypadFailSoundEvent;
    public EventReference buttonPressSoundEvent;

    private string _currentInput = "";
    private float _previousTransitionTime = 0f;
    private bool _isProcessing = false;

    protected override void Start()
    {
        base.Start();

        player = player != null ? player : Player.Instance;
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

        if (GameManager.Instance != null && GameManager.Instance.HasDisableControlsRequest(this))
        {
            ForceExitKeypad();
        }
    }

    public override Transform GetTransform()
    {
        return activatorCollider.transform;
    }

    public override void Interact()
    {
        if (targetDoorController == null || _isProcessing || targetDoorController.currentState == KeypadDoorController.DoorState.Broken)
        {
            RuntimeManager.PlayOneShot(keypadFailSoundEvent, transform.position);
            return;
        }

        if (targetDoorController.locked)
        {
            FMODHelper.PlayOneShot3D(keypadFailSoundEvent, transform.position);
            return;
        }

        _isProcessing = true;
        _currentInput = "";

        player.playerInputs.DisableGameplayInputs();
        player.playerInputs.DisableUIInputs();
        player.playerInputs.EnableKeypadInputs();

        Player.Instance.playerController.ForceRotate(keypadCamera.transform.rotation.eulerAngles);

        GameManager.Instance.RequestDisableControls(this, shouldDisable: true);
        GameManager.Instance.UpdateCursorVisiblity(forceDisable: true);

        _previousTransitionTime = CameraManager.Instance.cameraBrain.DefaultBlend.Time;
        CameraManager.Instance.cameraBrain.DefaultBlend.Time = cameraTransitionDuration;
        keypadCamera.Priority = 100;
        keypadCamera.enabled = true;

        keypadTweener.ToggleLogo(false);
        keypadTweener.ToggleText(true);
        keypadTweener.ChangeScreenText("");
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
                Log.Warning($"Keypad visual for number {number} is missing from the array.");
            }
        }
    }

    private async UniTask AppendInput(char number, GameObject buttonVisual)
    {
        if (_currentInput.Length < targetDoorController.maxCodeLength)
        {
            _currentInput += number;
            FMODHelper.PlayOneShot3D(buttonPressSoundEvent, transform.position);
            keypadTweener.ChangeScreenText(_currentInput);
            await keypadTweener.PlayNumberKeyTween(buttonVisual);
        }
    }

    private void RemoveLastInput()
    {
        keypadTweener.PlayClearKeyTween().Forget();

        FMODHelper.PlayOneShot3D(buttonPressSoundEvent, transform.position);

        if (_currentInput.Length > 0)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
            keypadTweener.ChangeScreenText(_currentInput);
        }
    }

    private async UniTask CheckCode(string input)
    {
        player.playerInputs.DisableKeypadInputs();

        keypadTweener.PlayEnterKeyTween().Forget();

        FMODHelper.PlayOneShot3D(buttonPressSoundEvent, transform.position);

        bool success = input == targetDoorController.correctCode;

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

        targetDoorController.UpdateActivatorVisuals(success, "");

        await UniTask.WaitForSeconds(targetDoorController.codeResetDelay, ignoreTimeScale: false);

        _isProcessing = false;

        player.playerInputs.DisableKeypadInputs();
        player.playerInputs.EnableUIInputs();
        player.playerInputs.EnableGameplayInputs();

        await ResetPlayerCamera();
    }

    public void ForceExitKeypad()
    {
        _isProcessing = false;

        player.playerInputs.DisableKeypadInputs();
        player.playerInputs.EnableUIInputs();
        player.playerInputs.EnableGameplayInputs();

        ResetPlayerCamera(wasForceExit: true).Forget();
    }

    public async UniTask ResetPlayerCamera(bool? wasForceExit = false)
    {
        keypadCamera.Priority = -1;
        keypadCamera.enabled = false;

        GameManager.Instance.RequestDisableControls(this, shouldDisable: false);

        if (wasForceExit == false)
        {
            keypadTweener.ToggleLogo(true);
            keypadTweener.ToggleText(false);
            keypadTweener.ChangeScreenColor(targetDoorController.defaultColor, true, 0.8f);
            _currentInput = "";
        }

        await UniTask.WaitForSeconds(cameraTransitionDuration, ignoreTimeScale: false);
        CameraManager.Instance.cameraBrain.DefaultBlend.Time = _previousTransitionTime;
        _previousTransitionTime = 0f;
    }

    public async UniTask ResetButtonDisplay()
    {
        await UniTask.WaitForSeconds(1.6f, ignoreTimeScale: false);

        keypadTweener.ToggleLogo(true);
        keypadTweener.ToggleText(false);
        keypadTweener.ChangeScreenColor(targetDoorController.defaultColor, true, 0.8f);

        await UniTask.WaitForSeconds(0.6f, ignoreTimeScale: false);

        SetButtonState(true);
    }

    public override void StartPulseEffect(Color startColor, float? customDuration = null, float? customIntensity = null)
    {
        if (keypadTweener != null)
        {
            keypadTweener.StartPulse(startColor, customDuration, customIntensity);
        }
    }

    public override void StopPulseEffect()
    {
        if (keypadTweener != null)
        {
            keypadTweener.StopPulse();
        }
    }

    public void TransitionToPulseEffect(Color targetColor, float transitionDuration, float pulseDuration, float pulseIntensity)
    {
        if (keypadTweener != null)
        {
            keypadTweener.TransitionToPulse(targetColor, transitionDuration, pulseDuration, pulseIntensity);
        }
    }
}