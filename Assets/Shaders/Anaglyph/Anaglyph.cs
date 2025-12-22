using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/Anaglyph Effect")]
public class Anaglyph : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Left eye tint color (typically red)")]
    public ColorParameter leftTint = new ColorParameter(new Color(1, 0, 0, 1), false, false, false);

    [Tooltip("Right eye tint color (typically cyan)")]
    public ColorParameter rightTint = new ColorParameter(new Color(0, 1, 1, 1), false, false, false);

    [Tooltip("Channel separation distance")]
    public ClampedFloatParameter separation = new ClampedFloatParameter(0.005f, 0f, 0.02f);

    public bool IsActive() => separation.overrideState && separation.value > 0f;
    public bool IsTileCompatible() => false;
}