using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GaussianBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("When to apply the effect")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Tooltip("The GaussianBlur shader")]
        public Shader shader;
    }

    public Settings settings = new Settings();
    private GaussianBlurPass blurPass;

    public override void Create()
    {
        blurPass = new GaussianBlurPass(settings.shader, settings.renderPassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.shader == null)
        {
            Debug.LogWarning("GaussianBlurFeature: Shader is not assigned!");
            return;
        }

        renderer.EnqueuePass(blurPass);
    }

    protected override void Dispose(bool disposing)
    {
        blurPass?.Dispose();
    }
}