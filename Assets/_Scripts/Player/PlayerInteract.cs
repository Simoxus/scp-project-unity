using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class PlayerInteract : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Camera cameraBrain;

    [Header("UI Settings")]
    [SerializeField] private bool useFading = true;
    [SerializeField] private float borderPadding = 50f;
    [SerializeField] private float fadeSpeed = 10f;
    [Range(0f, 180f), SerializeField] private float onScreenFrustumAngle = 60f;

    [Header("On-Screen UI")]
    [SerializeField] private InteractionUI onScreenInteractionUI;

    [Header("Off-Screen UI")]
    [SerializeField] private InteractionUI offScreenInteractionUI;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float fadeStartDistance = 2f;
    [SerializeField] private float uiHeightOffset = 0f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Icon Tween Settings")]
    [SerializeField] private Ease easingStyle1 = Ease.InSine;
    [SerializeField] private Ease easingStyle2 = Ease.OutSine;
    [SerializeField] private float onScreenTweenScale1 = 0.14f;
    [SerializeField] private float onScreenTweenScale2 = 0.17f;
    [SerializeField] private float offScreenTweenScale1 = 0.65f;
    [SerializeField] private float offScreenTweenScale2 = 0.85f;
    [SerializeField] private float tweenDuration1 = 0.12f;
    [SerializeField] private float tweenDuration2 = 0.08f;
    [SerializeField] private float delayBetweenTweens = 0.121f;

    [SerializeField] private InteractionSpriteData[] interactionSpriteData;
    private Dictionary<string, Sprite> _interactionSprites;
    private IInteractable _currentTarget;
    private Transform _currentTargetTransform;

    private Collider[] _interactableColliders = new Collider[10]; // Pre-allocated array for Physics.OverlapSphereNonAlloc

    [System.Serializable]
    public class InteractionUI
    {
        public RectTransform uiTransform;
        public Image iconImage;
        public CanvasGroup canvasGroup;

        private bool _wasIconActive = false; // To track active state and avoid redundant SetActive calls

        public void SetIcon(Sprite sprite, float alpha)
        {
            if (iconImage == null) return;

            iconImage.sprite = sprite;
            Color currentColor = iconImage.color;
            currentColor.a = alpha;
            iconImage.color = currentColor;

            bool shouldBeActive = sprite != null && alpha > 0.001f;
            if (iconImage.gameObject.activeSelf != shouldBeActive)
            {
                iconImage.gameObject.SetActive(shouldBeActive);
            }
            _wasIconActive = shouldBeActive;
        }

        public void UpdateCanvasGroup(float targetAlpha, float lerpSpeed)
        {
            if (canvasGroup == null || uiTransform == null) return;

            bool shouldBeUIActive = targetAlpha > 0.001f;
            if (uiTransform.gameObject.activeSelf != shouldBeUIActive)
            {
                uiTransform.gameObject.SetActive(shouldBeUIActive);
            }

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * lerpSpeed);
            bool isActiveForInteraction = canvasGroup.alpha > 0.01f;
            canvasGroup.blocksRaycasts = isActiveForInteraction;
            canvasGroup.interactable = isActiveForInteraction;
        }

        public void SetCanvasGroupDirect(float alpha, bool interactable)
        {
            if (canvasGroup == null || uiTransform == null) return;

            bool shouldBeUIActive = alpha > 0.001f;
            if (uiTransform.gameObject.activeSelf != shouldBeUIActive)
            {
                uiTransform.gameObject.SetActive(shouldBeUIActive);
            }

            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = interactable;
            canvasGroup.interactable = interactable;
        }

        public void HideUI()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            if (uiTransform != null && uiTransform.gameObject.activeSelf) // Avoid redundant SetActive
            {
                uiTransform.gameObject.SetActive(false);
            }
            if (iconImage != null)
            {
                iconImage.sprite = null;
                if (iconImage.gameObject.activeSelf) // Avoid redundant SetActive
                {
                    iconImage.gameObject.SetActive(false);
                }
            }
            _wasIconActive = false;
        }
    }

    [System.Serializable]
    public struct InteractionSpriteData
    {
        public string type;
        public Sprite sprite;
    }

    private void OnEnable()
    {
        /*
        BasicInteract.OnObjectInteracted += HandleObjectInteracted;
        DoorActivator.OnObjectInteracted += HandleObjectInteracted;
        */

        // Subscribe to the PlayerInputs' interact event
        if (player != null && player.playerInputs != null)
        {
            player.playerInputs.OnInteractPressed += CheckInteractionInput;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the PlayerInputs' interact event
        if (player != null && player.playerInputs != null)
        {
            player.playerInputs.OnInteractPressed -= CheckInteractionInput;
        }

        HideAllUI();
    }

    private void HideAllUI()
    {
        // Ensure that both on-screen and off-screen UI elements are immediately
        // hidden and their icons are cleared.
        onScreenInteractionUI.HideUI();
        offScreenInteractionUI.HideUI();

        // Reset the current target to null
        _currentTarget = null;
        _currentTargetTransform = null;
    }

    // This method now directly calls the local tweening logic
    private async void HandleObjectInteracted()
    {
        await SizeInteractIcons();
    }

    private void Awake()
    {
        // Check for player and if there's no player, try to find the singleton/instance
        player = player != null ? player : Player.Instance;

        InitializeInteractionSprites();
        // It's generally good practice to set up references here if possible
        // If cameraBrain is not set in inspector, try to find it here instead of Start.
        if (cameraBrain == null)
        {
            SetupCamera(); // Try to get Camera.main early
        }
    }

    private void Start()
    {
        // If cameraBrain was already set in Awake or Inspector, this is skipped.
        if (cameraBrain == null)
        {
            SetupCamera();
        }
        InitializeUIState();
    }

    private void Update()
    {
        FindInteractable();
        UpdateInteractionUI();
    }

    // Private Helper Methods

    private void InitializeInteractionSprites()
    {
        _interactionSprites = new Dictionary<string, Sprite>();
        foreach (var data in interactionSpriteData)
        {
            if (data.sprite != null && !string.IsNullOrEmpty(data.type))
            {
                _interactionSprites[data.type.ToLowerInvariant()] = data.sprite; // Using InvariantCulture for consistency
            }
        }
    }

    private void SetupCamera()
    {
        cameraBrain = Camera.main;
        if (cameraBrain == null)
        {
            Debug.LogError("No camera has been assigned to PlayerInteract and Camera.main not found. Disabling script.", this);
            enabled = false;
        }
    }

    private void InitializeUIState()
    {
        if (useFading)
        {
            onScreenInteractionUI.HideUI();
            offScreenInteractionUI.HideUI();
        }
        else
        {
            // Only set active state if currently different
            if (onScreenInteractionUI.uiTransform != null && onScreenInteractionUI.uiTransform.gameObject.activeSelf)
            {
                onScreenInteractionUI.uiTransform.gameObject.SetActive(false);
            }
            if (offScreenInteractionUI.uiTransform != null && offScreenInteractionUI.uiTransform.gameObject.activeSelf)
            {
                offScreenInteractionUI.uiTransform.gameObject.SetActive(false);
            }
            onScreenInteractionUI.SetCanvasGroupDirect(0f, false);
            offScreenInteractionUI.SetCanvasGroupDirect(0f, false);
        }
    }

    private void FindInteractable()
    {
        _currentTarget = null;
        _currentTargetTransform = null;

        // Use Physics.OverlapSphereNonAlloc to avoid garbage creation
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, interactionRange, _interactableColliders, interactableLayer);

        float closestDistanceSqr = Mathf.Infinity;
        IInteractable potentialTarget = null;
        Transform potentialTargetTransform = null;

        for (int i = 0; i < numColliders; i++)
        {
            var hitCollider = _interactableColliders[i];
            if (hitCollider.TryGetComponent(out IInteractable interactable))
            {
                float distanceSqr = (transform.position - hitCollider.transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    potentialTarget = interactable;
                    potentialTargetTransform = hitCollider.transform;
                }
            }
        }

        _currentTarget = potentialTarget;
        _currentTargetTransform = potentialTargetTransform;
    }

    private void UpdateInteractionUI()
    {
        if (_currentTarget == null || _currentTargetTransform == null || cameraBrain == null)
        {
            HandleNoCurrentTarget();
            return;
        }

        float distance = Vector3.Distance(transform.position, _currentTargetTransform.position);
        float calculatedAlpha = Mathf.Clamp01(Mathf.InverseLerp(interactionRange, fadeStartDistance, distance));

        Vector3 directionToTarget = (_currentTargetTransform.position - cameraBrain.transform.position).normalized;
        float angleToTarget = Vector3.Angle(cameraBrain.transform.forward, directionToTarget);
        bool isInCustomFrustum = angleToTarget < (onScreenFrustumAngle / 2f);

        Vector3 viewportPoint = cameraBrain.WorldToViewportPoint(_currentTargetTransform.position);
        bool isGenerallyOnScreen = viewportPoint.z > 0 &&
                                   viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                                   viewportPoint.y >= 0 && viewportPoint.y <= 1;

        bool isOnScreen = isGenerallyOnScreen && isInCustomFrustum;
        string interactionType = _currentTarget.GetInteractionType(); // Get once

        if (isOnScreen)
        {
            HandleOnScreenUI(calculatedAlpha, interactionType);
            offScreenInteractionUI.HideUI();
        }
        else
        {
            HandleOffScreenUI(calculatedAlpha, viewportPoint, interactionType);
            onScreenInteractionUI.HideUI();
        }
    }

    private void HandleNoCurrentTarget()
    {
        if (useFading)
        {
            onScreenInteractionUI.UpdateCanvasGroup(0f, fadeSpeed);
            offScreenInteractionUI.UpdateCanvasGroup(0f, fadeSpeed);
            onScreenInteractionUI.SetIcon(null, 0f); // Ensure icon is cleared
            offScreenInteractionUI.SetIcon(null, 0f); // Ensure icon is cleared
        }
        else
        {
            onScreenInteractionUI.HideUI();
            offScreenInteractionUI.HideUI();
        }
    }

    private void HandleOnScreenUI(float targetAlpha, string interactionType)
    {
        if (onScreenInteractionUI.uiTransform == null) return;

        if (useFading)
        {
            onScreenInteractionUI.UpdateCanvasGroup(targetAlpha, fadeSpeed);
            SetInteractionIcon(onScreenInteractionUI, interactionType, targetAlpha);
        }
        else
        {
            onScreenInteractionUI.SetCanvasGroupDirect(1f, true);
            SetInteractionIcon(onScreenInteractionUI, interactionType, 1f);
        }

        onScreenInteractionUI.uiTransform.position = _currentTargetTransform.position + Vector3.up * uiHeightOffset;
        onScreenInteractionUI.uiTransform.LookAt(onScreenInteractionUI.uiTransform.position + cameraBrain.transform.rotation * Vector3.forward, cameraBrain.transform.rotation * Vector3.up);
    }

    private void HandleOffScreenUI(float targetAlpha, Vector3 viewportPoint, string interactionType)
    {
        if (offScreenInteractionUI.uiTransform == null) return;

        if (useFading)
        {
            offScreenInteractionUI.UpdateCanvasGroup(targetAlpha, fadeSpeed);
            SetInteractionIcon(offScreenInteractionUI, interactionType, targetAlpha);
        }
        else
        {
            offScreenInteractionUI.SetCanvasGroupDirect(1f, true);
            SetInteractionIcon(offScreenInteractionUI, interactionType, 1f);
        }

        // Calculate off-screen indicator position and rotation
        Vector2 screenPoint = cameraBrain.WorldToScreenPoint(_currentTargetTransform.position);
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 direction = new Vector2(screenPoint.x - screenCenter.x, screenPoint.y - screenCenter.y);
        direction.Normalize();

        float halfScreenWidth = Screen.width / 2f - borderPadding;
        float halfScreenHeight = Screen.height / 2f - borderPadding;
        float angle = Mathf.Atan2(direction.y, direction.x);
        float tanAngle = Mathf.Tan(angle);
        float indicatorX, indicatorY;

        // Prevent division by zero if tanAngle is extremely close to 0
        if (Mathf.Abs(tanAngle) < float.Epsilon && Mathf.Abs(direction.x) > float.Epsilon) // if horizontal and not precisely at center
        {
            indicatorX = Mathf.Sign(direction.x) * halfScreenWidth;
            indicatorY = 0;
        }
        else if (Mathf.Abs(tanAngle) > (halfScreenHeight / halfScreenWidth)) // If it hits vertical border first
        {
            indicatorY = Mathf.Sign(direction.y) * halfScreenHeight;
            indicatorX = indicatorY / tanAngle;
        }
        else // Hits horizontal border first
        {
            indicatorX = Mathf.Sign(direction.x) * halfScreenWidth;
            indicatorY = tanAngle * indicatorX;
        }

        if (viewportPoint.z < 0)
        {
            indicatorX *= -1;
            indicatorY *= -1;
        }

        float rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        offScreenInteractionUI.uiTransform.position = screenCenter + new Vector2(indicatorX, indicatorY);
        offScreenInteractionUI.uiTransform.rotation = Quaternion.Euler(0, 0, rotationAngle - 90);
    }

    private void SetInteractionIcon(InteractionUI interactionUI, string type, float targetAlpha)
    {
        if (interactionUI.iconImage == null) return;

        Sprite iconSprite;
        if (_interactionSprites.TryGetValue(type.ToLowerInvariant(), out iconSprite)) // Using InvariantCulture
        {
            interactionUI.SetIcon(iconSprite, targetAlpha);
        }
        else
        {
            interactionUI.SetIcon(null, 0f);
        }
    }

    private void CheckInteractionInput()
    {
        if (_currentTarget != null)
        {
            _currentTarget.Interact();
            HandleObjectInteracted();
        }
    }

    private async UniTask SizeInteractIcons()
    {
        // Safety check: Ensure icon RectTransforms exist
        if (onScreenInteractionUI?.iconImage?.rectTransform == null || offScreenInteractionUI?.iconImage?.rectTransform == null)
        {
            Debug.LogWarning("Cannot size interact icons: one or both icon RectTransforms are null.");
            return;
        }

        // Ensure the icon GameObjects are active BEFORE tweening them if they were not already.
        // The InteractionUI.SetIcon method now handles this more robustly.

        // Store the tweens
        var onScreenTween1 = Tween.Scale(onScreenInteractionUI.iconImage.rectTransform, onScreenTweenScale1, tweenDuration1, easingStyle1)
            .ToYieldInstruction().ToUniTask();
        var offScreenTween1 = Tween.Scale(offScreenInteractionUI.iconImage.rectTransform, offScreenTweenScale1, tweenDuration1, easingStyle1)
            .ToYieldInstruction().ToUniTask();

        // Await both tweens concurrently
        await UniTask.WhenAll(onScreenTween1, offScreenTween1);

        await UniTask.WaitForSeconds(delayBetweenTweens, ignoreTimeScale: false);

        var onScreenTween2 = Tween.Scale(onScreenInteractionUI.iconImage.rectTransform, onScreenTweenScale2, tweenDuration2, easingStyle2)
            .ToYieldInstruction().ToUniTask();
        var offScreenTween2 = Tween.Scale(offScreenInteractionUI.iconImage.rectTransform, offScreenTweenScale2, tweenDuration2, easingStyle2)
            .ToYieldInstruction().ToUniTask();

        await UniTask.WhenAll(onScreenTween2, offScreenTween2);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw the custom frustum angle in the editor
        if (cameraBrain != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 cameraPos = cameraBrain.transform.position;
            Vector3 cameraForward = cameraBrain.transform.forward;

            // Calculate the half angle in radians
            float halfAngleRad = onScreenFrustumAngle * 0.5f * Mathf.Deg2Rad;

            // Calculate the far distance for the frustum visualization
            float frustumDisplayDistance = interactionRange + 1f; // A bit beyond interaction range

            // Get the direction vectors for the edges of the frustum
            Quaternion leftRotation = Quaternion.AngleAxis(-onScreenFrustumAngle / 2f, cameraBrain.transform.up);
            Quaternion rightRotation = Quaternion.AngleAxis(onScreenFrustumAngle / 2f, cameraBrain.transform.up);

            Vector3 leftRayDirection = leftRotation * cameraForward;
            Vector3 rightRayDirection = rightRotation * cameraForward;

            // Draw lines representing the frustum edges
            Gizmos.DrawLine(cameraPos, cameraPos + leftRayDirection * frustumDisplayDistance);
            Gizmos.DrawLine(cameraPos, cameraPos + rightRayDirection * frustumDisplayDistance);
        }
    }
}