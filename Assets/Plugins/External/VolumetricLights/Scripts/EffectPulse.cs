using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLB
{
    [HelpURL(Consts.Help.UrlEffectPulse)]
    [AddComponentMenu(Consts.Help.AddComponentMenuEffectPulse)]
    public class EffectPulse : EffectAbstractBase
    {
        public new const string ClassName = "EffectPulse";

        /// <summary>
        /// Frequency of pulsing.
        /// Higher value means the pulsing will occur faster.
        /// </summary>
        [Range(0.1f, 60.0f)]
        public float frequency = Consts.Effects.FrequencyDefault;

        /// <summary>
        /// The amplitude of intensity change which will be applied to the Light and/or Beam.
        /// A random value will be picked each time inside that range.
        /// </summary>
        [MinMaxRange(-5.0f, 5.0f)]
        public MinMaxRangeFloat intensityAmplitude = Consts.Effects.IntensityAmplitudeDefault;

        public override void InitFrom(EffectAbstractBase source)
        {
            base.InitFrom(source);

            var sourcePulse = source as EffectPulse;
            if (sourcePulse)
            {
                frequency = sourcePulse.frequency;
                intensityAmplitude = sourcePulse.intensityAmplitude;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(CoUpdate());
        }

        IEnumerator CoUpdate()
        {
            var t = 0.0f;
            while (true)
            {
                var sin = Mathf.Sin(frequency * t);
                var value = intensityAmplitude.GetLerpedValue(sin * 0.5f + 0.5f);
                SetAdditiveIntensity(value);

                yield return null;
                t += Time.deltaTime;
            }
        }
    }
}

