using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private InteractionSpriteData[] interactionSpriteData;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Camera cameraBrain;
    [SerializeField] private InteractionUI interactionUI;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Position Settings")]
    [SerializeField] private float baseHorizontalPadding = -200f;
    [SerializeField] private float baseVerticalPadding = -140f;
    [SerializeField] private float referenceAspect = 16f / 9f;
    [SerializeField] private float cornerLerpSpeed = 10f;
    [SerializeField] private float edgeThreshold = 0.05f;

    [Header("Fade Settings")]
    [SerializeField] private bool disableFading = false;
    [SerializeField] private float fadeSpeed = 10f;
    [SerializeField] private float fadeStartDistance = 2f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.3f;
    private const float OFF_SCREEN_FADE = 0.6f;

    [Header("Animation Settings")]
    [SerializeField] private Ease easingStyle1 = Ease.InSine;
    [SerializeField] private Ease easingStyle2 = Ease.OutSine;
    [SerializeField] private float tweenIdleScale = 0.7f;
    [SerializeField] private float tweenDownScale = 0.6f;
    [SerializeField] private float tweenDuration1 = 0.12f;
    [SerializeField] private float tweenDuration2 = 0.08f;
    [SerializeField] private float delayBetweenTweens = 0.121f;

    private Dictionary<string, Sprite> _interactionSprites;
    private IInteractable _currentTarget;
    private Transform _currentTargetTransform;
    private Outline _currentOutline;
    private readonly Collider[] _interactableColliders = new Collider[10];

    // Cached values to avoid a lot of math
    private int _screenWidth;
    private int _screenHeight;
    private float _cachedAspect;
    private float _cachedHorizontalPadding;
    private ScreenBounds _screenBounds;

    private struct ScreenBounds
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
    }

    private void Awake()
    {
        player = player != null ? player : Player.Instance;
        cameraBrain = cameraBrain != null ? cameraBrain : Camera.main;

        BuildSpriteCache();
        CacheScreenValues();
    }

    private void Update()
    {
        // Update cached screen values if resolution changes
        if (_screenWidth != Screen.width || _screenHeight != Screen.height)
        {
            CacheScreenValues();
        }

        FindNearestInteractable();
        UpdateInteractionUI();
    }

    private void OnEnable()
    {
        if (player != null && player.playerInputs != null)
        {
            player.playerInputs.OnInteract += HandleInteraction;
        }

        FindNearestInteractable();
        UpdateOutline();
    }

    private void OnDisable()
    {
        if (player != null && player.playerInputs != null)
        {
            player.playerInputs.OnInteract -= HandleInteraction;
        }

        DisableCurrentOutline();
        interactionUI.Hide();
    }

    private void BuildSpriteCache()
    {
        _interactionSprites = new Dictionary<string, Sprite>();

        if (interactionSpriteData == null) return;

        foreach (var data in interactionSpriteData)
        {
            if (!string.IsNullOrEmpty(data.type) && data.sprite != null)
            {
                _interactionSprites[data.type.ToLower()] = data.sprite;
            }
        }
    }

    private void CacheScreenValues()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        _cachedAspect = (float)_screenWidth / _screenHeight;

        _cachedHorizontalPadding = Mathf.Clamp(
            baseHorizontalPadding * (_cachedAspect / referenceAspect),
            baseHorizontalPadding,
            baseHorizontalPadding * 1.6f
        );

        _screenBounds.minX = _screenWidth * edgeThreshold + _cachedHorizontalPadding;
        _screenBounds.maxX = _screenWidth * (1 - edgeThreshold) - _cachedHorizontalPadding;
        _screenBounds.minY = _screenHeight * edgeThreshold + baseVerticalPadding;
        _screenBounds.maxY = _screenHeight * (1 - edgeThreshold) - baseVerticalPadding;
    }

    private void FindNearestInteractable()
    {
        IInteractable previousTarget = _currentTarget;
        _currentTarget = null;
        _currentTargetTransform = null;

        int count = Physics.OverlapSphereNonAlloc(transform.position, interactionRange, _interactableColliders, interactableLayer);
        float closestDistSqr = float.MaxValue;
        Vector3 playerPos = transform.position;

        for (int i = 0; i < count; i++)
        {
            Collider collider = _interactableColliders[i];
            if (collider == null) continue;
            if (!collider.TryGetComponent(out IInteractable interactable)) continue;

            Vector3 targetPos = collider.transform.position;
            Vector3 directionToTarget = targetPos - playerPos;
            float distSqr = directionToTarget.sqrMagnitude;

            // Check distance first before raycast
            if (distSqr >= closestDistSqr) continue;

            float distance = Mathf.Sqrt(distSqr);

            if (Physics.Raycast(playerPos, directionToTarget / distance, distance, obstacleLayers))
            {
                continue; // Obstacle blocking line of sight
            }

            closestDistSqr = distSqr;
            _currentTarget = interactable;
            _currentTargetTransform = collider.transform;
        }

        if (_currentTarget != previousTarget)
        {
            DisableCurrentOutline();
            UpdateOutline();
        }
    }

    private void UpdateOutline()
    {
        if (_currentTarget == null)
        {
            _currentOutline = null;
            return;
        }

        _currentOutline = _currentTarget.GetOutline();

        if (_currentOutline != null)
        {
            _currentOutline.enabled = true;
        }
    }

    private void DisableCurrentOutline()
    {
        if (_currentOutline != null)
        {
            _currentOutline.enabled = false;
            _currentOutline = null;
        }
    }

    private void UpdateInteractionUI()
    {
        if (_currentTarget == null || cameraBrain == null)
        {
            HideInteractionUI();
            return;
        }

        UIAccess.Instance.crosshair.enabled = false;

        float distSqr = (transform.position - _currentTargetTransform.position).sqrMagnitude;
        float distance = Mathf.Sqrt(distSqr);
        float alpha = disableFading ? 1f : CalculateAlpha(distance);

        Vector3 screenPos = cameraBrain.WorldToScreenPoint(_currentTargetTransform.position);

        if (screenPos.z < 0)
        {
            screenPos.x = _screenWidth - screenPos.x;
            screenPos.y = _screenHeight - screenPos.y;
        }

        bool offScreen = IsOffScreen(screenPos);
        Vector3 clampedPos = ClampToScreenBounds(screenPos);
        float fadeMult = (disableFading || !offScreen) ? 1f : OFF_SCREEN_FADE;

        interactionUI.iconTransform.position = Vector3.Lerp(
            interactionUI.iconTransform.position,
            clampedPos,
            Time.deltaTime * cornerLerpSpeed
        );

        interactionUI.UpdateCanvasGroup(alpha * fadeMult, fadeSpeed, disableFading);
        UpdateIcon(alpha);
    }

    private void HandleInteraction()
    {
        if (_currentTarget == null) return;

        _currentTarget.Interact();
        PlayInteractionTween().Forget();
    }

    private void HideInteractionUI()
    {
        interactionUI.UpdateCanvasGroup(0, fadeSpeed, disableFading);
        interactionUI.SetIcon(null, 0);
        UIAccess.Instance.crosshair.enabled = true;
    }

    private float CalculateAlpha(float distance)
    {
        float rawAlpha = Mathf.InverseLerp(interactionRange, fadeStartDistance, distance);
        return Mathf.Clamp(rawAlpha, minAlpha, 1f);
    }

    private Vector3 ClampToScreenBounds(Vector3 screenPos)
    {
        return new Vector3(
            Mathf.Clamp(screenPos.x, _screenBounds.minX, _screenBounds.maxX),
            Mathf.Clamp(screenPos.y, _screenBounds.minY, _screenBounds.maxY),
            screenPos.z
        );
    }

    private bool IsOffScreen(Vector3 screenPos)
    {
        return screenPos.x < _screenBounds.minX || screenPos.x > _screenBounds.maxX ||
               screenPos.y < _screenBounds.minY || screenPos.y > _screenBounds.maxY;
    }

    private void UpdateIcon(float alpha)
    {
        string type = _currentTarget.GetInteractionType();
        if (_interactionSprites.TryGetValue(type.ToLower(), out var sprite))
        {
            interactionUI.SetIcon(sprite, alpha);
        }
    }

    private async UniTask PlayInteractionTween()
    {
        if (interactionUI?.iconImage?.rectTransform == null) return;

        var rect = interactionUI.iconTransform;

        await Tween.Scale(rect, tweenDownScale, tweenDuration1, easingStyle1).ToYieldInstruction().ToUniTask();
        await UniTask.WaitForSeconds(delayBetweenTweens);
        await Tween.Scale(rect, tweenIdleScale, tweenDuration2, easingStyle2).ToYieldInstruction().ToUniTask();
    }

    [System.Serializable]
    public class InteractionUI
    {
        public RectTransform uiTransform;
        public RectTransform iconTransform;
        public Image iconImage;
        public CanvasGroup canvasGroup;

        public void SetIcon(Sprite sprite, float alpha)
        {
            if (iconImage == null) return;

            iconImage.sprite = sprite;
            var color = iconImage.color;
            color.a = alpha;
            iconImage.color = color;
            iconImage.gameObject.SetActive(sprite != null && alpha > 0.001f);
        }

        public void UpdateCanvasGroup(float targetAlpha, float speed, bool disableFading)
        {
            if (canvasGroup == null) return;

            if (disableFading)
            {
                canvasGroup.alpha = 1f;
                uiTransform.gameObject.SetActive(true);
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;

                return;
            }

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * speed);

            bool isActive = canvasGroup.alpha > 0.01f;
            uiTransform.gameObject.SetActive(isActive);
            canvasGroup.blocksRaycasts = isActive;
            canvasGroup.interactable = isActive;
        }

        public void Hide()
        {
            if (uiTransform != null)
            {
                uiTransform.gameObject.SetActive(false);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }
        }
    }

    [System.Serializable]
    public struct InteractionSpriteData
    {
        public string type;
        public Sprite sprite;
    }
}
