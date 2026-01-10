using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/Gaussian Blur")]
public class GaussianBlur : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 10f);
    public ClampedIntParameter iterations = new ClampedIntParameter(2, 1, 4);

    public bool IsActive() => intensity.overrideState && intensity.value > 0.01f;
    public bool IsTileCompatible() => false;
}