using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Player", menuName = "Custom/FMOD/Player Audio Data")]
public class AudioData_Player : ScriptableObject
{
    public EventReference WalkFootstepSound;
    public EventReference RunFootstepSound;
    public EventReference TiredSound;
    public EventReference TiredGasSound;
}