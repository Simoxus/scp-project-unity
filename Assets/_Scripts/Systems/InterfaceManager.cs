using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject playerUIOverlays;
    [SerializeField] private CanvasGroup indicatorsGroup;
    [SerializeField] private CanvasGroup blinkOverlayGroup;

    [Header("Cursor References")]
    public Texture2D normalCursor;
    public Texture2D clickCursor;
    public Vector2 hotspot = Vector2.zero; // pivot point of the cursor

    // Cached references and state
    private EventSystem _eventSystem;
    private bool _isMouseOverUI;
    private bool _wasMousePressed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Cache EventSystem reference
        _eventSystem = EventSystem.current;
    }

    private void Start()
    {
        InitializeCursor();
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void InitializeCursor()
    {
        if (normalCursor != null)
        {
            Cursor.SetCursor(normalCursor, hotspot, CursorMode.Auto);
        }
    }

    private void HandleMouseInput()
    {
        // Cache mouse button states to avoid multiple Input calls
        bool mouseDown = Input.GetMouseButtonDown(0);
        bool mouseUp = Input.GetMouseButtonUp(0);

        // Only check UI overlay when mouse state changes
        if (mouseDown || mouseUp)
        {
            _isMouseOverUI = _eventSystem != null && _eventSystem.IsPointerOverGameObject();
        }

        // Handle cursor changes
        if (mouseDown && _isMouseOverUI && !_wasMousePressed)
        {
            if (clickCursor != null)
                Cursor.SetCursor(clickCursor, hotspot, CursorMode.Auto);
            _wasMousePressed = true;
        }
        else if (mouseUp && _wasMousePressed)
        {
            if (normalCursor != null)
                Cursor.SetCursor(normalCursor, hotspot, CursorMode.Auto);
            _wasMousePressed = false;
        }
    }
}