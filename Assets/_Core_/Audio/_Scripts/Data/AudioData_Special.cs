using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Special", menuName = "Custom/FMOD/Special Audio Data")]
public class AudioData_Special : ScriptableObject
{
    [Space]
    public EventReference FcvenySound;
    public EventReference SCP420JSong;
}