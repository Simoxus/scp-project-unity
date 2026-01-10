using EditorAttributes;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;

public enum PlayerState
{
    Freefall,
    Noclip,
    Idle,
    Walking,
    Sprinting,
    Crouching
}

public class Player : Singleton<Player>
{
    [ReadOnly] public PlayerState CurrentState = PlayerState.Idle;

    [Header("Scripts")]
    public PlayerController PlayerController;
    public PlayerInputs PlayerInputs;
    public PlayerStats PlayerStats;
    public PlayerHealth PlayerHealth;
    public PlayerBobbing PlayerBobbing;
    public PlayerSanity PlayerSanity;
    public PlayerFootsteps PlayerFootsteps;
    public PlayerInteract PlayerInteract;
    public PlayerFreecam PlayerFreecam;

    [Header("Components")]
    public CharacterController CharacterController;
    public GameObject CameraRoot;
    public Camera CameraBrain;
    public CinemachineCamera CameraMain;
    public CinemachineBrain CameraSettings;
    public CinemachineImpulseListener CameraImpulseListener;
    public CinemachineImpulseSource CameraImpulseSource;
    public StudioListener CameraBrainStudioListener;

    public void Reset()
    {
        AssignReferences();
    }

    public bool CanMove()
    {
        return CurrentState != PlayerState.Noclip
            && PlayerController != null
            && PlayerController.enabled;
    }

    public bool IsInState(PlayerState state) => CurrentState == state;

    public bool IsMoving() => PlayerController != null && PlayerController.IsMoving;

    public bool IsGrounded() => CharacterController != null && CharacterController.isGrounded;

    private void AssignReferences()
    {
        // Assign scripts
        PlayerController = GetComponent<PlayerController>();
        PlayerInputs = GetComponent<PlayerInputs>();
        PlayerStats = GetComponent<PlayerStats>();
        PlayerHealth = GetComponent<PlayerHealth>();
        PlayerBobbing = GetComponent<PlayerBobbing>();
        PlayerSanity = GetComponent<PlayerSanity>();
        PlayerFootsteps = GetComponent<PlayerFootsteps>();
        PlayerInteract = GetComponent<PlayerInteract>();
        PlayerFreecam = GetComponent<PlayerFreecam>();

        // Assign components
        CharacterController = GetComponent<CharacterController>();
        CameraRoot = GetComponentInChildren<CinemachineCamera>().transform.parent.gameObject;
        CameraBrain = GetComponentInChildren<Camera>();
        CameraMain = GetComponentInChildren<CinemachineCamera>();
        CameraSettings = GetComponentInChildren<CinemachineBrain>();
        CameraImpulseListener = GetComponentInChildren<CinemachineImpulseListener>();
        CameraImpulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        CameraBrainStudioListener = GetComponentInChildren<StudioListener>();
    }
}