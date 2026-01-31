using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Player", menuName = "Custom/FMOD/Player Audio Data")]
public class AudioData_Player : ScriptableObject
{
    [Space]
    public EventReference WalkFootstepSound;
    public EventReference RunFootstepSound;
    public EventReference GetUpFootstepSound;
    public EventReference TiredSound;
    public EventReference TiredGasSound;
}