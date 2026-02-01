using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AnaglyphFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Shader shader;
    }

    public Settings settings = new Settings();
    private AnaglyphPass pass;
    private Material material;

    public override void Create()
    {
        if (settings.shader != null)
        {
            material = new Material(settings.shader);
            pass = new AnaglyphPass(material);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null || settings.shader == null) return;

        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }
}