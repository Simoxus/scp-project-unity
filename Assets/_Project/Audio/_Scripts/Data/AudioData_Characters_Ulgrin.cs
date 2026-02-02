using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_Characters_Ulgrin", menuName = "Custom/FMOD/Agent Ulgrin Audio Data")]
public class AudioData_Characters_Ulgrin : ScriptableObject
{
    [Space]
    public EventReference BeforeCellOpen;
    public EventReference ExitCellRequest;
    public EventReference ExitCellRefuseA;
    public EventReference ExitCellRefuseB;
    public EventReference ExitCellKillA;
    public EventReference ExitCellKillB;

    [Header("Escort")]
    public EventReference EscortStartA;
    public EventReference EscortStartB;
    public EventReference EscortWrongWay;
    public EventReference EscortKillA;
    public EventReference EscortKillB;
    public EventReference EscortPissedA;
    public EventReference EscortPissedB;
    public EventReference EscortRefuseA;
    public EventReference EscortRefuseB;
    public EventReference EscortDone1A;
    public EventReference EscortDone2B;
    public EventReference EscortDone3C;
    public EventReference EscortDone2;
    public EventReference EscortDone3;
    public EventReference EscortDone4;
    public EventReference EscortConvoA;
    public EventReference EscortConvoB;
    public EventReference EscortConvoC;
    public EventReference EscortConvoD;
    public EventReference EscortConvoE;
}