using UnityEngine;

public class PlayerBobbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player = Player.Instance;

    [Header("Bob Settings")]
    public bool doBob = true;
    public float bobMultiplier = 1f;
    public float bobSpeed = 10f;
    [SerializeField] private float bobAmountX = 0.1f;
    [SerializeField] private float bobAmountY = 0.15f;

    [Header("Tilt Settings")]
    public bool doTilt = true;
    public float tiltMultiplier = 1f;
    public float tiltSpeed = 10f;
    [SerializeField] private float tiltMaxAngle = 1.5f;

    [Header("Transition Settings")]
    [SerializeField] private float transitionTime = 0.2f;
    [SerializeField] private float transitionStopTime = 0.1f;

    private float _bobTimer = 0f;
    private float _tiltTimer = 0f;
    private Vector3 _originalCameraLocalPosition;
    private Quaternion _originalCameraLocalRotation;
    private Vector3 _currentPositionVelocity;
    private float _currentTiltVelocity;

    private void Awake()
    {
        // Check for player and if there's no player, try to find the singleton/instance
        player = player != null ? player : Player.Instance;
    }

    private void Start()
    {
        if (player.cameraRoot.transform != null)
        {
            _originalCameraLocalPosition = player.cameraRoot.transform.localPosition;
            _originalCameraLocalRotation = player.cameraRoot.transform.localRotation;
        }
    }

    private void Update()
    {
        if (player.cameraRoot.transform == null || !enabled) return;
        if (player.playerController == null) { return; }

        Vector3 targetLocalPosition = _originalCameraLocalPosition;
        float targetTiltZ = 0f;
        float currentTransitionTime = transitionTime;

        if (player.playerController.isMoving)
        {
            float currentMoveSpeed = player.playerController.DetermineCurrentSpeed();
            float speedScale = currentMoveSpeed / 5f;

            _bobTimer += Time.deltaTime * bobSpeed * speedScale;
            _tiltTimer += Time.deltaTime * tiltSpeed * speedScale;

            if (doBob)
            {
                float bobX = Mathf.Cos(_bobTimer / 2f) * bobAmountX * bobMultiplier;
                float bobY = Mathf.Sin(_bobTimer) * bobAmountY * bobMultiplier;
                targetLocalPosition = _originalCameraLocalPosition + new Vector3(bobX, bobY, 0f);
            }

            if (doTilt)
            {
                targetTiltZ = Mathf.Cos(_tiltTimer / 2f) * tiltMaxAngle * tiltMultiplier;
            }

            currentTransitionTime = transitionTime;
        }
        else // Not moving
        {
            currentTransitionTime = transitionStopTime;
            _bobTimer = 0f; // Reset timer when not moving
            _tiltTimer = 0f;
        }

        player.cameraRoot.transform.localPosition = Vector3.SmoothDamp(
            player.cameraRoot.transform.localPosition,
            targetLocalPosition,
            ref _currentPositionVelocity,
            currentTransitionTime
        );

        float currentZ = player.cameraRoot.transform.localRotation.eulerAngles.z;
        currentZ = Mathf.DeltaAngle(0, currentZ);
        float smoothedZ = Mathf.SmoothDamp(
            currentZ,
            targetTiltZ,
            ref _currentTiltVelocity,
            currentTransitionTime
        );

        Vector3 currentEuler = _originalCameraLocalRotation.eulerAngles;
        player.cameraRoot.transform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, smoothedZ);
    }
}