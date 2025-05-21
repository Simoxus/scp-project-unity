using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LODGroup))]
    [HelpURL(Consts.Help.UrlLODBeamGroup)]
    public class LODBeamGroup : MonoBehaviour
    {
        [SerializeField] VolumetricLightBeamAbstractBase[] m_LODBeams = null;
        [SerializeField] bool m_ResetAllLODsLocalTransform = false;
        [SerializeField] BeamProps m_LOD0PropsToCopy = (BeamProps)~0;
        [SerializeField] bool m_CopyLOD0PropsEachFrame = false;
        [SerializeField] bool m_CullVolumetricDustParticles = true;

        LODGroup m_LODGroup;

#if UNITY_EDITOR
        GameObject m_SelectionToRestore = null;
#endif

        void Awake()
        {
            m_LODGroup = GetComponent<LODGroup>();
            SetupLodGroupData();
        }

        void Start()
        {
            UnifyBeamsProperties();
        }

        public LOD[] GetLODsFromLODGroup()
        {
            Debug.Assert(m_LODGroup != null);
            return m_LODGroup.GetLODs();
        }

        void SetLODRenderer(int lodIdx, Renderer renderer)
        {
            SetLODRenderers(lodIdx, renderer ? new Renderer[1] { renderer } : null);
        }

        void SetLODRenderers(int lodIdx, Renderer[] renderers)
        {
            Debug.Assert(m_LODGroup != null);

            LOD[] lods = m_LODGroup.GetLODs();
            Debug.Assert(lods != null);

#if UNITY_EDITOR
            if(lods[lodIdx].renderers == null || lods[lodIdx].renderers.Length == 0)
            {
                if(renderers != null)
                {
                    // Fix a very weird Unity bug happening on 2021 and higher, where the Unity's LODGroup inspector generates errors when modifying its LOD data.
                    // The only workaround I found is to deselect the gameobject for a bit of time.
                    m_SelectionToRestore = Selection.activeGameObject;
                    Selection.activeGameObject = null;
                }
            }
#endif
            lods[lodIdx].renderers = renderers;
            m_LODGroup.SetLODs(lods);
        }

        void SetLOD(int lodIdx)
        {
            Debug.Assert(m_LODGroup != null);
            Debug.Assert(m_LODBeams != null);

            LOD[] lods = m_LODGroup.GetLODs();
            Debug.Assert(lods != null);

            if (lods.IsValidIndex(lodIdx))
            {
                var beamGeom = m_LODBeams[lodIdx].GetBeamGeometry();
                if (beamGeom)
                {
                    var beamRenderer = beamGeom.meshRenderer;
                    if (beamRenderer)
                    {
                        if (m_CullVolumetricDustParticles)
                        {
                            var particlesComponent = m_LODBeams[lodIdx].GetComponent<VolumetricDustParticles>();
                            if (particlesComponent)
                            {
                                var particlesRenderer = particlesComponent.FindRenderer();
                                if (particlesRenderer)
                                {
                                    SetLODRenderers(lodIdx, new Renderer[2] { beamRenderer, particlesRenderer });
                                    return;
                                }
                            }
                        }

                        if (lods[lodIdx].renderers == null || lods[lodIdx].renderers.Length != 1 || lods[lodIdx].renderers[0] != beamRenderer)
                        {
                            SetLODRenderer(lodIdx, beamRenderer);
                        }
                    }
                }
            }
        }

        void OnBeamGeometryGenerated(VolumetricLightBeamAbstractBase beam)
        {
            Debug.Assert(m_LODGroup != null);

            LOD[] lods = GetLODsFromLODGroup();
            if (lods == null || m_LODBeams == null)
            {
                return;
            }
 
            for (int i = 0; i < m_LODBeams.Length; i++)
            {
                if (m_LODBeams[i] == beam)
                {
                    SetLOD(i);
                    return;
                }
            }
        }

        void SetupLodGroupData()
        {
            if (m_LODGroup == null)
            {
                return;
            }

            LOD[] lods = GetLODsFromLODGroup();
            if (lods == null)
            {
                return;
            }

            if(m_LODBeams == null || m_LODBeams.Length < lods.Length)
            {
                Utils.ResizeArray(ref m_LODBeams, lods.Length);
            }

            for (int i = 0; i < m_LODBeams.Length; i++)
            {
                if (m_LODBeams[i] == null)
                {
                    if (i < lods.Length)
                    {
                        SetLODRenderer(i, null);
                    }
                }
                else
                {
                    m_LODBeams[i].RegisterBeamGeometryGeneratedCallback(OnBeamGeometryGenerated);
                }
            }
        }

        void UnifyBeamsProperties()
        {
            if (m_LODBeams == null)
            {
                return;
            }

            if (m_ResetAllLODsLocalTransform)
            {
                // Process for all the beams, even LOD0
                foreach (var beam in m_LODBeams)
                {
                    if (beam)
                    {
                        beam.transform.localPosition = Vector3.zero;
                        beam.transform.localRotation = Quaternion.identity;
                        beam.transform.localScale = Vector3.one;
                    }
                }
            }

            if(m_LOD0PropsToCopy == 0 || m_LODBeams.Length <= 1)
            {
                return;
            }

            var LOD0 = m_LODBeams[0];
            if(LOD0 == null)
            {
                return;
            }

            // Process for all the "slave" beams only
            for(int i = 1; i < m_LODBeams.Length; ++i)
            {
                var LODi = m_LODBeams[i];
                if(LODi)
                {
                    LODi.CopyPropsFrom(LOD0, m_LOD0PropsToCopy);

                    // Disable 'property from light' feature on slave beams AFTER copying the properties from LOD0,
                    // because we copy some multipliers during this process, and multipliers properties handle the 'from light' feature in some cases
                    UtilsBeamProps.SetColorFromLight(LODi, false);
                    UtilsBeamProps.SetFallOffEndFromLight(LODi, false);
                    UtilsBeamProps.SetIntensityFromLight(LODi, false);
                    UtilsBeamProps.SetSpotAngleFromLight(LODi, false);
                }
            }
        }

#if UNITY_EDITOR
        public bool IsPropertlyLoaded()
        {
            return m_LODGroup != null;
        }

        public bool GetLODFromLODGroup(int lodIdx, ref LOD lodData)
        {
            Debug.Assert(m_LODGroup != null);

            LOD[] lods = m_LODGroup.GetLODs();
            Debug.Assert(lods != null);
            if (lods.IsValidIndex(lodIdx))
            {
                lodData = lods[lodIdx];
                return true;
            }
            return false;
        }

        public void SetLODBeamComponent(int lodIdx, VolumetricLightBeamAbstractBase beam)
        {
            if (m_LODBeams.IsValidIndex(lodIdx))
            {
                m_LODBeams[lodIdx] = beam;
            }
        }
#endif

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (m_SelectionToRestore)
                {
                    Selection.activeGameObject = m_SelectionToRestore;
                    m_SelectionToRestore = null;
                }

                SetupLodGroupData();
                UnifyBeamsProperties();
                return;
            }
#endif

            if(m_CopyLOD0PropsEachFrame)
            {
                UnifyBeamsProperties();
            }
        }
    }
}
