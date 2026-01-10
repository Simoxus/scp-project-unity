using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Collision", menuName = "Custom/FMOD/Collision Audio Data")]
public class AudioData_Collision : ScriptableObject
{
    public EventReference DoorFallSound;
    public EventReference GateFallSound;
}