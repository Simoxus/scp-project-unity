using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/Gaussian Blur")]
public class GaussianBlur : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Blur intensity (0 = no blur, 10 = maximum blur)")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 10f);

    [Tooltip("Number of blur iterations for smoother results (1-4)")]
    public ClampedIntParameter iterations = new ClampedIntParameter(2, 1, 4);

    public bool IsActive() => intensity.overrideState && intensity.value > 0.01f;
    public bool IsTileCompatible() => false;
}