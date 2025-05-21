#if UNITY_EDITOR
using UnityEditor;

namespace VLB
{
    public abstract class Editor_EffectAbstractBase<T> : Editor_CommonSD where T : EffectAbstractBase
    {
        SerializedProperty componentsToChange = null;
        SerializedProperty restoreIntensityOnDisable = null;

        protected TargetList<T> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<T>(targets);
        }

        protected abstract void DisplayChildProperties();

        public sealed override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_Targets.HasAtLeastOneTargetWith((T comp) =>
            {
                if (comp.componentsToChange.HasFlag(EffectAbstractBase.ComponentsToChange.UnityLight))
                {
                    var light = comp.GetComponent<UnityEngine.Light>();
#if UNITY_5_6_OR_NEWER
                    return (light && light.lightmapBakeType == UnityEngine.LightmapBakeType.Baked);
#else
                    return (light && light.lightmappingMode == UnityEngine.LightmappingMode.Baked);
#endif
                }
                return false;
            }))
            {
                EditorGUILayout.HelpBox(EditorStrings.Effects.HelpLightNotChangeable, MessageType.Warning);
            }

            if (m_Targets.HasAtLeastOneTargetWith((T comp) =>
            {
                if (comp.componentsToChange.HasFlag(EffectAbstractBase.ComponentsToChange.VolumetricLightBeam))
                {
                    var beam = comp.GetComponent<VolumetricLightBeamSD>();
                    return (beam && !beam.trackChangesDuringPlaytime);
                }
                return false;
            }))
            {
                EditorGUILayout.HelpBox(EditorStrings.Effects.HelpBeamNotChangeable, MessageType.Warning);
            }

            DisplayChildProperties();

            if (FoldableHeader.Begin(this, EditorStrings.Effects.HeaderMisc))
            {
                componentsToChange.CustomMask<EffectAbstractBase.ComponentsToChange>(EditorStrings.Effects.ComponentsToChange);
                EditorGUILayout.PropertyField(restoreIntensityOnDisable, EditorStrings.Effects.RestoreIntensityOnDisable);
            }
            FoldableHeader.End();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
