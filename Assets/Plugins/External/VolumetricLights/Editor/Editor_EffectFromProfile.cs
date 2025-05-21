#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VLB
{
    [CustomEditor(typeof(EffectFromProfile))]
    [CanEditMultipleObjects]
    public class Editor_EffectFromProfile : Editor_CommonHD
    {
        SerializedProperty effectProfile = null;
        TargetList<EffectFromProfile> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<EffectFromProfile>(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            bool needToApplyProfile = false;

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(effectProfile, EditorStrings.Effects.EffectProfile);
            }
            if (EditorGUI.EndChangeCheck())
            {
                needToApplyProfile = Application.isPlaying;
            }

            DrawInfos();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(EditorStrings.Effects.ButtonNewProfileFlicker, EditorStyles.miniButton))
                {
                    EditorMenuItems.CreateEffectProfileFlicker();
                }

                if (GUILayout.Button(EditorStrings.Effects.ButtonNewProfilePulse, EditorStyles.miniButton))
                {
                    EditorMenuItems.CreateEffectProfilePulse();
                }
            }

            serializedObject.ApplyModifiedProperties();

            if(needToApplyProfile)
            {
                m_Targets.DoAction((EffectFromProfile effectFromProfile) => { effectFromProfile.InitInstanceFromProfile(); });
            }
        }

        protected override void GetInfoTips(List<InfoTip> tips)
        {
            tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Effects.HelpEffectProfile });
            base.GetInfoTips(tips);
        }
    }
}

#endif
