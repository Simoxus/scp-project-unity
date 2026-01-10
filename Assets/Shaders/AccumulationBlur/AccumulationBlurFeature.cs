using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AccumulationBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
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
        if (settings.shader == null) return;

        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }
}