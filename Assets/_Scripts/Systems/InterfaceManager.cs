using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance { get; private set; }

    [Header("UI-related References")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject playerUIOverlays;
    [SerializeField] private CanvasGroup indicatorsGroup;
    [SerializeField] private CanvasGroup blinkOverlayGroup;

    [Header("Cursor References")]
    public Texture2D normalCursor;
    public Texture2D clickCursor;
    public Vector2 hotspot = Vector2.zero; // pivot point of the cursor

    // Tweens
    private Tween _blinkTween;
    private Tween _hudTween;

    // States
    //private bool _isClickingUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Cursor.SetCursor(normalCursor, hotspot, CursorMode.Auto);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject()) // LMB pressed
        {
            Cursor.SetCursor(clickCursor, hotspot, CursorMode.Auto);
            //_isClickingUI = true;
        }

        if (Input.GetMouseButtonUp(0)) // LMB released
        {
            Cursor.SetCursor(normalCursor, hotspot, CursorMode.Auto);
            //_isClickingUI = false;
        }
    }

    public void TogglePlayerHUD(float duration = 0.8f)
    {
        if (indicatorsGroup == null) return;

        // Toggle the state
        GameManager.Instance.hidePlayerHUD = !GameManager.Instance.hidePlayerHUD;

        // Stop any ongoing tween to prevent jumping
        _hudTween.Stop();

        // Start a new tween based on the new state
        float targetAlpha = GameManager.Instance.hidePlayerHUD ? 0 : 1;
        _hudTween = Tween.Alpha(indicatorsGroup, targetAlpha, duration, Ease.InOutCubic);
    }
}