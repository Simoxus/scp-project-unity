using UnityEngine;
using Unity.Cinemachine;
using FMODUnity;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

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
    public UIIndicators uiIndicators;

    [Header("Components")]
    public CharacterController characterController;
    public StudioEventEmitter footstepEmitter;
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
    
    private void OnValidate()
    {
        AutoAssignReferences();
    }

    // (attempt an) Auto assignment
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

        characterController = GetComponent<CharacterController>();
        footstepEmitter = GetComponentInChildren<StudioEventEmitter>();
        cameraBrain = GetComponentInChildren<Camera>();
        cameraMain = GetComponentInChildren<CinemachineCamera>();
        cameraImpulseListener = GetComponentInChildren<CinemachineImpulseListener>();
        cameraImpulseSource = GetComponentInChildren<CinemachineImpulseSource>();
    }
}