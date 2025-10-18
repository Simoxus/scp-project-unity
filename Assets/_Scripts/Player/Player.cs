using UnityEngine;
using Unity.Cinemachine;
using FMODUnity;
using EditorAttributes;

public enum PlayerState
{
    Idle,
    Walking,
    Sprinting,
    Crouching,
    Freefall
}

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [ReadOnly] public PlayerState currentState = PlayerState.Idle;

    [Header("Scripts")]
    public PlayerController playerController;
    public PlayerInputs playerInputs;
    public PlayerStats playerStats;
    public PlayerHealth playerHealth;
    public PlayerEffects playerEffects;
    public PlayerBobbing playerBobbing;
    public PlayerSanity playerSanity;
    public PlayerFootsteps playerFootsteps;
    public PlayerInteract playerInteract;
    public PlayerFreecam playerFreecam;

    [Header("Components")]
    public CharacterController characterController;
    public GameObject cameraRoot;
    public Camera cameraBrain;
    public CinemachineCamera cameraMain;
    public CinemachineImpulseListener cameraImpulseListener;
    public CinemachineImpulseSource cameraImpulseSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Reset()
    {
        AutoAssignReferences();
    }

    private void AutoAssignReferences()
    {
        playerController = GetComponent<PlayerController>();
        playerInputs = GetComponent<PlayerInputs>();
        playerStats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();
        playerEffects = GetComponent<PlayerEffects>();
        playerBobbing = GetComponent<PlayerBobbing>();
        playerSanity = GetComponent<PlayerSanity>();
        playerFootsteps = GetComponent<PlayerFootsteps>();
        playerInteract = GetComponent<PlayerInteract>();
        playerFreecam = GetComponent<PlayerFreecam>();

        characterController = GetComponent<CharacterController>();
        cameraBrain = GetComponentInChildren<Camera>();
        cameraMain = GetComponentInChildren<CinemachineCamera>();
        cameraRoot = cameraBrain.transform.parent.gameObject;
        cameraImpulseListener = GetComponentInChildren<CinemachineImpulseListener>();
        cameraImpulseSource = GetComponentInChildren<CinemachineImpulseSource>();
    }
}