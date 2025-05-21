#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VLB
{
    [CustomEditor(typeof(VolumetricCookieHD))]
    [CanEditMultipleObjects]
    public class Editor_VolumetricCookieHD : Editor_CommonHD
    {
        SerializedProperty m_Contribution = null, m_CookieTexture = null, m_Channel = null, m_Negative = null;
        SerializedProperty m_Translation = null, m_Rotation = null, m_Scale = null;
        protected TargetList<VolumetricCookieHD> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<VolumetricCookieHD>(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (FoldableHeader.Begin(this, EditorStrings.Cookie.HeaderVisual))
            {
                EditorGUILayout.Slider(m_Contribution, Consts.Cookie.ContributionMin, Consts.Cookie.ContributionMax, EditorStrings.Cookie.Contribution);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Cookie.HeaderTexture))
            {
                EditorGUILayout.PropertyField(m_CookieTexture, EditorStrings.Cookie.CookieTexture);
                EditorGUILayout.PropertyField(m_Channel, EditorStrings.Cookie.Channel);

                if (m_Targets.HasAtLeastOneTargetWith((VolumetricCookieHD comp) => { return comp.channel != CookieChannel.RGBA; }))
                {
                    EditorGUILayout.PropertyField(m_Negative, EditorStrings.Cookie.Negative);
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Cookie.HeaderTransform))
            {
                EditorGUILayout.PropertyField(m_Translation, EditorStrings.Cookie.Translation);
                EditorGUILayout.PropertyField(m_Rotation, EditorStrings.Cookie.Rotation);
                EditorGUILayout.PropertyField(m_Scale, EditorStrings.Cookie.Scale);
            }
            FoldableHeader.End();

            DrawInfos();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void GetInfoTips(List<InfoTip> tips)
        {
            if (m_Targets.HasAtLeastOneTargetWith((VolumetricCookieHD comp) => {
                if (comp.cookieTexture is Texture2D tex2D)
                    return tex2D.mipmapCount > 1;
                return false;
            }))
                tips.Add(new InfoTip { type = MessageType.Warning, message = EditorStrings.Cookie.TipCookieMipMaps });

            if (m_Targets.HasAtLeastOneTargetWith((VolumetricCookieHD comp) => { return comp.GetComponent<VolumetricLightBeamHD>().jitteringFactor == 0.0f; }))
                tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Beam.HD.TipJittering });

            base.GetInfoTips(tips);
        }
    }
}
#endif
