using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerFreecam : MonoBehaviour
{
    [Space]
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float sprintMultiplier = 5.0f;
    [SerializeField] private float decelerateMultiplier = 0.2f;
    [SerializeField] private float moveAcceleration = 5.0f;
    [SerializeField] private float moveDamping = 8.0f;

    [Header("Camera Settings")]
    [SerializeField] private float minFOV = 10.0f;
    [SerializeField] private float maxFOV = 150.0f;
    [SerializeField] private float fovScrollSensitivity = 5.0f;
    [SerializeField] private float fovLerpSpeed = 10.0f;
    [SerializeField] private float fovLerpSpeedSmooth = 5.0f;
    [SerializeField] private float rotationEasingSpeed = 10.0f;

    private Player _player;
    private Camera _freecamCamera;
    private GameObject _freecamObject;
    private StudioListener _studioListener;
    private CameraWobble _cameraWobbler;

    private bool _isFreecamActive = false;
    private bool _cameraLocked = false;
    private bool _cursorLocked = false;
    private bool _smoothEnabled = true;
    private bool _wobbleEnabled = false;
    private float _currentFOV;
    private float _targetFOV;

    private Vector2 _currentLook;
    private Vector2 _currentLookVelocity;
    private Vector3 _currentVelocity = Vector3.zero;
    private Vector3 _targetVelocity = Vector3.zero;
    private Vector2 _targetRotation = Vector2.zero;
    private Vector2 _currentRotation = Vector2.zero;

    private Transform _originalCullingTransform;

    public bool IsFreecamActive => _isFreecamActive;
    public bool IsCameraLocked => _cameraLocked;
    public bool IsSmoothEnabled => _smoothEnabled;
    public bool IsWobbleEnabled => _cameraWobbler != null && _cameraWobbler.enabled;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        if (_player?.Inputs != null)
        {
            _player.Inputs.OnFreecamLock += HandleFreecamLock;
            _player.Inputs.OnFreecamWobble += HandleFreecamWobble;
            _player.Inputs.OnFreecamPause += HandleFreecamPause;
            _player.Inputs.OnFreecamTutorial += HandleToggleTutorial;
            _player.Inputs.OnFreecamSmooth += HandleFreecamSmooth;
        }
    }

    private void OnDisable()
    {
        if (_player?.Inputs != null)
        {
            _player.Inputs.OnFreecamLock -= HandleFreecamLock;
            _player.Inputs.OnFreecamWobble -= HandleFreecamWobble;
            _player.Inputs.OnFreecamPause -= HandleFreecamPause;
            _player.Inputs.OnFreecamTutorial -= HandleToggleTutorial;
            _player.Inputs.OnFreecamSmooth -= HandleFreecamSmooth;
        }

        if (_isFreecamActive && Core.GameManager != null)
        {
            Core.GameManager.ReleaseCursorControl(this);
        }

        if (_isFreecamActive && _originalCullingTransform != null)
        {
            RestoreCullingTransform();
        }
    }

    private void Update()
    {
        if (!_isFreecamActive || _freecamCamera == null) return;

        GameManager gameManager = Core.GameManager;
        bool isPausedByFreecam = gameManager.HasPauseRequest(this) && gameManager.pauseRequestCount == 1;
        bool shouldLockCursor = !gameManager.gamePaused || isPausedByFreecam;

        if (shouldLockCursor != _cursorLocked)
        {
            _cursorLocked = shouldLockCursor;
            gameManager.SetCursorState(this, visible: !shouldLockCursor,
                lockMode: shouldLockCursor ? CursorLockMode.Locked : CursorLockMode.None);
        }

        if (!shouldLockCursor) return;

        HandleFreecamMovement();
        HandleFreecamRotation();
        HandleFreecamFOV();
        UpdateFOVLerp();
        SyncCameraSettings();
    }

    private void HandleFreecamLock()
    {
        if (_freecamCamera != null)
        {
            _cameraLocked = !_cameraLocked;
        }
    }

    private void HandleFreecamWobble()
    {
        if (_freecamCamera != null && _cameraWobbler != null)
        {
            _cameraWobbler.enabled = !_cameraWobbler.enabled;
            _wobbleEnabled = _cameraWobbler.enabled;
        }
    }

    private void HandleToggleTutorial()
    {
        if (Core.UI?.Tutorials?.freecamHintPanel != null && _isFreecamActive)
        {
            bool isCurrentlyVisible = Core.UI.Tutorials.freecamHintPanel.alpha > 0f;
            if (isCurrentlyVisible)
            {
                Core.UI.Tutorials.HideFreecamHints();
            }
            else
            {
                Core.UI.Tutorials.ShowFreecamHints();
            }
        }
    }

    private void HandleFreecamMovement()
    {
        Vector2 moveInput = _player.Inputs.FreecamMoveInput;
        float yInput = 0f;

        if (_player.Inputs.FreecamUpHeld) yInput = 1f;
        if (_player.Inputs.FreecamDownHeld) yInput = -1f;

        float currentMoveSpeed = moveSpeed;
        if (_player.Inputs.FreecamAccelerateHeld)
            currentMoveSpeed *= sprintMultiplier;
        else if (_player.Inputs.FreecamDecelerateHeld)
            currentMoveSpeed *= decelerateMultiplier;

        if (_smoothEnabled)
        {
            Vector3 targetDirection = (_freecamCamera.transform.forward * moveInput.y +
                                       _freecamCamera.transform.right * moveInput.x +
                                       Vector3.up * yInput).normalized;

            float inputMagnitude = new Vector3(moveInput.x, yInput, moveInput.y).magnitude;
            inputMagnitude = Mathf.Clamp01(inputMagnitude);

            _targetVelocity = targetDirection * currentMoveSpeed * inputMagnitude;

            _currentVelocity = Vector3.Lerp(_currentVelocity, _targetVelocity,
                Time.unscaledDeltaTime * (inputMagnitude > 0 ? moveAcceleration : moveDamping));

            _freecamCamera.transform.position += _currentVelocity * Time.unscaledDeltaTime;
        }
        else
        {
            if (moveInput.sqrMagnitude == 0f && yInput == 0f) return;

            currentMoveSpeed *= Time.unscaledDeltaTime;

            _freecamCamera.transform.position += _freecamCamera.transform.forward * currentMoveSpeed * moveInput.y;
            _freecamCamera.transform.position += _freecamCamera.transform.right * currentMoveSpeed * moveInput.x;
            _freecamCamera.transform.position += Vector3.up * currentMoveSpeed * yInput;
        }
    }

    private void HandleFreecamRotation()
    {
        if (_cameraLocked) return;
        if (_player.Inputs.FreecamZoomModifierHeld) return;

        Vector2 lookInput = _player.Inputs.FreecamLookInput * _player.Controller.LookSpeed;

        if (!_player.Controller.InvertYAxis)
        {
            lookInput.y = -lookInput.y;
        }

        Vector2 processedLook;
        if (_player.Controller.SmoothLook)
        {
            processedLook = Vector2.SmoothDamp(
                _currentLook,
                lookInput,
                ref _currentLookVelocity,
                _player.Controller.LookSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
            _currentLook = processedLook;
        }
        else
        {
            processedLook = lookInput;
        }

        if (processedLook.x == 0.0f && processedLook.y == 0.0f) return;

        if (_smoothEnabled)
        {
            _targetRotation.y += processedLook.x;
            _targetRotation.x -= processedLook.y;
            _targetRotation.x = Mathf.Clamp(_targetRotation.x, -90f, 90f);

            float t = Time.unscaledDeltaTime * rotationEasingSpeed;
            _currentRotation.x = Mathf.Lerp(_currentRotation.x, _targetRotation.x, t);
            _currentRotation.y = Mathf.LerpAngle(_currentRotation.y, _targetRotation.y, t);

            _freecamCamera.transform.localRotation = Quaternion.Euler(_currentRotation.x, _currentRotation.y, 0f);
        }
        else
        {
            float rotationX = _freecamCamera.transform.localEulerAngles.x;
            float newRotationY = _freecamCamera.transform.localEulerAngles.y + processedLook.x;

            float newRotationX = (rotationX - processedLook.y);
            if (rotationX <= 90.0f && newRotationX >= 0.0f)
                newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
            if (rotationX >= 270.0f)
                newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

            _freecamCamera.transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, _freecamCamera.transform.localEulerAngles.z);
        }
    }

    private void HandleFreecamFOV()
    {
        float zoomInput = _player.Inputs.FreecamZoomInput;
        if (zoomInput == 0f) return;

        if (Gamepad.current != null && Mouse.current != null && Gamepad.current.lastUpdateTime > Mouse.current.lastUpdateTime)
        {
            if (!_player.Inputs.FreecamZoomModifierHeld)
                return;
        }

        _targetFOV -= zoomInput * fovScrollSensitivity;
        _targetFOV = Mathf.Clamp(_targetFOV, minFOV, maxFOV);
    }

    private void UpdateFOVLerp()
    {
        float fovSpeed = _smoothEnabled ? fovLerpSpeedSmooth : fovLerpSpeed;
        _currentFOV = Mathf.Lerp(_currentFOV, _targetFOV, Time.unscaledDeltaTime * fovSpeed);
        _freecamCamera.fieldOfView = _currentFOV;
    }

    private void HandleFreecamPause()
    {
        if (!_isFreecamActive) return;

        if (Core.GameManager != null)
        {
            if (Core.GameManager.HasPauseRequest(this))
            {
                Core.GameManager.ReleasePause(this);
            }
            else
            {
                Core.GameManager.RequestPause(this);
            }
        }
    }

    private void HandleFreecamSmooth()
    {
        if (_freecamCamera != null)
        {
            _smoothEnabled = !_smoothEnabled;

            if (_smoothEnabled)
            {
                Vector3 currentEuler = _freecamCamera.transform.localEulerAngles;
                float x = currentEuler.x > 180f ? currentEuler.x - 360f : currentEuler.x;
                float y = currentEuler.y > 180f ? currentEuler.y - 360f : currentEuler.y;
                _currentRotation = new Vector2(x, y);
                _targetRotation = _currentRotation;

                _currentVelocity = Vector3.zero;
                _targetVelocity = Vector3.zero;
            }
        }
    }

    private void SyncCameraSettings()
    {
        if (_player.CameraBrain != null && _freecamCamera != null)
        {
            _freecamCamera.nearClipPlane = _player.CameraMain.Lens.NearClipPlane;
            _freecamCamera.farClipPlane = _player.CameraMain.Lens.FarClipPlane;
        }
    }

    public bool ToggleFreecam()
    {
        _isFreecamActive = !_isFreecamActive;

        if (_isFreecamActive)
        {
            EnableFreecam();
        }
        else
        {
            DisableFreecam();
        }

        return _isFreecamActive;
    }

    private void EnableFreecam()
    {
        _player.CameraBrainStudioListener.enabled = false;

        _freecamObject = new GameObject("FreecamCamera");
        _freecamCamera = _freecamObject.AddComponent<Camera>();
        _studioListener = _freecamObject.AddComponent<StudioListener>();

        _cameraWobbler = _freecamObject.AddComponent<CameraWobble>();
        _cameraWobbler.enabled = _wobbleEnabled;

        if (_player.CameraBrain != null)
        {
            _currentFOV = _player.CameraBrain.fieldOfView;
            _targetFOV = _currentFOV;
            _freecamCamera.fieldOfView = _currentFOV;
            _freecamCamera.nearClipPlane = _player.CameraBrain.nearClipPlane;
            _freecamCamera.farClipPlane = _player.CameraBrain.farClipPlane;
            _freecamCamera.clearFlags = _player.CameraBrain.clearFlags;
            _freecamCamera.backgroundColor = _player.CameraBrain.backgroundColor;
            _freecamCamera.useOcclusionCulling = false;
            _freecamCamera.GetUniversalAdditionalCameraData().renderPostProcessing = _player.CameraBrain.GetUniversalAdditionalCameraData().renderPostProcessing;
            _freecamCamera.SetVolumeFrameworkUpdateMode(VolumeFrameworkUpdateMode.EveryFrame);

            _freecamCamera.transform.position = _player.CameraBrain.transform.position;
            _freecamCamera.transform.rotation = _player.CameraBrain.transform.rotation;
        }

        _currentLook = Vector2.zero;
        _currentLookVelocity = Vector2.zero;
        _cameraLocked = false;
        _cursorLocked = false;

        if (Core.GameManager != null)
        {
            Core.GameManager.RequestDisableControls(this, shouldDisable: true);
            Core.GameManager.RequestCursorControl(this);
            Core.GameManager.SetCursorState(this, visible: false, CursorLockMode.Locked);
        }

        _player.CameraMain.enabled = false;
        _player.CameraBrain.enabled = false;

        if (Core.UI?.Tutorials != null)
        {
            Core.UI.Tutorials.ShowFreecamHints();
            Core.UI.Tutorials.UpdateFreecamHints();
        }

        SetupCullingForFreecam();
    }

    private void DisableFreecam()
    {
        _player.CameraBrainStudioListener.enabled = true;

        if (_freecamObject != null)
        {
            Destroy(_freecamObject);
            _freecamCamera = null;
            _freecamObject = null;
            _studioListener = null;
            _cameraWobbler = null;
        }

        _cameraLocked = false;
        _cursorLocked = false;

        _player.CameraMain.enabled = true;
        _player.CameraBrain.enabled = true;

        if (Core.GameManager != null)
        {
            Core.GameManager.RequestDisableControls(this, shouldDisable: false);
            Core.GameManager.ReleaseCursorControl(this);
            Core.GameManager.ReleasePause(this);
        }

        if (Core.UI?.Tutorials != null)
        {
            Core.UI.Tutorials.HideFreecamHints();
        }

        RestoreCullingTransform();
    }

    private void SetupCullingForFreecam()
    {
        if (Core.CullingSystem != null && _freecamCamera != null)
        {
            _originalCullingTransform = Core.CullingSystem.CullingOrigin;
            Core.CullingSystem.CullingOrigin = _freecamCamera.transform;
            Core.CullingSystem.ForceUpdate();
        }
    }

    private void RestoreCullingTransform()
    {
        if (Core.CullingSystem != null && _originalCullingTransform != null)
        {
            Core.CullingSystem.CullingOrigin = _originalCullingTransform;
            Core.CullingSystem.ForceUpdate();
            _originalCullingTransform = null;
        }
    }
}