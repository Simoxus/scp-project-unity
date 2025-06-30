using UnityEngine;
using Unity.Cinemachine;
using FMODUnity;

public class PlayerAccess : MonoBehaviour
{
    [Header("Scripts")]
    public PlayerController playerController;
    public PlayerInputs playerInputs;
    public PlayerHealth playerHealth;
    public PlayerEffects playerEffects;
    public PlayerBobbing playerBobbing;
    public PlayerSanity playerSanity;
    public PlayerFootsteps playerFootsteps;
    public PlayerInteract playerInteract;
    public PlayerCutscenes playerCutscenes;

    [Header("Components")]
    public CharacterController characterController;
    public StudioEventEmitter footstepEmitter;
    public GameObject cameraRoot;
    public Camera cameraBrain;
    public CinemachineCamera cameraMain;
    public CinemachineImpulseListener cameraImpulseListener;
    public CinemachineImpulseSource cameraImpulseSource;

    private void Reset() // Auto assignment
    {
        playerController = GetComponent<PlayerController>();
        playerInputs = GetComponent<PlayerInputs>();
        playerHealth = GetComponent<PlayerHealth>();
        playerEffects = GetComponent<PlayerEffects>();
        playerBobbing = GetComponent<PlayerBobbing>();
        playerFootsteps = GetComponent<PlayerFootsteps>();
        playerInteract = GetComponent<PlayerInteract>();
        playerCutscenes = GetComponent<PlayerCutscenes>();

        characterController = GetComponent<CharacterController>();
        footstepEmitter = GetComponentInChildren<StudioEventEmitter>();
        cameraBrain = GetComponentInChildren<Camera>();
        cameraMain = GetComponentInChildren<CinemachineCamera>();
        cameraImpulseListener = GetComponentInChildren<CinemachineImpulseListener>();
        cameraImpulseSource = GetComponentInChildren<CinemachineImpulseSource>();
    }
}