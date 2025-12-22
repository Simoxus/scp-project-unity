using Cysharp.Threading.Tasks;
using EditorAttributes;
using System.Threading;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Data Reference")]
    public RoomData roomData;

    [Header("Runtime References")]
    public ButtonDoorController[] buttonDoors;
    public Light[] roomLights;

    [Header("Room State"), ReadOnly]
    public bool isInitialized = false;
    public bool hasPlayer = false;

    // Properties
    public string RoomID => roomData?.roomID ?? "Unknown";
    public string RoomName => roomData?.roomName ?? "Unknown";

    private void Awake()
    {
        RoomLoad();
    }

    private void OnDestroy()
    {
        RoomUnload();
    }

    public virtual void RoomLoad()
    {
        // Auto-find components
        if (buttonDoors == null || buttonDoors.Length == 0)
            buttonDoors = GetComponentsInChildren<ButtonDoorController>();

        if (roomLights == null || roomLights.Length == 0)
            roomLights = GetComponentsInChildren<Light>();

        // Apply environment settings
        if (roomData != null)
        {
            if (roomData.customAmbience)
            {
                FMODHelper.PlayOneShotWithDynamicOcclusion(
                    roomData.customAmbienceAudio,
                    transform.position,
                    1.5f
                );
            }
        }

        isInitialized = true;

        Log.VerboseInfo($"[Room: {RoomName}] Loaded");
    }

    public virtual void RoomUnload()
    {
        Log.VerboseInfo($"[Room: {RoomName}] Unloaded");
    }

    public virtual void RoomUpdate()
    {

    }

    private void Update()
    {
        if (isInitialized)
            RoomUpdate();
    }

    public ButtonDoorController GetDoor(int index)
    {
        if (buttonDoors != null && index >= 0 && index < buttonDoors.Length)
            return buttonDoors[index];
        return null;
    }
}