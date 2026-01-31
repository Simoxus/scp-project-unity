using UnityEngine;

public class NPC_Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float stoppingDistance = 0.1f;

    private Animator _animator;
    private Vector3 _targetPosition;
    private bool _hasTarget = false;

    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");

    public bool IsMoving { get; private set; }
    public event System.Action OnMovementStarted;
    public event System.Action OnMovementStopped;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_animator != null)
        {
            _animator.SetBool(Move, false);
            _animator.SetFloat(MoveX, 0f);
            _animator.SetFloat(MoveY, 0f);
        }
    }

    private void Update()
    {
        if (!_hasTarget)
        {
            SetMovingState(false);
            return;
        }

        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0f; // Keep movement on XZ plane
        float distance = direction.magnitude;

        // Check if reached target
        if (distance <= stoppingDistance)
        {
            SetMovingState(false);
            return;
        }

        // Move towards target
        Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Rotate towards movement direction
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Update animator
        SetMovingState(true);
        if (_animator != null)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(direction.normalized * moveSpeed);
            _animator.SetFloat(MoveX, localVelocity.x);
            _animator.SetFloat(MoveY, localVelocity.z);
        }
    }

    public void SetDestination(Vector3 destination)
    {
        _targetPosition = destination;
        _hasTarget = true;
    }

    public void Stop()
    {
        _hasTarget = false;
        SetMovingState(false);

        if (_animator != null)
        {
            _animator.SetFloat(MoveX, 0f);
            _animator.SetFloat(MoveY, 0f);
        }
    }

    public void Resume()
    {
        // If we have a target position, resume movement towards it
        if (_targetPosition != Vector3.zero)
        {
            _hasTarget = true;
        }
    }

    public void ClearDestination()
    {
        _hasTarget = false;
        _targetPosition = Vector3.zero;
    }

    public bool HasReachedDestination()
    {
        if (!_hasTarget) return false;

        float distance = Vector3.Distance(transform.position, _targetPosition);
        return distance <= stoppingDistance;
    }

    private void SetMovingState(bool moving)
    {
        if (IsMoving != moving)
        {
            IsMoving = moving;

            if (_animator != null)
            {
                _animator.SetBool(Move, moving);
            }

            if (moving)
            {
                OnMovementStarted?.Invoke();
            }
            else
            {
                OnMovementStopped?.Invoke();
            }
        }
    }
}