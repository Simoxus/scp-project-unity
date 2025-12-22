using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FootstepData", menuName = "Custom/Player Footstep Data")]
public class FootstepData : ScriptableObject
{
    public List<Texture> textures = new List<Texture>();
    public string fmodParameterLabel;
    public float fmodParameterValue;
}
