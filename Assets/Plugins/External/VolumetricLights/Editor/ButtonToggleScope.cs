#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    class ButtonToggleScope : System.IDisposable
    {
        SerializedProperty m_Property;
        bool m_DisableGroup = false;
        GUIContent m_Content = null;

        void Enable()
        {
            EditorGUILayout.BeginHorizontal();
            if (m_DisableGroup)
                EditorGUI.BeginDisabledGroup(isPropertyToggled || m_Property.hasMultipleDifferentValues);
        }

        void Disable()
        {
            EndDisabledGroup();
            DrawToggleButton();
            EditorGUILayout.EndHorizontal();
            m_Property = null;
        }

        public void EndDisabledGroup()
        {
            if (m_DisableGroup)
                EditorGUI.EndDisabledGroup();
            m_DisableGroup = false; // prevent from calling EndDisabledGroup twice
        }

        public ButtonToggleScope(SerializedProperty prop, bool disableGroup, GUIContent content)
        {
            m_Property = prop;
            m_DisableGroup = disableGroup;
            m_Content = content;
            Enable();
        }

        public void Dispose() { Disable(); }

        static GUIStyle ms_ToggleButtonStyleNormal = null;
        static GUIStyle ms_ToggleButtonStyleToggled = null;
        static GUIStyle ms_ToggleButtonStyleMixedValue = null;

        bool isPropertyToggled
        {
            get
            {
                switch(m_Property.propertyType)
                {
                    case SerializedPropertyType.Boolean: return m_Property.boolValue;
                    case SerializedPropertyType.Float: return m_Property.floatValue >= 0.0f;
                    default: Debug.LogFormat("Invalid PropertyType {0}", m_Property.propertyType); return false;
                }
            }
        }

        void ToggleValue()
        {
            switch (m_Property.propertyType)
            {
                case SerializedPropertyType.Boolean: m_Property.boolValue = !m_Property.boolValue; break;
                case SerializedPropertyType.Float:
                    {
                        if (m_Property.floatValue == 0f) m_Property.floatValue = float.MinValue;
                        else if (m_Property.floatValue == float.MinValue) m_Property.floatValue = 0f;
                        else m_Property.floatValue = -m_Property.floatValue;
                        break;
                    }
                default: Debug.LogFormat("Invalid PropertyType {0}", m_Property.propertyType); break;
            }
        }

        void DrawToggleButton()
        {
            if (ms_ToggleButtonStyleNormal == null)
            {
                ms_ToggleButtonStyleNormal = new GUIStyle(EditorStyles.miniButton);
                ms_ToggleButtonStyleToggled = new GUIStyle(ms_ToggleButtonStyleNormal);
                ms_ToggleButtonStyleToggled.normal.background = ms_ToggleButtonStyleToggled.active.background;
                ms_ToggleButtonStyleMixedValue = new GUIStyle(ms_ToggleButtonStyleToggled);
                ms_ToggleButtonStyleMixedValue.fontStyle = FontStyle.Italic;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = m_Property.hasMultipleDifferentValues;

            var style = EditorGUI.showMixedValue ? ms_ToggleButtonStyleMixedValue : (isPropertyToggled ? ms_ToggleButtonStyleToggled : ms_ToggleButtonStyleNormal);
            var calcSize = style.CalcSize(m_Content);

#if UNITY_2019_3_OR_NEWER
            var defaultColor = GUI.backgroundColor;
            if(isPropertyToggled)
                GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
#endif

            GUILayout.Button(
                m_Content,
                style,
                GUILayout.MaxWidth(calcSize.x));

#if UNITY_2019_3_OR_NEWER
            GUI.backgroundColor = defaultColor;
#endif

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                ToggleValue();
        }

        public static ButtonToggleScope FromLight(SerializedProperty prop, bool visible)
        {
            if (!visible) return null;

            return new ButtonToggleScope(prop,
                true,   // disableGroup
                EditorData.Instance.contentFromSpotLight);
        }

        public static ButtonToggleScope Advanced(SerializedProperty prop, bool visible)
        {
            if (!visible) return null;

            return new ButtonToggleScope(prop,
                false,  // disableGroup
                EditorStrings.Beam.SD.IntensityModeAdvanced);
        }
    }
}
#endif // UNITY_EDITOR

