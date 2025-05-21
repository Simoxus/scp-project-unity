using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB
{
    [AddComponentMenu("")] // hide it from Component search
    public class EffectAbstractBase : MonoBehaviour
    {
        public const string ClassName = "EffectAbstractBase";

        [System.Flags]
        public enum ComponentsToChange
        {
            UnityLight = 1 << 0,
            VolumetricLightBeam = 1 << 1,
            VolumetricDustParticles = 1 << 2,
        }

        /// <summary>
        /// Decide which component to change among:
        /// - Unity's Light
        /// - Volumetric Light Beam
        /// - Volumetric Dust Particles
        /// </summary>
        public ComponentsToChange componentsToChange = Consts.Effects.ComponentsToChangeDefault;

        /// <summary>
        /// Restore the default intensity when this component is disabled.
        /// </summary>
        [FormerlySerializedAs("restoreBaseIntensity")]
        public bool restoreIntensityOnDisable = Consts.Effects.RestoreIntensityOnDisableDefault;

        [System.Obsolete("Use 'restoreIntensityOnDisable' instead")]
        public bool restoreBaseIntensity { get { return restoreIntensityOnDisable; } set { restoreIntensityOnDisable = value; } }

        protected VolumetricLightBeamAbstractBase m_Beam = null;
        protected Light m_Light = null;
        protected VolumetricDustParticles m_Particles = null;
        protected float m_BaseIntensityBeamInside = 0.0f;
        protected float m_BaseIntensityBeamOutside = 0.0f;
        protected float m_BaseIntensityLight = 0.0f;

        public virtual void InitFrom(EffectAbstractBase Source)
        {
            if(Source)
            {
                componentsToChange = Source.componentsToChange;
                restoreIntensityOnDisable = Source.restoreIntensityOnDisable;
            }
        }

        void GetIntensity(VolumetricLightBeamSD beam)
        {
            if (beam)
            {
                m_BaseIntensityBeamInside = beam.intensityInside;
                m_BaseIntensityBeamOutside = beam.intensityOutside;
            }
        }

        void GetIntensity(VolumetricLightBeamHD beam)
        {
            if (beam)
            {
                m_BaseIntensityBeamOutside = beam.intensity;
            }
        }

        void SetIntensity(VolumetricLightBeamSD beam, float additive)
        {
            if (beam)
            {
                beam.intensityInside = Mathf.Max(0.0f, m_BaseIntensityBeamInside + additive);
                beam.intensityOutside = Mathf.Max(0.0f, m_BaseIntensityBeamOutside + additive);
            }
        }

        void SetIntensity(VolumetricLightBeamHD beam, float additive)
        {
            if (beam)
            {
                beam.intensity = Mathf.Max(0.0f, m_BaseIntensityBeamOutside + additive);
            }
        }

        protected void SetAdditiveIntensity(float additive)
        {
            if (componentsToChange.HasFlag(ComponentsToChange.VolumetricLightBeam) && m_Beam)
            {
                SetIntensity(m_Beam as VolumetricLightBeamSD, additive);
                SetIntensity(m_Beam as VolumetricLightBeamHD, additive);
            }

            if (componentsToChange.HasFlag(ComponentsToChange.UnityLight) && m_Light)
                m_Light.intensity = Mathf.Max(0.0f, m_BaseIntensityLight + additive);

            if (componentsToChange.HasFlag(ComponentsToChange.VolumetricDustParticles) && m_Particles)
                m_Particles.alphaAdditionalRuntime = 1.0f + additive;
        }

        void Awake()
        {
            m_Beam = GetComponent<VolumetricLightBeamAbstractBase>();
            m_Light = GetComponent<Light>();
            m_Particles = GetComponent<VolumetricDustParticles>();
            GetIntensity(m_Beam as VolumetricLightBeamSD);
            GetIntensity(m_Beam as VolumetricLightBeamHD);
            m_BaseIntensityLight = m_Light ? m_Light.intensity : 0.0f;
        }

        protected virtual void OnEnable()
        {
            StopAllCoroutines();
        }

        void OnDisable()
        {
            StopAllCoroutines();

            if (restoreIntensityOnDisable)
                SetAdditiveIntensity(0.0f);
        }
    }
}
