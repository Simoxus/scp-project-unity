using UnityEngine;

public class NPC_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float stoppingDistance = 0.1f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float minRotationSpeed = 5f;
    [SerializeField] private float maxRotationSpeed = 15f;
    [SerializeField] private bool rotateBeforeMoving = false;
    [SerializeField] private float minAngleToMove = 30f; // Only used if rotateBeforeMoving is true

    [Header("Animation Settings")]
    [SerializeField] private float animatorDampTime = 0.15f;
    [SerializeField] private bool useRootMotion = false;

    private Animator _animator;
    private Vector3 _targetPosition;
    private bool _hasTarget = false;
    private float _currentSpeed = 0f;
    private float _currentAnimX = 0f;
    private float _currentAnimY = 0f;
    private bool _isPaused = false;

    // Animation parameter hashes
    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");

    public bool IsMoving { get; private set; }
    public float CurrentSpeed => _currentSpeed;
    public Vector3 Velocity => transform.forward * _currentSpeed;

    public event System.Action OnMovementStarted;
    public event System.Action OnMovementStopped;
    public event System.Action OnDestinationReached;

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
        if (_isPaused || !_hasTarget)
        {
            HandleDeceleration();
            return;
        }

        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0f; // Keep movement on XZ plane
        float distance = direction.magnitude;

        // Check if reached target
        if (distance <= stoppingDistance)
        {
            ReachDestination();
            return;
        }

        // Handle rotation
        bool canMove = HandleRotation(direction, distance);

        // Handle movement
        if (canMove || !rotateBeforeMoving)
        {
            HandleMovement(direction, distance);
        }
        else
        {
            HandleDeceleration();
        }

        // Update animator with smooth damping
        UpdateAnimator(direction);
    }

    private bool HandleRotation(Vector3 direction, float distance)
    {
        if (direction.magnitude < 0.01f) return true;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        // Adaptive rotation speed based on angle difference
        float adaptiveRotSpeed = Mathf.Lerp(minRotationSpeed, maxRotationSpeed, angleDifference / 180f);
        float finalRotSpeed = rotationSpeed * adaptiveRotSpeed;

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * finalRotSpeed
        );

        // Check if we can start moving (only matters if rotateBeforeMoving is true)
        return angleDifference < minAngleToMove;
    }

    private void HandleMovement(Vector3 direction, float distance)
    {
        // Calculate target speed based on distance (slow down near destination)
        float targetSpeed = moveSpeed;
        float slowDownDistance = moveSpeed * 0.5f; // Start slowing down at half a second away

        if (distance < slowDownDistance)
        {
            targetSpeed = Mathf.Lerp(0f, moveSpeed, distance / slowDownDistance);
        }

        // Smoothly accelerate or decelerate to target speed
        if (_currentSpeed < targetSpeed)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, deceleration * Time.deltaTime);
        }

        // Move the character
        if (!useRootMotion)
        {
            Vector3 movement = direction.normalized * _currentSpeed * Time.deltaTime;
            transform.position += movement;
        }

        SetMovingState(true);
    }

    private void HandleDeceleration()
    {
        if (_currentSpeed > 0.01f)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, deceleration * Time.deltaTime);

            if (!useRootMotion)
            {
                Vector3 movement = transform.forward * _currentSpeed * Time.deltaTime;
                transform.position += movement;
            }

            UpdateAnimator(transform.forward);
        }
        else
        {
            _currentSpeed = 0f;
            SetMovingState(false);
        }
    }

    private void UpdateAnimator(Vector3 direction)
    {
        if (_animator == null) return;

        // Convert world direction to local space
        Vector3 localVelocity = transform.InverseTransformDirection(direction.normalized * _currentSpeed);

        // Smooth damp the animator values for better blending
        _currentAnimX = Mathf.Lerp(_currentAnimX, localVelocity.x, Time.deltaTime / animatorDampTime);
        _currentAnimY = Mathf.Lerp(_currentAnimY, localVelocity.z, Time.deltaTime / animatorDampTime);

        _animator.SetFloat(MoveX, _currentAnimX);
        _animator.SetFloat(MoveY, _currentAnimY);
    }

    private void ReachDestination()
    {
        _hasTarget = false;
        HandleDeceleration();
        OnDestinationReached?.Invoke();
    }

    public void SetDestination(Vector3 destination)
    {
        _targetPosition = destination;
        _hasTarget = true;
        _isPaused = false;
    }

    public void Stop()
    {
        _hasTarget = false;
        _isPaused = false;

        // Let deceleration handle the smooth stop
        if (_currentSpeed <= 0.01f)
        {
            SetMovingState(false);
            if (_animator != null)
            {
                _animator.SetFloat(MoveX, 0f);
                _animator.SetFloat(MoveY, 0f);
            }
        }
    }

    public void Pause()
    {
        _isPaused = true;
    }

    public void Resume()
    {
        // Resume movement towards existing target
        if (_targetPosition != Vector3.zero)
        {
            _hasTarget = true;
            _isPaused = false;
        }
    }

    public void ClearDestination()
    {
        _hasTarget = false;
        _isPaused = false;
        _targetPosition = Vector3.zero;
        Stop();
    }

    public bool HasReachedDestination()
    {
        if (!_hasTarget) return false;
        float distance = Vector3.Distance(transform.position, _targetPosition);
        return distance <= stoppingDistance;
    }

    public float GetDistanceToDestination()
    {
        if (!_hasTarget) return 0f;
        return Vector3.Distance(transform.position, _targetPosition);
    }

    public Vector3 GetDirectionToDestination()
    {
        if (!_hasTarget) return Vector3.zero;
        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0f;
        return direction.normalized;
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

                // Reset animator values when fully stopped
                _currentAnimX = 0f;
                _currentAnimY = 0f;
            }
        }
    }
}