using UnityEngine;
using System.Collections;

namespace VLB
{
    [ExecuteInEditMode]
    [HelpURL(Consts.Help.SD.UrlDynamicOcclusionDepthBuffer)]
    [AddComponentMenu(Consts.Help.SD.AddComponentMenuDynamicOcclusionDepthBuffer)]
    public class DynamicOcclusionDepthBuffer : DynamicOcclusionAbstractBase
    {
        public new const string ClassName = "DynamicOcclusionDepthBuffer";

        /// <summary>
        /// The beam can only be occluded by objects located on the layers matching this mask.
        /// It's very important to set it as restrictive as possible (checking only the layers which are necessary)
        /// to perform a more efficient process in order to increase the performance.
        /// It should NOT include the layer on which the beams are generated.
        /// </summary>
        public LayerMask layerMask = Consts.DynOcclusion.LayerMaskDefault;

        /// <summary>
        /// Whether or not the virtual camera will use occlusion culling during rendering from the beam's POV.
        /// </summary>
        public bool useOcclusionCulling = Consts.DynOcclusion.DepthBufferOcclusionCullingDefault;

        /// <summary>
        /// Controls how large the depth texture captured by the virtual camera is.
        /// The lower the resolution, the better the performance, but the less accurate the rendering.
        /// </summary>
        public int depthMapResolution = Consts.DynOcclusion.DepthBufferDepthMapResolutionDefault;

        /// <summary>
        /// Fade out the beam before the occlusion surface in order to soften the transition.
        /// </summary>
        public float fadeDistanceToSurface = Consts.DynOcclusion.DepthBufferFadeDistanceToSurfaceDefault;


        protected override string GetShaderKeyword() { return ShaderKeywords.SD.OcclusionDepthTexture; }
        protected override MaterialManager.SD.DynamicOcclusion GetDynamicOcclusionMode() { return MaterialManager.SD.DynamicOcclusion.DepthTexture; }

        Camera m_DepthCamera = null;
        bool m_NeedToUpdateOcclusionNextFrame = false;

        void ProcessOcclusionInternal()
        {
            UpdateDepthCameraPropertiesAccordingToBeam();
            m_DepthCamera.Render();
        }

        protected override bool OnProcessOcclusion(ProcessOcclusionSource source)
        {
            Debug.Assert(m_Master && m_DepthCamera);

            if (SRPHelper.IsUsingCustomRenderPipeline()) // Recursive rendering is not supported on SRP
                m_NeedToUpdateOcclusionNextFrame = true;
            else
                ProcessOcclusionInternal();

            return true;
        }

        void Update()
        {
            if (m_NeedToUpdateOcclusionNextFrame && m_Master && m_DepthCamera
                && Time.frameCount > 1)  // fix NullReferenceException in UnityEngine.Rendering.Universal.Internal.CopyDepthPass.Execute when using SRP
            {
                ProcessOcclusionInternal();
                m_NeedToUpdateOcclusionNextFrame = false;
            }
        }

        void UpdateDepthCameraPropertiesAccordingToBeam()
        {
            Debug.Assert(m_Master);

            Utils.SetupDepthCamera(m_DepthCamera
            , m_Master.coneApexOffsetZ, m_Master.maxGeometryDistance, m_Master.coneRadiusStart, m_Master.coneRadiusEnd
            , m_Master.beamLocalForward, m_Master.GetLossyScale(), m_Master.IsScalable(), m_Master.beamInternalLocalRotation
            , true);
        }

        public bool HasLayerMaskIssues()
        {
            if(Config.Instance.geometryOverrideLayer)
            {
                int layerBit = 1 << Config.Instance.geometryLayerID;
                return ((layerMask.value & layerBit) == layerBit);
            }
            return false;
        }

        protected override void OnValidateProperties()
        {
            base.OnValidateProperties();
            depthMapResolution = Mathf.Clamp(Mathf.NextPowerOfTwo(depthMapResolution), 8, 2048);
            fadeDistanceToSurface = Mathf.Max(fadeDistanceToSurface, 0f);
        }

        void InstantiateOrActivateDepthCamera()
        {
            if (m_DepthCamera != null)
            {
                m_DepthCamera.gameObject.SetActive(true); // active it in case it has been disabled by OnDisable()
            }
            else
            {
                // delete old depth cameras when duplicating the GAO
                gameObject.ForeachComponentsInDirectChildrenOnly<Camera>(cam => DestroyImmediate(cam.gameObject), true);

                m_DepthCamera = Utils.NewWithComponent<Camera>("Depth Camera");

                if (m_DepthCamera && m_Master)
                {
                    m_DepthCamera.enabled = false;
                    m_DepthCamera.cullingMask = layerMask;
                    m_DepthCamera.clearFlags = CameraClearFlags.Depth;
                    m_DepthCamera.depthTextureMode = DepthTextureMode.Depth;
                    m_DepthCamera.renderingPath = RenderingPath.VertexLit; // faster
                    m_DepthCamera.useOcclusionCulling = useOcclusionCulling;
                    m_DepthCamera.gameObject.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
                    m_DepthCamera.transform.SetParent(transform, false);
                    Config.Instance.SetURPScriptableRendererIndexToDepthCamera(m_DepthCamera);

                    var rt = new RenderTexture(depthMapResolution, depthMapResolution, 16, RenderTextureFormat.Depth);
                    m_DepthCamera.targetTexture = rt;

                    UpdateDepthCameraPropertiesAccordingToBeam();

#if UNITY_EDITOR
                    UnityEditor.GameObjectUtility.SetStaticEditorFlags(m_DepthCamera.gameObject, m_Master.GetStaticEditorFlagsForSubObjects());
                    m_DepthCamera.gameObject.SetSameSceneVisibilityStatesThan(m_Master.gameObject);
#endif
                }
            }
        }

        protected override void OnEnablePostValidate()
        {
            InstantiateOrActivateDepthCamera();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_DepthCamera) m_DepthCamera.gameObject.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            MarkMaterialAsDirty();
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestroyDepthCamera();

#if UNITY_EDITOR
            MarkMaterialAsDirty();
#endif
        }

        void DestroyDepthCamera()
        {
            if (m_DepthCamera)
            {
                if (m_DepthCamera.targetTexture)
                {
                    m_DepthCamera.targetTexture.Release();
                    DestroyImmediate(m_DepthCamera.targetTexture);
                    m_DepthCamera.targetTexture = null;
                }

                DestroyImmediate(m_DepthCamera.gameObject); // Make sure to delete the GAO
                m_DepthCamera = null;
            }
        }

        protected override void OnModifyMaterialCallback(MaterialModifier.Interface owner)
        {
            Debug.Assert(owner != null);
            owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionDepthTexture, m_DepthCamera.targetTexture);
            var scale = m_Master.GetLossyScale();
            owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionDepthProps, new Vector4(Mathf.Sign(scale.x) * Mathf.Sign(scale.z), Mathf.Sign(scale.y), fadeDistanceToSurface, m_DepthCamera.orthographic ? 0f : 1f));
        }

#if UNITY_EDITOR
        bool m_NeedToReinstantiateDepthCamera = false;

        public void ForceReinstantiateDepthCamera()
        {
            m_NeedToReinstantiateDepthCamera = true;
        }

        void MarkMaterialAsDirty()
        {
            // when adding/removing this component in editor, we might need to switch from a GPU Instanced material to a custom one,
            // since this feature doesn't support GPU Instancing
            if (!Application.isPlaying)
                m_Master._EditorSetBeamGeomDirty();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_NeedToReinstantiateDepthCamera = true;
        }

        void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                if (m_NeedToReinstantiateDepthCamera)
                {
                    DestroyDepthCamera();
                    InstantiateOrActivateDepthCamera();
                    m_NeedToReinstantiateDepthCamera = false;
                }

                if(m_Master && m_Master.enabled)
                    ProcessOcclusion(ProcessOcclusionSource.EditorUpdate);
            }
        }
#endif // UNITY_EDITOR
    }
}
