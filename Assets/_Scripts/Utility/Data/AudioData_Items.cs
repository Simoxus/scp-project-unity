using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Items", menuName = "Custom/FMOD/Items Audio Data")]
public class AudioData_Items : ScriptableObject
{
    [Header("General")]
    public EventReference ItemPick1Sound;
    public EventReference ItemPick2Sound;
    public EventReference ItemPickDocSound;
    public EventReference ItemPickMiscSound;
}