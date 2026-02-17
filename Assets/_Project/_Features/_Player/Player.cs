using FMODUnity;
using TriInspector;
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
    [Space]
    [ReadOnly] public PlayerState CurrentState = PlayerState.Idle;

    [Header("Scripts")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerSprint playerSprint;
    [SerializeField] private PlayerBlink playerBlink;
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerBobbing playerBobbing;
    [SerializeField] private PlayerSanity playerSanity;
    [SerializeField] private PlayerFootsteps playerFootsteps;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerFreecam playerFreecam;

    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject cameraRoot;
    [SerializeField] private Camera cameraBrain;
    [SerializeField] private CinemachineCamera cameraMain;
    [SerializeField] private CinemachineBrain cameraSettings;
    [SerializeField] private CinemachineImpulseListener cameraImpulseListener;
    [SerializeField] private CinemachineImpulseSource cameraImpulseSource;
    [SerializeField] private StudioListener cameraBrainStudioListener;

    // Properties
    public PlayerController Controller => playerController;
    public PlayerSprint Sprint => playerSprint;
    public PlayerBlink Blink => playerBlink;
    public PlayerInputs Inputs => playerInputs;
    public PlayerHealth Health => playerHealth;
    public PlayerBobbing Bobbing => playerBobbing;
    public PlayerSanity Sanity => playerSanity;
    public PlayerFootsteps Footsteps => playerFootsteps;
    public PlayerInteract Interact => playerInteract;
    public PlayerInventory Inventory => playerInventory;
    public PlayerFreecam Freecam => playerFreecam;

    public CharacterController CharacterController => characterController;
    public GameObject CameraRoot => cameraRoot;
    public Camera CameraBrain => cameraBrain;
    public CinemachineCamera CameraMain => cameraMain;
    public CinemachineBrain CameraSettings => cameraSettings;
    public CinemachineImpulseListener CameraImpulseListener => cameraImpulseListener;
    public CinemachineImpulseSource CameraImpulseSource => cameraImpulseSource;
    public StudioListener CameraBrainStudioListener => cameraBrainStudioListener;

    public void Reset()
    {
        AssignReferences();
    }

    public bool CanMove()
    {
        return CurrentState != PlayerState.Noclip
            && playerController != null
            && playerController.enabled;
    }

    public bool IsInState(PlayerState state) => CurrentState == state;
    public bool IsMoving() => playerController != null && playerController.IsMoving;
    public bool IsGrounded() => characterController != null && characterController.isGrounded;

    private void AssignReferences()
    {
        // Assign scripts
        playerController = GetComponent<PlayerController>();
        playerSprint = GetComponent<PlayerSprint>();
        playerBlink = GetComponent<PlayerBlink>();
        playerInputs = GetComponent<PlayerInputs>();
        playerHealth = GetComponent<PlayerHealth>();
        playerBobbing = GetComponent<PlayerBobbing>();
        playerSanity = GetComponent<PlayerSanity>();
        playerFootsteps = GetComponent<PlayerFootsteps>();
        playerInteract = GetComponent<PlayerInteract>();
        playerInventory = GetComponent<PlayerInventory>();
        playerFreecam = GetComponent<PlayerFreecam>();

        // Assign components
        characterController = GetComponent<CharacterController>();
        cameraRoot = GetComponentInChildren<CinemachineCamera>().transform.parent.gameObject;
        cameraBrain = GetComponentInChildren<Camera>();
        cameraMain = GetComponentInChildren<CinemachineCamera>();
        cameraSettings = GetComponentInChildren<CinemachineBrain>();
        cameraImpulseListener = GetComponentInChildren<CinemachineImpulseListener>();
        cameraImpulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        cameraBrainStudioListener = GetComponentInChildren<StudioListener>();
    }
}