using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Music", menuName = "Custom/FMOD/Music Audio Data")]
public class AudioData_Music : ScriptableObject
{
    [Space]
    public EventReference MainMenuMusic;
    public EventReference CreditsMusic;
    public EventReference IntroMusic;
    public EventReference EntranceZoneMusic;
    public EventReference HeavyContainmentZoneMusic;
    public EventReference LightContainmentZoneMusic;
}