#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
#define UI_USE_FOLDOUT_HEADER_2019
#endif

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace VLB
{
    public abstract class Editor_Common : Editor
    {
        protected virtual void OnEnable()
        {
            FoldableHeader.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
    #if UNITY_2019_3_OR_NEWER
            // no vertical space in 2019.3 looks better
    #else
            EditorGUILayout.Separator();
    #endif
        }

        protected static void ButtonOpenConfig(bool miniButton = true)
        {
            bool buttonClicked = false;
            if (miniButton) buttonClicked = GUILayout.Button(EditorStrings.Common.ButtonOpenGlobalConfig, EditorStyles.miniButton);
            else            buttonClicked = GUILayout.Button(EditorStrings.Common.ButtonOpenGlobalConfig);

            if (buttonClicked)
                Config.EditorSelectInstance();
        }


        // SERIALIZED PROPERTY RETRIEVAL
        string GetThisNamespaceAsString() { return GetType().Namespace; }

        SerializedProperty FindProperty(string prefix, string name)
        {
            Debug.Assert(serializedObject != null);
            var prop = serializedObject.FindProperty(name); // try first with the plain name
            if (prop == null)
            {
                name = string.Format("{0}{1}{2}", prefix, char.ToUpperInvariant(name[0]), name.Substring(1)); // try with a different name to catch private props: get '_Name' from 'name'
                prop = serializedObject.FindProperty(name);
            }
            return prop;
        }

        void RetrieveSerializedProperties(string prefix, Type t)
        {
            if (t == null) return;
            if (t.Namespace != GetThisNamespaceAsString()) return;

            var allEditorFields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var field in allEditorFields)
            {
                if (field.FieldType == typeof(SerializedProperty))
                {
                    var runtimeFieldName = field.Name;
                    var serializedProp = FindProperty(prefix, runtimeFieldName);
                    Debug.AssertFormat(serializedProp != null, "Fail to find serialized field '{0}' in object {1}", runtimeFieldName, serializedObject.targetObject);
                    field.SetValue(this, serializedProp);
                }
            }

            RetrieveSerializedProperties(prefix, t.BaseType);
        }

        protected void RetrieveSerializedProperties(string prefix)
        {
            RetrieveSerializedProperties(prefix, GetType());
        }

        protected static void DrawMultiplierProperty(SerializedProperty property, string tooltip)
        {
            using (new EditorExtensions.LabelWidth(13f))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property, new GUIContent("x", tooltip), GUILayout.MinWidth(35.0f), GUILayout.MaxWidth(55.0f));
                if(EditorGUI.EndChangeCheck())
                {
                    property.floatValue = Mathf.Max(property.floatValue, Consts.Beam.MultiplierMin);
                }
            }
        }

        protected struct InfoTip
        {
            public MessageType type;
            public string message;
        }

        protected virtual void GetInfoTips(List<InfoTip> tips) {}

        protected bool DrawInfos()
        {
            var tips = new List<InfoTip>();
            GetInfoTips(tips);

            if (tips != null && tips.Count > 0)
            {
                if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderInfos))
                {
                    if (tips != null)
                    {
                        foreach (var tip in tips)
                            EditorGUILayout.HelpBox(tip.message, tip.type);
                    }
                }
                FoldableHeader.End();
                return true;
            }
            return false;
        }
    }
}
#endif // UNITY_EDITOR

