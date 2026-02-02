using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/Accumulation Blur")]
public class AccumulationBlur : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter blurPower = new ClampedFloatParameter(0.5f, 0f, 0.99f);
    public ClampedFloatParameter decay = new ClampedFloatParameter(0.99f, 0.5f, 1f);
    public ClampedFloatParameter decayStrength = new ClampedFloatParameter(1.0f, 0f, 1f);
    public ClampedFloatParameter desaturation = new ClampedFloatParameter(0f, 0f, 1f);
    public ColorParameter tintColor = new ColorParameter(Color.white, false, false, false);

    public bool IsActive() => blurPower.overrideState && blurPower.value > 0.01f;
    public bool IsTileCompatible() => false;
}