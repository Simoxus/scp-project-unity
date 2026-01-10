using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Music", menuName = "Custom/FMOD/Music Audio Data")]
public class AudioData_Music : ScriptableObject
{
    public EventReference CreditsMusic;
    public EventReference EntranceMusic;
    public EventReference HeavyContainmentMusic;
    public EventReference LightContainmentMusic;
    public EventReference MainMenuMusic;
}