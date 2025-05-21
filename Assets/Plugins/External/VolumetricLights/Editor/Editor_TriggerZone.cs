#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VLB
{
    [CustomEditor(typeof(TriggerZone))]
    [CanEditMultipleObjects]
    public class Editor_TriggerZone : Editor_CommonSD
    {
        SerializedProperty setIsTrigger = null;
        SerializedProperty rangeMultiplier = null;
        TargetList<VolumetricLightBeamAbstractBase> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<VolumetricLightBeamAbstractBase>(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(setIsTrigger, EditorStrings.TriggerZone.SetIsTrigger);
            EditorGUILayout.PropertyField(rangeMultiplier, EditorStrings.TriggerZone.RangeMultiplier);

            if (FoldableHeader.Begin(this, EditorStrings.TriggerZone.HeaderInfos))
            {
                EditorGUILayout.HelpBox(
                    UtilsBeamProps.GetDimensions(m_Targets.m_Targets[0]) == Dimensions.Dim3D ? EditorStrings.TriggerZone.HelpDescription3D : EditorStrings.TriggerZone.HelpDescription2D
                    , MessageType.Info);

                if(m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamAbstractBase beam) => { return UtilsBeamProps.CanChangeDuringPlaytime(beam); }))
                {
                    EditorGUILayout.HelpBox(EditorStrings.TriggerZone.HelpTrackChangesDuringPlaytimeEnabled, MessageType.Warning);
                }
            }
            FoldableHeader.End();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
