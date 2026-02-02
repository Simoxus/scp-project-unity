using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Doors", menuName = "Custom/FMOD/Doors Audio Data")]
public class AudioData_Doors : ScriptableObject
{
    [Header("Buttons")]
    public EventReference ButtonSound;
    public EventReference ButtonErrorSound;
    public EventReference ButtonKeycardSound;
    public EventReference ButtonKeypadSound;

    [Header("Doors")]
    public EventReference DoorSound;
    public EventReference DoorBreakSound;
    public EventReference DoorBrokenSound;
    public EventReference DoorForcedSound;
    public EventReference DoorOfficeSound;
    public EventReference DoorOfficeLeverSound;

    [Header("Gates")]
    public EventReference GateSound;
    public EventReference GateBreakSound;
    public EventReference GateBrokenSound;
}