using PrimeTween;
using UnityEngine;
public abstract class BaseDoorActivator : MonoBehaviour, IInteractable
{
    [Header("Activator Settings")]
    [SerializeField] protected bool enableSecondActivator;
    [SerializeField] protected string interactionType = "Hand";
    [Header("Outline Reference")]
    public Outline outline;
    [Header("Collider References")]
    public BoxCollider activatorCollider;
    public BoxCollider secondActivatorCollider;
    protected Tween _pulseTween;
    protected virtual void Reset()
    {
        outline = GetComponentInChildren<Outline>();
    }
    protected virtual void Awake()
    {
        if (activatorCollider == null && secondActivatorCollider == null)
        {
            Log.VerboseWarning($"{GetType()} on '{gameObject.name}' has no colliders assigned. It will not be detectable.");
        }
    }
    protected virtual void Start()
    {
    }
    protected virtual void OnDestroy()
    {
        _pulseTween.Stop();
    }
    public Outline GetOutline()
    {
        return outline;
    }
    public virtual Transform GetTransform()
    {
        return transform;
    }
    public string GetInteractionType()
    {
        return interactionType;
    }
    public abstract void Interact();
    public virtual void SetButtonState(bool enabled)
    {
        if (activatorCollider != null)
        {
            activatorCollider.enabled = enabled;
        }
        if (enableSecondActivator && secondActivatorCollider != null)
        {
            secondActivatorCollider.enabled = enabled;
        }
    }
    public abstract void StartPulseEffect(Color color, float? customDuration = null, float? customIntensity = null);
    public abstract void StopPulseEffect();
}