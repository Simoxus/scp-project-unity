using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_SCPs", menuName = "Custom/FMOD/SCPs Audio Data")]
public class AudioData_SCPs : ScriptableObject
{
    [Header("SCP-008")]

    [Header("SCP-049")]
    public EventReference SCP049VoiceSound;
    public EventReference SCP049FootstepSound;

    [Header("SCP-096")]
    public EventReference SCP096IdleSound;
    public EventReference SCP096EnragedSound;
    public EventReference SCP096ScreamSound;

    [Header("SCP-106")]
    public EventReference SCP106AmbientSound;
    public EventReference SCP106ChaseSound;
    public EventReference SCP106CorrosionSound;
    public EventReference SCP106SinkholeSound;
    public EventReference SCP106SinkholeFallSound;

    [Header("SCP-173")]
    public EventReference SCP173MovementSpeed;
    public EventReference SCP173NeckBreakSound;
    public EventReference SCP173StingerSound;

    [Header("SCP-939")]
    public EventReference SCP939BreathSound;
    public EventReference SCP939GrowlSound;
    public EventReference SCP939AttackSound;
}