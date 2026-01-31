using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Characters", menuName = "Custom/FMOD/Characters Audio Data")]
public class AudioData_Characters : ScriptableObject
{
    [Header("Agents")]
    public AudioData_Characters_Ulgrin Ulgrin;
    public EventReference AgentBalconySound;
    public EventReference AgentMusicLoverSound;
    public EventReference AgentRobertsSound;
    public EventReference AgentSadSound;
    public EventReference AgentSexyWhiteGuySound;

    [Header("Mobile Task Force")]
    public EventReference ApacheSound;
    public EventReference MtfSound;

    [Header("Personnel")]
    public EventReference D9341Sound;
    public EventReference DFemurVictimSound;
    public EventReference JanitorArnoldassSound;
    public EventReference ResearcherEmilySound;

    [Header("PA System")]
    public EventReference PaSound;
    public EventReference PaControlSound;
}