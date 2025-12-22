using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/Accumulation Blur")]
public class AccumulationBlur : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Blur power/intensity (0 = no blur, 1 = maximum ghosting). Matches DreamFilter's EntityAlpha parameter.")]
    public ClampedFloatParameter blurPower = new ClampedFloatParameter(0.5f, 0f, 0.99f);

    [Tooltip("Decay rate per frame (0.5 = fast fade, 1.0 = no fade). Higher values = longer, more persistent trails.")]
    public ClampedFloatParameter decay = new ClampedFloatParameter(0.99f, 0.5f, 1f);

    [Tooltip("How strongly decay is applied (0 = no decay effect, 1 = full decay). Use this to fine-tune trail persistence.")]
    public ClampedFloatParameter decayStrength = new ClampedFloatParameter(1.0f, 0f, 1f);

    [Tooltip("Desaturation amount (0 = color, 1 = black and white)")]
    public ClampedFloatParameter desaturation = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Tint color applied to blur trails")]
    public ColorParameter tintColor = new ColorParameter(Color.white, false, false, false);

    public bool IsActive() => blurPower.overrideState && blurPower.value > 0.01f;
    public bool IsTileCompatible() => false;
}