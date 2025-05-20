using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

using Cysharp.Threading.Tasks;
using PrimeTween;

public class Blur_TimedEffect : MonoBehaviour
{
    public EffectType EffectType => EffectType.Blurred;

    [SerializeField] private Volume postProcessingVolume; // Reference to volume component
    private MotionBlur motionBlur;

    private void Awake()
    {
        if (postProcessingVolume.profile.TryGet(out motionBlur))
        {
            motionBlur.active = false; // Initially turn off motion blur
        }
        else
        {
            Debug.LogWarning("MotionBlur effect not found in the volume profile.");
        }
    }

    public void PlayEffect(float strength = 1f)
    {
        _ = PlayEffectTimed(strength, 0.5f); // You can modify the duration here if needed
    }

    // This method is used for timed effects where blur will be active for the given duration.
    public async UniTask PlayEffectTimed(float strength, float duration)
    {
        if (motionBlur != null)
        {
            motionBlur.active = true; // Enable Motion Blur
            motionBlur.intensity.Override(strength); // Set intensity based on the effect strength

            // Wait time (in seconds) for the effect to go away
            await UniTask.WaitForSeconds(duration);

            // After the duration ends, fade away the Motion Blur effect gradually
        }
    }
}
