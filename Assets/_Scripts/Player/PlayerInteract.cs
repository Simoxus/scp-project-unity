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
        if (Core.Player != null && Core.Player.Inputs != null)
        {
            Core.Player.Inputs.OnInteract += HandleInteraction;
        }

        FindNearestInteractable();
        UpdateOutline();
    }

    private void OnDisable()
    {
        if (Core.Player != null && Core.Player.Inputs != null)
        {
            Core.Player.Inputs.OnInteract -= HandleInteraction;
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

            Collider interactionCollider = interactable.GetInteractionCollider();
            if (interactionCollider != null && interactionCollider != collider)
            {
                continue;
            }

            Vector3 targetPos = collider.transform.position;
            Vector3 directionToTarget = targetPos - playerPos;
            float distSqr = directionToTarget.sqrMagnitude;

            if (distSqr >= closestDistSqr) continue;

            // Use the interactable's specified raycast target
            Vector3 raycastTarget = interactable.GetRaycastTarget();
            Vector3 rayDirection = raycastTarget - playerPos;
            float distance = rayDirection.magnitude;

            if (Physics.Raycast(playerPos, rayDirection / distance, distance, obstacleLayers))
            {
                continue;
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

    private void OnDrawGizmos()
    {
        // Draw interaction sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw raycast to current target
        if (_currentTargetTransform != null)
        {
            Vector3 playerPos = transform.position;
            Vector3 targetPos = _currentTargetTransform.position;
            Vector3 direction = targetPos - playerPos;
            float distance = direction.magnitude;

            // Green line to valid target
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerPos, targetPos);
            Gizmos.DrawWireSphere(targetPos, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show blocked raycasts (more expensive, only when selected)
        if (!Application.isPlaying) return;

        Vector3 playerPos = transform.position;

        for (int i = 0; i < _interactableColliders.Length; i++)
        {
            Collider collider = _interactableColliders[i];
            if (collider == null) continue;
            if (collider.transform == _currentTargetTransform) continue; // Already drawn

            Vector3 targetPos = collider.transform.position;
            Vector3 direction = targetPos - playerPos;
            float distance = direction.magnitude;

            if (distance > interactionRange) continue;

            // Red line if raycast is blocked
            if (Physics.Raycast(playerPos, direction.normalized, distance, obstacleLayers))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(playerPos, targetPos);
                Gizmos.DrawWireSphere(targetPos, 0.15f);
            }
        }
    }
}