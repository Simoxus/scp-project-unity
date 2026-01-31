using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Alarms", menuName = "Custom/FMOD/Alarms Audio Data")]
public class AudioData_Alarms : ScriptableObject
{
    [Space]
    public EventReference AlarmSound;
    public EventReference AlarmAirlockSound;
    public EventReference AlarmAirlockBrokenSound;
    public EventReference AlarmWarheadSound;
}