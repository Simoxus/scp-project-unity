using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/Accumulation Blur")]
public class AccumulationBlur : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Blur power/intensity (0 = no blur, 1 = maximum ghosting)")]
    public ClampedFloatParameter blurPower = new ClampedFloatParameter(0.5f, 0f, 0.99f);

    [Tooltip("Desaturation amount (0 = color, 1 = black and white)")]
    public ClampedFloatParameter desaturation = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Tint color applied to blur trails")]
    public ColorParameter tintColor = new ColorParameter(Color.white, false, false, false);

    public bool IsActive() => blurPower.value > 0.01f;
    public bool IsTileCompatible() => false;
}