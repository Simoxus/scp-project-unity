using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class GaussianBlurPass : ScriptableRenderPass
{
    private Material blurMaterial;
    private const string PROFILER_TAG = "Gaussian Blur";

    private static readonly int GridSizeID = Shader.PropertyToID("_GridSize");
    private static readonly int SpreadID = Shader.PropertyToID("_Spread");

    private class PassData
    {
        public Material material;
        public TextureHandle source;
        public TextureHandle destination;
        public int gridSize;
        public float spread;
    }

    public GaussianBlurPass(Shader shader, RenderPassEvent renderPassEvent)
    {
        this.renderPassEvent = renderPassEvent;

        if (shader != null)
        {
            blurMaterial = CoreUtils.CreateEngineMaterial(shader);
        }
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (blurMaterial == null) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        // Get volume settings
        var stack = VolumeManager.instance.stack;
        var effect = stack.GetComponent<GaussianBlur>();

        if (effect == null || !effect.IsActive())
        {
            return;
        }

        if (!resourceData.isActiveTargetBackBuffer)
        {
            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            // Calculate grid size from strength (standard deviation)
            float strength = effect.intensity.value;
            int gridSize = Mathf.CeilToInt(strength * 6.0f);

            // Ensure grid has a center pixel (odd number)
            if (gridSize % 2 == 0)
            {
                gridSize++;
            }

            // Ensure minimum grid size of 1
            gridSize = Mathf.Max(1, gridSize);

            TextureHandle source = resourceData.activeColorTexture;
            TextureHandle temp = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, desc, "_TempBlurRT", false
            );

            int iterations = effect.iterations.value;

            // Perform blur iterations
            for (int i = 0; i < iterations; i++)
            {
                // Horizontal pass
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    $"{PROFILER_TAG} Horizontal {i}", out var passData))
                {
                    passData.material = blurMaterial;
                    passData.source = source;
                    passData.destination = temp;
                    passData.gridSize = gridSize;
                    passData.spread = strength;

                    builder.UseTexture(passData.source, AccessFlags.Read);
                    builder.SetRenderAttachment(passData.destination, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        data.material.SetInteger(GridSizeID, data.gridSize);
                        data.material.SetFloat(SpreadID, data.spread);
                        Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                    });
                }

                // Vertical pass
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    $"{PROFILER_TAG} Vertical {i}", out var passData))
                {
                    passData.material = blurMaterial;
                    passData.source = temp;
                    passData.destination = source;
                    passData.gridSize = gridSize;
                    passData.spread = strength;

                    builder.UseTexture(passData.source, AccessFlags.Read);
                    builder.SetRenderAttachment(passData.destination, 0, AccessFlags.Write);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        data.material.SetInteger(GridSizeID, data.gridSize);
                        data.material.SetFloat(SpreadID, data.spread);
                        Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 1);
                    });
                }
            }
        }
    }

    public void Dispose()
    {
        CoreUtils.Destroy(blurMaterial);
    }
}