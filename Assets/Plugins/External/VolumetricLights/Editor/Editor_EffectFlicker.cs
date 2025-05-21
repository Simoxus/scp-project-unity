#if UNITY_EDITOR
using UnityEditor;

namespace VLB
{
    [CustomEditor(typeof(EffectFlicker))]
    [CanEditMultipleObjects]
    public class Editor_EffectFlicker : Editor_EffectAbstractBase<EffectFlicker>
    {
        SerializedProperty performPauses = null;
        SerializedProperty flickeringDuration = null;
        SerializedProperty pauseDuration = null;
        SerializedProperty restoreIntensityOnPause = null;
        SerializedProperty frequency = null;
        SerializedProperty intensityAmplitude = null;
        SerializedProperty smoothing = null;

        protected override void DisplayChildProperties()
        {
            if (FoldableHeader.Begin(this, EditorStrings.Effects.HeaderTimings))
            {
                EditorGUILayout.PropertyField(frequency, EditorStrings.Effects.FrequencyFlicker);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Effects.HeaderPause))
            {
                EditorGUILayout.PropertyField(performPauses, EditorStrings.Effects.PerformPauses);

                if (m_Targets.HasAtLeastOneTargetWith((EffectFlicker comp) => { return comp.performPauses == true; }))
                {
                    EditorGUILayout.PropertyField(flickeringDuration, EditorStrings.Effects.FlickeringDuration);
                    EditorGUILayout.PropertyField(pauseDuration, EditorStrings.Effects.PauseDuration);
                    EditorGUILayout.PropertyField(restoreIntensityOnPause, EditorStrings.Effects.RestoreIntensityOnPause);
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Effects.HeaderVisual))
            {
                EditorGUILayout.PropertyField(intensityAmplitude, EditorStrings.Effects.IntensityAmplitude);
                EditorGUILayout.PropertyField(smoothing, EditorStrings.Effects.Smoothing);
            }
            FoldableHeader.End();
        }
    }
}
#endif
