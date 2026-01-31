using PrimeTween;
using UnityEngine;

public abstract class BaseDoorActivator : MonoBehaviour, IInteractable
{
    public abstract BaseDoorController DoorController { get; }

    [Space]
    public BoxCollider ActivatorCollider;
    public BoxCollider SecondActivatorCollider;
    public Outline InteractOutline;

    [Header("Settings")]
    [SerializeField] protected string interactionType = "Hand";

    protected Tween _pulseTween;

    protected virtual void Awake()
    {
        if (ActivatorCollider == null && SecondActivatorCollider == null)
        {
            Log.VerboseWarning($"Door activator on '{gameObject.name}' has no colliders assigned.");
        }
    }

    protected virtual void Start()
    {
    }

    protected virtual void OnDestroy()
    {
        if (_pulseTween.isAlive)
        {
            _pulseTween.Stop();
        }
    }

    public Outline GetOutline()
    {
        return InteractOutline;
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
        if (ActivatorCollider != null)
        {
            ActivatorCollider.enabled = enabled;
        }

        if (SecondActivatorCollider != null)
        {
            SecondActivatorCollider.enabled = enabled;
        }
    }

    public abstract void StartPulseEffect(Color color, float? customDuration = null, float? customIntensity = null);
    public abstract void StopPulseEffect();
}