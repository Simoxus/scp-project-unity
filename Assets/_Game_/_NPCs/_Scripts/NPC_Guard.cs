using Cysharp.Threading.Tasks;
using UnityEngine;

public class NPC_Guard : BaseNPC
{
    [Space]
    [SerializeField] private Animator animator;
    [SerializeField] private NPC_Movement movement;
    [SerializeField] private Headlook headLook;
    [SerializeField] private GameObject voiceEmitter;

    [Header("Guard Settings")]
    [SerializeField] private bool startWithPlayerTracking = true;

    // Animation parameter hashes
    private static readonly int Idle2Hash = Animator.StringToHash("Idle-2");
    private static readonly int AimHash = Animator.StringToHash("Aim");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");
    private static readonly int TurnLeftHash = Animator.StringToHash("TurnLeft");
    private static readonly int TurnRightHash = Animator.StringToHash("TurnRight");
    private static readonly int LeanHash = Animator.StringToHash("Lean");
    private static readonly int Death1AHash = Animator.StringToHash("Death-1A");
    private static readonly int Death2Hash = Animator.StringToHash("Death-2");

    private LookTarget _playerLookTarget;
    private bool _isAiming = false;
    private bool _isDead = false;
    private const float MIN_DIRECTION_MAGNITUDE = 0.01f;

    public Animator Animator => animator;
    public GameObject VoiceEmitter => voiceEmitter;
    public bool IsAiming => _isAiming;
    public bool IsDead => _isDead;
    public bool IsMoving => movement != null && movement.IsMoving;

    protected override void OnNPCAwake()
    {
        if (startWithPlayerTracking && Core.Player != null)
        {
            CachePlayerLookTarget();
        }
    }

    private void CachePlayerLookTarget()
    {
        if (Core.Player.CameraBrain.TryGetComponent<LookTarget>(out _playerLookTarget))
        {
            if (headLook != null)
            {
                headLook.AddLookTarget(_playerLookTarget);
            }
        }
    }

    public void WalkTo(Vector3 destination)
    {
        if (movement != null && !_isDead)
        {
            movement.SetDestination(destination);
        }
    }

    public void WalkTo(Transform destination)
    {
        if (destination != null)
        {
            WalkTo(destination.position);
        }
    }

    public void StopMoving()
    {
        if (movement != null)
        {
            movement.Stop();
        }
    }

    public void ResumeMoving()
    {
        if (movement != null && !_isDead)
        {
            movement.Resume();
        }
    }

    public bool HasReachedDestination()
    {
        return movement != null && movement.HasReachedDestination();
    }

    public void SetIdle(bool isIdle2)
    {
        if (animator != null && !_isDead)
        {
            animator.SetBool(Idle2Hash, isIdle2);
        }
    }

    public void SetAiming(bool aiming)
    {
        if (animator != null && !_isDead)
        {
            _isAiming = aiming;
            animator.SetBool(AimHash, aiming);
        }
    }

    public void FireWeapon()
    {
        if (animator != null && _isAiming && !_isDead)
        {
            animator.SetTrigger(ShootHash);
        }
    }

    public void TurnLeft()
    {
        if (animator != null && !_isDead)
        {
            animator.SetTrigger(TurnLeftHash);
        }
    }

    public void TurnRight()
    {
        if (animator != null && !_isDead)
        {
            animator.SetTrigger(TurnRightHash);
        }
    }

    public void SetLeaning(bool leaning)
    {
        if (animator != null && !_isDead)
        {
            animator.SetBool(LeanHash, leaning);
        }
    }

    public void Die(int deathType = 1)
    {
        if (_isDead) return;

        _isDead = true;

        if (movement != null)
        {
            movement.Stop();
        }

        if (animator != null)
        {
            animator.SetTrigger(deathType == 1 ? Death1AHash : Death2Hash);
        }

        // Disable head tracking on death
        if (headLook != null)
        {
            headLook.IsEnabled = false;
        }
    }

    public void EnableHeadTracking(bool enable)
    {
        if (headLook != null)
        {
            headLook.IsEnabled = enable;
        }
    }

    public void LookAtPlayer(bool enable)
    {
        if (headLook == null) return;

        if (enable)
        {
            if (_playerLookTarget == null && Core.Player != null)
            {
                CachePlayerLookTarget();
            }

            if (_playerLookTarget != null)
            {
                headLook.AddLookTarget(_playerLookTarget);
            }
        }
        else
        {
            if (_playerLookTarget != null)
            {
                headLook.RemoveLookTarget(_playerLookTarget);
            }
        }
    }

    public void AddLookTarget(Transform target)
    {
        if (headLook == null || target == null) return;

        if (target.TryGetComponent<LookTarget>(out LookTarget lookTarget))
        {
            headLook.AddLookTarget(lookTarget);
        }
    }

    public void RemoveLookTarget(Transform target)
    {
        if (headLook == null || target == null) return;

        if (target.TryGetComponent<LookTarget>(out LookTarget lookTarget))
        {
            headLook.RemoveLookTarget(lookTarget);
        }
    }

    public void ClearAllLookTargets()
    {
        if (headLook != null && _playerLookTarget != null)
        {
            headLook.RemoveLookTarget(_playerLookTarget);
        }
    }

    public void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > MIN_DIRECTION_MAGNITUDE)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = targetRotation;
        }
    }

    public void FaceTarget(Transform target)
    {
        if (target != null)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            FaceDirection(direction);
        }
    }

    public async UniTask FaceTargetSmoothly(Transform target, float duration = 1f)
    {
        if (target == null || _isDead) return;

        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < MIN_DIRECTION_MAGNITUDE)
        {
            return; // Already facing target
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        while (elapsed < duration && target != null && !_isDead)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }

        if (target != null && !_isDead)
        {
            transform.rotation = targetRotation;
        }
    }

    public async UniTask<bool> FollowWaypoints(Transform[] waypoints, float waitTimeAtWaypoint = 0.5f)
    {
        if (waypoints == null || waypoints.Length == 0 || _isDead)
        {
            return false;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform waypoint = waypoints[i];

            if (waypoint == null || _isDead)
            {
                return false;
            }

            Vector3 destination = new Vector3(waypoint.position.x, transform.position.y, waypoint.position.z);
            WalkTo(destination);

            await UniTask.Yield();

            float timeout = 30f;
            float elapsed = 0f;

            while (!HasReachedDestination() && elapsed < timeout && !_isDead)
            {
                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }

            if (elapsed >= timeout)
            {
                return false;
            }

            if (waitTimeAtWaypoint > 0f)
            {
                await UniTask.WaitForSeconds(waitTimeAtWaypoint, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }

        return true;
    }
}