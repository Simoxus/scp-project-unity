using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Props", menuName = "Custom/FMOD/Prop Audio Data")]
public class AudioData_Props : ScriptableObject
{
    [Space]
    public EventReference CameraSound;
    public EventReference GasHissSound;
}