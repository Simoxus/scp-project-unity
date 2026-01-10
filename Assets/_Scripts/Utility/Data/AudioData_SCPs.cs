using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_SCPs", menuName = "Custom/FMOD/SCPs Audio Data")]
public class AudioData_SCPs : ScriptableObject
{
    [Header("SCP-106")]
    public EventReference SinkholeSound;
    public EventReference SinkholeFallSound;
}