#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VLB
{
    [CustomEditor(typeof(VolumetricShadowHD))]
    [CanEditMultipleObjects]
    public class Editor_VolumetricShadowHD : Editor_CommonHD
    {
        SerializedProperty m_Strength = null;
        SerializedProperty m_DepthMapResolution = null, m_LayerMask = null, m_UseOcclusionCulling = null;
        SerializedProperty m_UpdateRate = null, m_WaitXFrames = null;
        TargetList<VolumetricShadowHD> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<VolumetricShadowHD>(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            bool shouldUpdateDepthCameraProperties = false;

            if (FoldableHeader.Begin(this, EditorStrings.Shadow.HeaderVisual))
            {
                EditorGUILayout.Slider(m_Strength, Consts.Shadow.StrengthMin, Consts.Shadow.StrengthMax, EditorStrings.Shadow.Strength);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Shadow.HeaderCamera))
            {
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(m_LayerMask, EditorStrings.Shadow.LayerMask);

                    if (Config.Instance.geometryOverrideLayer == false)
                    {
                        EditorGUILayout.HelpBox(EditorStrings.Shadow.HelpOverrideLayer, MessageType.Warning);
                    }
                    else if (m_Targets.HasAtLeastOneTargetWith((VolumetricShadowHD comp) => { return comp.HasLayerMaskIssues(); }))
                    {
                        EditorGUILayout.HelpBox(EditorStrings.Shadow.HelpLayerMaskIssues, MessageType.Warning);
                    }

                    EditorGUILayout.PropertyField(m_UseOcclusionCulling, EditorStrings.Shadow.OcclusionCulling);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    shouldUpdateDepthCameraProperties = true;
                }

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(m_DepthMapResolution, EditorStrings.Shadow.DepthMapResolution);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (Application.isPlaying)
                    {
                        Debug.LogErrorFormat(Consts.Shadow.GetErrorChangeRuntimeDepthMapResolution(m_Targets[0]));
                    }
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Shadow.HeaderUpdateRate))
            {
                m_UpdateRate.CustomEnum<ShadowUpdateRate>(EditorStrings.Shadow.UpdateRate, EditorStrings.Shadow.UpdateRateDescriptions);

                if (m_Targets.HasAtLeastOneTargetWith((VolumetricShadowHD comp) => { return comp.updateRate.HasFlag(ShadowUpdateRate.EveryXFrames); }))
                {
                    EditorGUILayout.PropertyField(m_WaitXFrames, EditorStrings.Shadow.WaitXFrames);
                }

                EditorGUILayout.HelpBox(
                    string.Format(EditorStrings.Shadow.GetUpdateRateAdvice<VolumetricShadowHD>(m_Targets[0].updateRate), m_Targets[0].waitXFrames),
                    MessageType.Info);
            }

            FoldableHeader.End();

            DrawInfos();

            serializedObject.ApplyModifiedProperties();

            if (shouldUpdateDepthCameraProperties)
            {
                foreach (var target in m_Targets) target.UpdateDepthCameraProperties();
            }
        }

        protected override void GetInfoTips(List<InfoTip> tips)
        {
            if (m_Targets.HasAtLeastOneTargetWith((VolumetricShadowHD comp) => { return comp.GetComponent<VolumetricLightBeamHD>().jitteringFactor == 0.0f; }))
                tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Beam.HD.TipJittering });
            base.GetInfoTips(tips);
        }
    }
}
#endif
