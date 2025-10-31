using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class AnaglyphPass : ScriptableRenderPass
{
    private Material material;
    private const string PROFILER_TAG = "Anaglyph Effect";

    private static readonly int LeftTintID = Shader.PropertyToID("_LeftTint");
    private static readonly int RightTintID = Shader.PropertyToID("_RightTint");
    private static readonly int SeparationID = Shader.PropertyToID("_Separation");

    private class PassData
    {
        public Material material;
        public TextureHandle source;
        public TextureHandle destination;
        public Color leftTint;
        public Color rightTint;
        public float separation;
    }

    public AnaglyphPass(Material material)
    {
        this.material = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (material == null) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        // Get volume settings
        var stack = VolumeManager.instance.stack;
        var effect = stack.GetComponent<Anaglyph>();

        if (effect == null || !effect.IsActive())
            return;

        var desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;

        TextureHandle tempHandle = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, desc, "_AnaglyphTemp", false
        );

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PROFILER_TAG, out var passData))
        {
            passData.material = material;
            passData.source = resourceData.activeColorTexture;
            passData.destination = tempHandle;
            passData.leftTint = effect.leftTint.value;
            passData.rightTint = effect.rightTint.value;
            passData.separation = effect.separation.value;

            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(passData.destination, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                data.material.SetColor(LeftTintID, data.leftTint);
                data.material.SetColor(RightTintID, data.rightTint);
                data.material.SetFloat(SeparationID, data.separation);

                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        // Copy result back to camera
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Output Anaglyph", out var passData))
        {
            passData.source = tempHandle;
            passData.destination = resourceData.activeColorTexture;

            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(passData.destination, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }

    public void Dispose()
    {
        CoreUtils.Destroy(material);
    }
}