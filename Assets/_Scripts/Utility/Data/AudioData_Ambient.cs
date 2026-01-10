using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Ambient", menuName = "Custom/FMOD/Ambient Audio Data")]
public class AudioData_Ambient : ScriptableObject
{
    public EventReference EntranceLoopSound;
    public EventReference ForestLoopSound;
    public EventReference GeneralLoopSound;
    public EventReference HeavyContainmentLoopSound;
    public EventReference LightContainmentLoopSound;
    public EventReference PrebreachLoopSound;
}