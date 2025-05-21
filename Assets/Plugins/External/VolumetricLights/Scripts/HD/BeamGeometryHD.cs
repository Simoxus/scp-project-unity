#if DEBUG
//#define DEBUG_SHOW_MESH_NORMALS
#endif
#define FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE

#if UNITY_2018_1_OR_NEWER
#define VLB_SRP_SUPPORT // Comment this to disable SRP support
#endif

using UnityEngine;
using System.Collections;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [AddComponentMenu("")] // hide it from Component search
    [ExecuteInEditMode]
    [HelpURL(Consts.Help.HD.UrlBeam)]
    public class BeamGeometryHD : BeamGeometryAbstractBase
    {
        VolumetricLightBeamHD m_Master = null;
        VolumetricCookieHD m_Cookie = null;
        VolumetricShadowHD m_Shadow = null;

        protected override VolumetricLightBeamAbstractBase GetMaster() { return m_Master; }

        public bool visible
        {
            set { if (meshRenderer) meshRenderer.enabled = value; }
        }

        public int sortingLayerID
        {
            set { if (meshRenderer) meshRenderer.sortingLayerID = value; }
        }

        public int sortingOrder
        {
            set { if(meshRenderer) meshRenderer.sortingOrder = value; }
        }

#if VLB_SRP_SUPPORT
        Camera m_CurrentCameraRenderingSRP = null;

        void OnDisable()
        {
            SRPHelper.UnregisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
            m_CurrentCameraRenderingSRP = null;
        }

        public static bool isCustomRenderPipelineSupported { get { return true; } }
#else
        public static bool isCustomRenderPipelineSupported { get { return false; } }
#endif

        bool shouldUseGPUInstancedMaterial
        {
            get
            {
                if (Config.Instance.GetActualRenderingMode(ShaderMode.HD) == RenderingMode.GPUInstancing)
                {
                    return m_Cookie == null && m_Shadow == null; // sampler cannot be passed to shader as instanced property
                }
                return false;
            }
        }

        void OnEnable()
        {
#if VLB_SRP_SUPPORT
            SRPHelper.RegisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
#endif
        }

        public void Initialize(VolumetricLightBeamHD master)
        {
            Debug.Assert(master != null);

            var customHideFlags = Consts.Internal.ProceduralObjectsHideFlags;
            m_Master = master;

            transform.SetParent(master.transform, false);

            meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
            meshRenderer.hideFlags = customHideFlags;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off; // different reflection probes could break batching with GPU Instancing
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

            m_Cookie = m_Master.GetAdditionalComponentCookie();
            m_Shadow = m_Master.GetAdditionalComponentShadow();

            if (!shouldUseGPUInstancedMaterial)
            {
                m_CustomMaterial = Config.Instance.NewMaterialTransient(ShaderMode.HD, gpuInstanced:false);
                ApplyMaterial();
            }

            if (m_Master.DoesSupportSorting2D())
            {
                if (SortingLayer.IsValid(m_Master.GetSortingLayerID()))
                    sortingLayerID = m_Master.GetSortingLayerID();
                else
                    Debug.LogError(string.Format("Beam '{0}' has an invalid sortingLayerID ({1}). Please fix it by setting a valid layer.", Utils.GetPath(m_Master.transform), m_Master.GetSortingLayerID()));

                sortingOrder = m_Master.GetSortingOrder();
            }

            meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            meshFilter.hideFlags = customHideFlags;

            gameObject.hideFlags = customHideFlags;

#if UNITY_EDITOR
            UnityEditor.GameObjectUtility.SetStaticEditorFlags(gameObject, master.GetStaticEditorFlagsForSubObjects());
            gameObject.SetSameSceneVisibilityStatesThan(master.gameObject);
#endif
        }

        /// <summary>
        /// Generate the cone mesh and calls UpdateMaterialAndBounds.
        /// Since this process involves recreating a new mesh, make sure to not call it at every frame during playtime.
        /// </summary>
        public void RegenerateMesh()
        {
            Debug.Assert(m_Master);

            if (Config.Instance.geometryOverrideLayer)
                gameObject.layer = Config.Instance.geometryLayerID;
            else
                gameObject.layer = m_Master.gameObject.layer;

            gameObject.tag = Config.Instance.geometryTag;

            coneMesh = GlobalMeshHD.Get();
            meshFilter.sharedMesh = coneMesh;

            UpdateMaterialAndBounds();
        }

        Vector3 ComputeLocalMatrix()
        {
            // In the VS, we compute the vertices so the whole beam fits into a fixed 2x2x1 box.
            // We have to apply some scaling to get the proper beam size.
            // This way we have the proper bounds without having to recompute specific bounds foreach beam.
            var maxRadius = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);

            var localScale = new Vector3(maxRadius, maxRadius, m_Master.maxGeometryDistance);
            if (!m_Master.scalable)
                localScale = localScale.Divide(m_Master.GetLossyScale());

            transform.localScale = localScale;
            transform.localRotation = m_Master.beamInternalLocalRotation;

            return localScale;
        }

        bool isNoiseEnabled { get { return m_Master.isNoiseEnabled && m_Master.noiseIntensity > 0f && Noise3D.isSupported; } } // test Noise3D.isSupported the last

        MaterialManager.StaticPropertiesHD ComputeMaterialStaticProperties()
        {
            var colorGradient = MaterialManager.ColorGradient.Off;
            if (m_Master.colorMode == ColorMode.Gradient)
            {
                var precision = Utils.GetFloatPackingPrecision();
                colorGradient = precision == Utils.FloatPackingPrecision.High ? MaterialManager.ColorGradient.MatrixHigh : MaterialManager.ColorGradient.MatrixLow;
            }

            Debug.Assert((int)BlendingMode.Additive == (int)MaterialManager.BlendingMode.Additive);
            Debug.Assert((int)BlendingMode.SoftAdditive == (int)MaterialManager.BlendingMode.SoftAdditive);
            Debug.Assert((int)BlendingMode.TraditionalTransparency == (int)MaterialManager.BlendingMode.TraditionalTransparency);

            return new MaterialManager.StaticPropertiesHD
            {
                blendingMode = (MaterialManager.BlendingMode)m_Master.blendingMode,
                attenuation = m_Master.attenuationEquation == AttenuationEquationHD.Linear ? MaterialManager.HD.Attenuation.Linear : MaterialManager.HD.Attenuation.Quadratic,
                noise3D = isNoiseEnabled ? MaterialManager.Noise3D.On : MaterialManager.Noise3D.Off,
                colorGradient = colorGradient,
                shadow = m_Shadow != null ? MaterialManager.HD.Shadow.On : MaterialManager.HD.Shadow.Off,
                cookie = (m_Cookie != null ? (m_Cookie.channel == CookieChannel.RGBA ? MaterialManager.HD.Cookie.RGBA : MaterialManager.HD.Cookie.SingleChannel) : MaterialManager.HD.Cookie.Off),
                raymarchingQualityIndex = m_Master.raymarchingQualityIndex
            };
        }

        bool ApplyMaterial()
        {
            var staticProps = ComputeMaterialStaticProperties();

            Material mat = null;
            if (!shouldUseGPUInstancedMaterial)
            {
                mat = m_CustomMaterial;
                if(mat)
                    staticProps.ApplyToMaterial(mat);
            }
            else
            {
                mat = MaterialManager.GetInstancedMaterial(m_Master._INTERNAL_InstancedMaterialGroupID, ref staticProps);
            }

            meshRenderer.material = mat;
            return mat != null;
        }

#if DEBUG
        bool m_CanChangePropertyBlock = false;
#endif

        public void SetMaterialProp(int nameID, float value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetFloat(nameID, value);
            else
            {
#if DEBUG
                Debug.Assert(m_CanChangePropertyBlock == true);
#endif
                MaterialManager.materialPropertyBlock.SetFloat(nameID, value);
            }
        }

        public void SetMaterialProp(int nameID, Vector4 value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetVector(nameID, value);
            else
            {
#if DEBUG
                Debug.Assert(m_CanChangePropertyBlock == true);
#endif
                MaterialManager.materialPropertyBlock.SetVector(nameID, value);
            }
        }

        public void SetMaterialProp(int nameID, Color value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetColor(nameID, value);
            else
            {
#if DEBUG
                Debug.Assert(m_CanChangePropertyBlock == true);
#endif
                MaterialManager.materialPropertyBlock.SetColor(nameID, value);
            }
        }

        public void SetMaterialProp(int nameID, Matrix4x4 value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetMatrix(nameID, value);
            else
            {
#if DEBUG
                Debug.Assert(m_CanChangePropertyBlock == true);
#endif
                MaterialManager.materialPropertyBlock.SetMatrix(nameID, value);
            }
        }

        public void SetMaterialProp(int nameID, Texture value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetTexture(nameID, value);
#if DEBUG
            else
            {
                Debug.Assert(m_CanChangePropertyBlock == true);
                Debug.LogErrorFormat(m_Master, "Setting a Texture property to a GPU instanced material is not supported: '{0}'", m_Master);
            }
#endif
        }

        public enum InvalidTexture
        {
            Null,
            NoDepth
        }

        public void SetMaterialProp(int nameID, InvalidTexture invalidTexture)
        {
            if (m_CustomMaterial)
            {
                Texture tex = null;
                if (invalidTexture == InvalidTexture.NoDepth)
                    tex = SystemInfo.usesReversedZBuffer? Texture2D.blackTexture: Texture2D.whiteTexture;

                m_CustomMaterial.SetTexture(nameID, tex);
            }
        }

        void MaterialChangeStart()
        {
            if (m_CustomMaterial == null)
                meshRenderer.GetPropertyBlock(MaterialManager.materialPropertyBlock);
#if DEBUG
            m_CanChangePropertyBlock = true;
#endif
        }

        void MaterialChangeStop()
        {
#if DEBUG
            m_CanChangePropertyBlock = false;
#endif
            if (m_CustomMaterial == null)
                meshRenderer.SetPropertyBlock(MaterialManager.materialPropertyBlock);
        }

        ////////////////////////
        /// DIRTY PROPERTIES
        ////////////////////////
        DirtyProps m_DirtyProps = DirtyProps.None;

        public void SetPropertyDirty(DirtyProps prop)
        {
            m_DirtyProps |= prop;

            if(prop.HasAtLeastOneFlag(DirtyProps.OnlyMaterialChangeOnly))
            {
                UpdateMaterialAndBounds(); // need to change material variant
            }
        }

        void UpdateMaterialAndBounds()
        {
            Debug.Assert(m_Master);

            if (ApplyMaterial() == false)
            {
                return;
            }

            MaterialChangeStart();
            {
                m_DirtyProps = DirtyProps.All; // make sure all props will be updated on next camera render

                if (isNoiseEnabled)
                {
                    Noise3D.LoadIfNeeded();
                }

                // make sure the bounds are good from the startup
                ComputeLocalMatrix(); // compute matrix before sending it to the shader

#if VLB_SRP_SUPPORT
                // This update is to make QA test 'ReflectionObliqueProjection' pass
                UpdateMatricesPropertiesForGPUInstancingSRP();
#endif
            }
            MaterialChangeStop();

#if DEBUG_SHOW_MESH_NORMALS
            for (int vertexInd = 0; vertexInd < coneMesh.vertexCount; vertexInd++)
            {
                var vertex = coneMesh.vertices[vertexInd];

                // apply modification done inside VS
                vertex.x *= Mathf.Lerp(coneRadius.x, coneRadius.y, vertex.z);
                vertex.y *= Mathf.Lerp(coneRadius.x, coneRadius.y, vertex.z);
                vertex.z *= m_Master.fallOffEnd;

                var cosSinFlat = new Vector2(vertex.x, vertex.y).normalized;
                var normal = new Vector3(cosSinFlat.x * Mathf.Cos(slopeRad), cosSinFlat.y * Mathf.Cos(slopeRad), -Mathf.Sin(slopeRad)).normalized;

                vertex = transform.TransformPoint(vertex);
                normal = transform.TransformDirection(normal);
                Debug.DrawRay(vertex, normal * 0.25f);
            }
#endif
        }

#if VLB_SRP_SUPPORT
        void UpdateMatricesPropertiesForGPUInstancingSRP()
        {
            if (SRPHelper.IsUsingCustomRenderPipeline() && Config.Instance.GetActualRenderingMode(ShaderMode.HD) == RenderingMode.GPUInstancing)
            {
                SetMaterialProp(ShaderProperties.LocalToWorldMatrix, transform.localToWorldMatrix);
                SetMaterialProp(ShaderProperties.WorldToLocalMatrix, transform.worldToLocalMatrix);
            }
        }

    #if UNITY_2019_1_OR_NEWER
        void OnBeginCameraRenderingSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
    #else
        void OnBeginCameraRenderingSRP(Camera cam)
    #endif
        {
            m_CurrentCameraRenderingSRP = cam;
        }
#endif

        void OnWillRenderObject()
        {
            Camera currentCam = null;

#if VLB_SRP_SUPPORT
            if (SRPHelper.IsUsingCustomRenderPipeline())
            {
                currentCam = m_CurrentCameraRenderingSRP;
            }
            else
#endif
            {
                currentCam = Camera.current;
            }

            OnWillCameraRenderThisBeam(currentCam);
        }

        void OnWillCameraRenderThisBeam(Camera cam)
        {
            if (m_Master && cam)
            {
                if (
#if UNITY_EDITOR
                    Utils.IsEditorCamera(cam) || // make sure to call UpdateCameraRelatedProperties for editor scene camera 
#endif
                    cam.enabled)    // prevent from doing stuff when we render from a previous DynamicOcclusionDepthBuffer's DepthCamera, because the DepthCamera are disabled 
                {
                    Debug.Assert(cam.GetComponentInParent<VolumetricLightBeamHD>() == null);
                    UpdateMaterialPropertiesForCamera(cam);

                    if (m_Shadow)
                        m_Shadow.OnWillCameraRenderThisBeam(cam, this);
                }
            }
        }

        void UpdateDirtyMaterialProperties()
        {
            if (m_DirtyProps != DirtyProps.None)
            {
                if (m_DirtyProps.HasFlag(DirtyProps.Intensity))
                {
                    SetMaterialProp(ShaderProperties.HD.Intensity, m_Master.intensity);
                }

                if (m_DirtyProps.HasFlag(DirtyProps.HDRPExposureWeight) && Config.Instance.isHDRPExposureWeightSupported)
                    {
                    SetMaterialProp(ShaderProperties.HDRPExposureWeight, m_Master.hdrpExposureWeight);
                }

                if (m_DirtyProps.HasFlag(DirtyProps.SideSoftness))
                {
                    SetMaterialProp(ShaderProperties.HD.SideSoftness, m_Master.sideSoftness);
                }

                if (m_DirtyProps.HasFlag(DirtyProps.Color))
                {
                    if (m_Master.colorMode == ColorMode.Flat)
                    {
                        SetMaterialProp(ShaderProperties.ColorFlat, m_Master.colorFlat);
                    }
                    else
                    {
                        var precision = Utils.GetFloatPackingPrecision();
                        m_ColorGradientMatrix = m_Master.colorGradient.SampleInMatrix((int)precision);
                        // pass the gradient matrix in OnWillRenderObject()
                    }
                }

                if (m_DirtyProps.HasFlag(DirtyProps.Cone))
                {
                    // kMinRadius and kMinApexOffset prevents artifacts when fresnel computation is done in the vertex shader
                    const float kMinRadius = 0.0001f;
                    var coneRadius = new Vector2(Mathf.Max(m_Master.coneRadiusStart, kMinRadius), Mathf.Max(m_Master.coneRadiusEnd, kMinRadius));
                    SetMaterialProp(ShaderProperties.ConeRadius, coneRadius);

                    const float kMinApexOffset = 0.0001f;
                    float apexOffsetZ = m_Master.GetConeApexOffsetZ(false);
                    float nonNullApex = Mathf.Sign(apexOffsetZ) * Mathf.Max(Mathf.Abs(apexOffsetZ), kMinApexOffset);
                    SetMaterialProp(ShaderProperties.ConeGeomProps, new Vector2(nonNullApex, Config.Instance.sharedMeshSides));

                    SetMaterialProp(ShaderProperties.DistanceFallOff, new Vector3(m_Master.fallOffStart, m_Master.fallOffEnd, m_Master.maxGeometryDistance));

                    ComputeLocalMatrix(); // compute matrix before sending it to the shader
                }

                if (m_DirtyProps.HasFlag(DirtyProps.Jittering))
                {
                    SetMaterialProp(ShaderProperties.HD.Jittering, new Vector4(m_Master.jitteringFactor, m_Master.jitteringFrameRate, m_Master.jitteringLerpRange.minValue, m_Master.jitteringLerpRange.maxValue));
                }

                if (isNoiseEnabled)
                {
                    if (m_DirtyProps.HasFlag(DirtyProps.NoiseMode) || m_DirtyProps.HasFlag(DirtyProps.NoiseIntensity))
                    {
                        SetMaterialProp(ShaderProperties.NoiseParam, new Vector2(
                            m_Master.noiseIntensity,
                            m_Master.noiseMode == NoiseMode.WorldSpace ? 0f : 1f));
                    }

                    if (m_DirtyProps.HasFlag(DirtyProps.NoiseVelocityAndScale))
                    {
                        var noiseVelocity = m_Master.noiseVelocityUseGlobal ? Config.Instance.globalNoiseVelocity : m_Master.noiseVelocityLocal;
                        var noiseScale = m_Master.noiseScaleUseGlobal ? Config.Instance.globalNoiseScale : m_Master.noiseScaleLocal;

                        SetMaterialProp(ShaderProperties.NoiseVelocityAndScale, new Vector4(
                            noiseVelocity.x,
                            noiseVelocity.y,
                            noiseVelocity.z,
                            noiseScale));
                    }
                }

                if (m_DirtyProps.HasFlag(DirtyProps.CookieProps))
                    VolumetricCookieHD.ApplyMaterialProperties(m_Cookie, this);

                if (m_DirtyProps.HasFlag(DirtyProps.ShadowProps))
                    VolumetricShadowHD.ApplyMaterialProperties(m_Shadow, this);
 
                m_DirtyProps = DirtyProps.None;
            }
        }

        void UpdateMaterialPropertiesForCamera(Camera cam)
        {
            if (cam && m_Master)
            {
                MaterialChangeStart();
                {
                    SetMaterialProp(ShaderProperties.HD.TransformScale, m_Master.scalable ? m_Master.GetLossyScale() : Vector3.one);

                    var camForwardVectorOSN = transform.InverseTransformDirection(cam.transform.forward).normalized;
                    SetMaterialProp(ShaderProperties.HD.CameraForwardOS, camForwardVectorOSN);
                    SetMaterialProp(ShaderProperties.HD.CameraForwardWS, cam.transform.forward);

                    UpdateDirtyMaterialProperties();

                    if (m_Master.colorMode == ColorMode.Gradient)
                    {
                        // Send the gradient matrix every frame since it's not a shader's property
                        SetMaterialProp(ShaderProperties.ColorGradientMatrix, m_ColorGradientMatrix);
                    }

#if VLB_SRP_SUPPORT
                    // This update is to be able to move beams without trackChangesDuringPlaytime enabled with SRP & GPU Instancing
                    UpdateMatricesPropertiesForGPUInstancingSRP();
#endif
                }
                MaterialChangeStop();

#if FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE
                cam.depthTextureMode |= DepthTextureMode.Depth;
#endif
            }
        }

#if UNITY_EDITOR
        public int  _EDITOR_InstancedMaterialID { get { return ComputeMaterialStaticProperties().GetMaterialID(); } }
#endif
    }
}
