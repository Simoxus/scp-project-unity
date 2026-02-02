using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData_UI", menuName = "Custom/FMOD/UI Audio Data")]
public class AudioData_UI : ScriptableObject
{
    [Space]
    public EventReference PressSound;
    public EventReference PressFailSound;
    public EventReference PressTooltipSound;
    public EventReference SliderSnapSound;
}