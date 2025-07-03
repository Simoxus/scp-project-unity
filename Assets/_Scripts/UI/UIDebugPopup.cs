using UnityEngine;
using UnityEngine.InputSystem;

public class UIDebugPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIAccess uiAccess;
    [SerializeField] private GameObject debugCanvas;

    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    public string actionMapName = "Debug";
    public string toggleActionName = "Popup";

    private InputAction _toggleAction;
    private InputActionMap _currentActionMap;

    void Awake()
    {
        _currentActionMap = inputActions.FindActionMap(actionMapName);
        if (_currentActionMap == null)
        {
            enabled = false;
            return;
        }

        _toggleAction = _currentActionMap.FindAction(toggleActionName);
        if (_toggleAction == null)
        {
            enabled = false;
            return;
        }

        debugCanvas.SetActive(false);
    }

    void OnEnable()
    {
        if (_toggleAction != null)
        {
            _toggleAction.performed += OnTogglePerformed;
            _currentActionMap.Enable();
        }
    }

    void OnDisable()
    {
        if (_toggleAction != null)
        {
            _toggleAction.performed -= OnTogglePerformed;
            _currentActionMap.Disable();
        }
    }

    private void OnTogglePerformed(InputAction.CallbackContext context)
    {
        if (context.action == _toggleAction)
        {
            // Determine if the console is currently active or not
            bool canvasIsActive = debugCanvas.activeSelf;

            if (!canvasIsActive) // Console is currently inactive, so we want to open it
            {
                debugCanvas.SetActive(true); // Activate the UI
                uiAccess.consoleUI.FocusOnInput(); // Focus on the input field
                GameManager.Instance.RequestPause();
            }
            else // Console is currently active, so we want to close it
            {
                debugCanvas.SetActive(false);
                GameManager.Instance.ReleasePause();
            }
        }
    }
}