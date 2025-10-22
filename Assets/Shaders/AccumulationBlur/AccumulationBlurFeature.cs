using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AccumulationBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("When to apply the effect")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Tooltip("The AccumulationBlur shader")]
        public Shader shader;
    }

    public Settings settings = new Settings();
    private AccumulationBlurPass pass;

    public override void Create()
    {
        pass = new AccumulationBlurPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.shader == null)
        {
            Debug.LogWarning("AccumulationBlurFeature: Shader is not assigned!");
            return;
        }

        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }
}