using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 4f;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private LayerMask interactableLayer;

    public IInteractable CurrentTarget => _currentTarget;

    private IInteractable _currentTarget;
    private Transform _currentTargetTransform;
    private Outline _currentOutline;
    private readonly Collider[] _interactableColliders = new Collider[10];

    private void Update()
    {
        FindNearestInteractable();
        UpdateInteractionUI();
    }

    private void OnEnable()
    {
        if (Core.Player != null && Core.Player.PlayerInputs != null)
        {
            Core.Player.PlayerInputs.OnInteract += HandleInteraction;
        }

        FindNearestInteractable();
        UpdateOutline();
    }

    private void OnDisable()
    {
        if (Core.Player != null && Core.Player.PlayerInputs != null)
        {
            Core.Player.PlayerInputs.OnInteract -= HandleInteraction;
        }

        DisableCurrentOutline();

        if (Core.UI?.Interact != null)
        {
            Core.UI.Interact.HideInteractionUI();
        }
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
            VibrationHelper.VibrateTap();
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
        if (Core.UI?.Interact == null) return;

        if (_currentTarget == null || _currentTargetTransform == null)
        {
            Core.UI.Interact.HideInteractionUI();
            return;
        }

        string interactionType = _currentTarget.GetInteractionType();
        Core.UI.Interact.UpdateInteractionUI(
            _currentTargetTransform,
            transform.position,
            interactionRange,
            interactionType
        );
    }

    private void HandleInteraction()
    {
        if (_currentTarget == null) return;

        _currentTarget.Interact();

        if (Core.UI?.Interact != null)
        {
            Core.UI.Interact.PlayInteractionTween().Forget();
        }

        VibrationHelper.VibrateLight();
    }
}