using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class AccumulationBlurPass : ScriptableRenderPass
{
    private Material material;
    private RTHandle accumulationTexture;
    private AccumulationBlurFeature.Settings settings;

    private const string PROFILER_TAG = "Accumulation Blur";

    private static readonly int AccumulationTexID = Shader.PropertyToID("_AccumulationTex");
    private static readonly int CurrentFrameID = Shader.PropertyToID("_CurrentFrame");
    private static readonly int BlurPowerID = Shader.PropertyToID("_BlurPower");
    private static readonly int DecayID = Shader.PropertyToID("_Decay");
    private static readonly int DecayStrengthID = Shader.PropertyToID("_DecayStrength");
    private static readonly int DesaturationID = Shader.PropertyToID("_Desaturation");
    private static readonly int TintColorID = Shader.PropertyToID("_TintColor");

    private bool _isFirstFrame = true;

    private class PassData
    {
        public Material material;
        public TextureHandle source;
        public TextureHandle accumulation;
        public TextureHandle temp;
        public TextureHandle destination;
        public float blurPower;
        public float decay;
        public float decayStrength;
        public float desaturation;
        public Color tintColor;
    }

    public AccumulationBlurPass(AccumulationBlurFeature.Settings settings)
    {
        this.settings = settings;
        renderPassEvent = settings.renderPassEvent;

        if (settings.shader != null)
            material = CoreUtils.CreateEngineMaterial(settings.shader);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (material == null) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        // Get volume settings
        var stack = VolumeManager.instance.stack;
        var effect = stack.GetComponent<AccumulationBlur>();

        if (effect == null || !effect.IsActive())
        {
            _isFirstFrame = true;
            accumulationTexture?.Release();
            accumulationTexture = null;
            return;
        }

        // Allocate accumulation texture (feedback buffer)
        var desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;
        desc.useMipMap = false;
        desc.autoGenerateMips = false;
        desc.colorFormat = RenderTextureFormat.DefaultHDR;

        RenderingUtils.ReAllocateHandleIfNeeded(
            ref accumulationTexture,
            desc,
            FilterMode.Bilinear,
            TextureWrapMode.Clamp,
            name: "_AccumulationTexture"
        );

        // First frame: Initialize accumulation buffer with current frame
        if (_isFirstFrame)
        {
            _isFirstFrame = false;

            TextureHandle accumulationHandle = renderGraph.ImportTexture(accumulationTexture);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Init Accumulation", out var passData))
            {
                passData.source = resourceData.activeColorTexture;
                passData.destination = accumulationHandle;

                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.SetRenderAttachment(passData.destination, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }

            return;
        }

        // Normal operation: Apply decay to accumulated frame, then blend with current
        TextureHandle accumulationHandle2 = renderGraph.ImportTexture(accumulationTexture);
        TextureHandle tempHandle = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, desc, "_TempAccumulation", false
        );

        // Step 1: Apply decay and effects to accumulated buffer, then blend with current frame
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PROFILER_TAG, out var passData))
        {
            passData.material = material;
            passData.source = resourceData.activeColorTexture;
            passData.accumulation = accumulationHandle2;
            passData.temp = tempHandle;
            passData.blurPower = effect.blurPower.value;
            passData.decay = effect.decay.value;
            passData.decayStrength = effect.decayStrength.value;
            passData.desaturation = effect.desaturation.value;
            passData.tintColor = effect.tintColor.value;

            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.UseTexture(passData.accumulation, AccessFlags.Read);
            builder.SetRenderAttachment(passData.temp, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);
            builder.AllowGlobalStateModification(true);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                // Apply decay, desaturation, tint, then blend with current frame
                data.material.SetFloat(BlurPowerID, data.blurPower);
                data.material.SetFloat(DecayID, data.decay);
                data.material.SetFloat(DecayStrengthID, data.decayStrength);
                data.material.SetFloat(DesaturationID, data.desaturation);
                data.material.SetColor(TintColorID, data.tintColor);
                data.material.SetTexture(AccumulationTexID, data.accumulation);
                data.material.SetTexture(CurrentFrameID, data.source);
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        // Step 2: Copy result back to accumulation texture
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Update Accumulation", out var passData))
        {
            passData.source = tempHandle;
            passData.destination = accumulationHandle2;

            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(passData.destination, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
            });
        }

        // Step 3: Output to camera
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Output Accumulation", out var passData))
        {
            passData.source = accumulationHandle2;
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
        accumulationTexture?.Release();
        CoreUtils.Destroy(material);
    }
}