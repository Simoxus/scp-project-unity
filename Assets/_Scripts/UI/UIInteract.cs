using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIInteract : MonoBehaviour
{
    [Space]
    public RectTransform uiTransform;
    public RectTransform iconTransform;
    public Image iconImage;
    public CanvasGroup canvasGroup;

    [Header("Sprite Data")]
    public InteractionSpriteData[] interactionSpriteData;

    [Header("Position Settings")]
    public float baseHorizontalPadding = -140f;
    public float baseVerticalPadding = -140f;
    public float referenceAspect = 16f / 9f;
    public float cornerLerpSpeed = 40f;
    public float edgeThreshold = 0.2f;

    [Header("Fade Settings")]
    public bool disableFading = false;
    public float onScreenFadeSpeed = 10f;
    public float offScreenFadeSpeed = 15f;
    public float fadeStartDistance = 2.5f;
    [Range(0f, 1f)] public float minAlpha = 0.4f;

    [Header("Animation Settings")]
    public Ease easingStyle1 = Ease.InSine;
    public Ease easingStyle2 = Ease.OutSine;
    public float tweenIdleScale = 0.65f;
    public float tweenDownScale = 0.55f;
    public float tweenDuration1 = 0.12f;
    public float tweenDuration2 = 0.08f;
    public float delayBetweenTweens = 0.121f;

    private Dictionary<string, Sprite> _interactionSprites;
    private int _screenWidth;
    private int _screenHeight;
    private float _cachedAspect;
    private float _cachedHorizontalPadding;
    private ScreenBounds _screenBounds;

    private bool _wasHidden = true;

    private struct ScreenBounds
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
    }

    private void Awake()
    {
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
    }

    public void UpdateInteractionUI(Transform targetTransform, Vector3 playerPosition, float interactionRange, string interactionType)
    {
        if (targetTransform == null || Core.Player.CameraBrain == null)
        {
            HideInteractionUI();
            return;
        }

        Core.UI.Crosshair.enabled = false;

        float distSqr = (playerPosition - targetTransform.position).sqrMagnitude;
        float distance = Mathf.Sqrt(distSqr);
        float alpha = disableFading ? 1f : CalculateAlpha(distance, interactionRange);

        Vector3 screenPos = Core.Player.CameraBrain.WorldToScreenPoint(targetTransform.position);

        if (screenPos.z < 0)
        {
            screenPos.x = _screenWidth - screenPos.x;
            screenPos.y = _screenHeight - screenPos.y;
        }

        bool offScreen = IsOffScreen(screenPos);
        Vector3 clampedPos = ClampToScreenBounds(screenPos);
        float fadeMult = (disableFading || !offScreen) ? 1f : offScreenFadeSpeed;

        if (_wasHidden)
        {
            iconTransform.position = clampedPos;
            _wasHidden = false;
        }
        else
        {
            iconTransform.position = Vector3.Lerp(
                iconTransform.position,
                clampedPos,
                Time.deltaTime * cornerLerpSpeed
            );
        }

        UpdateCanvasGroup(alpha * fadeMult);
        UpdateIcon(interactionType, alpha);
    }

    public void HideInteractionUI()
    {
        UpdateCanvasGroup(0);
        SetIcon(null, 0);
        Core.UI.Crosshair.enabled = true;
        _wasHidden = true;
    }

    public async UniTask PlayInteractionTween()
    {
        if (iconImage?.rectTransform == null) return;

        var rect = iconTransform;

        await Tween.Scale(rect, tweenDownScale, tweenDuration1, easingStyle1).ToYieldInstruction().ToUniTask();
        await UniTask.WaitForSeconds(delayBetweenTweens);
        await Tween.Scale(rect, tweenIdleScale, tweenDuration2, easingStyle2).ToYieldInstruction().ToUniTask();
    }

    public void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (uiTransform != null)
        {
            uiTransform.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        FMODHelper.PlayOneShot(Core.AudioDataAccess.Items.ItemPickDocSound);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (uiTransform != null)
        {
            uiTransform.gameObject.SetActive(false);
        }
    }

    public void Toggle(bool visible)
    {
        if (visible)
            Show();
        else
            Hide();
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

    private float CalculateAlpha(float distance, float interactionRange)
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

    private void UpdateIcon(string type, float alpha)
    {
        if (_interactionSprites.TryGetValue(type.ToLower(), out var sprite))
        {
            SetIcon(sprite, alpha);
        }
    }

    private void SetIcon(Sprite sprite, float alpha)
    {
        if (iconImage == null) return;

        iconImage.sprite = sprite;
        var color = iconImage.color;
        color.a = alpha;
        iconImage.color = color;
        iconImage.gameObject.SetActive(sprite != null && alpha > 0.001f);
    }

    private void UpdateCanvasGroup(float targetAlpha)
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

        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * onScreenFadeSpeed);

        bool isActive = canvasGroup.alpha > 0.01f;
        uiTransform.gameObject.SetActive(isActive);
        canvasGroup.blocksRaycasts = isActive;
        canvasGroup.interactable = isActive;
    }

    [System.Serializable]
    public struct InteractionSpriteData
    {
        public string type;
        public Sprite sprite;
    }
}