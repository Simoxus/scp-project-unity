#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace VLB
{
    public static class EditorExtensions
    {
        public static class SD
        {
            public static GameObject NewBeam() { return new GameObject("Volumetric Light Beam SD", typeof(VolumetricLightBeamSD)); }

            public static GameObject NewBeam2D()
            {
                var gao = new GameObject("Volumetric Light Beam SD (2D)", typeof(VolumetricLightBeamSD));
                gao.GetComponent<VolumetricLightBeamSD>().dimensions = Dimensions.Dim2D;
                return gao;
            }

            public static GameObject NewBeamAndDust() { return new GameObject("Volumetric Light Beam SD + Dust", typeof(VolumetricLightBeamSD), typeof(VolumetricDustParticles)); }

            public static GameObject NewSpotLightAndBeam()
            {
                var light = Utils.NewWithComponent<Light>("Spotlight and Beam SD");
                light.type = LightType.Spot;
                var gao = light.gameObject;
                gao.AddComponent<VolumetricLightBeamSD>();
                return gao;
            }
        }

        public static class HD
        {
            public static GameObject NewBeam() { return new GameObject("Volumetric Light Beam HD", typeof(VolumetricLightBeamHD)); }
            public static GameObject NewBeam2D() { return new GameObject("Volumetric Light Beam HD (2D)", typeof(VolumetricLightBeamHD2D)); }
            public static GameObject NewBeamAndDust() { return new GameObject("Volumetric Light Beam HD + Dust", typeof(VolumetricLightBeamHD), typeof(VolumetricDustParticles)); }

            public static GameObject NewSpotLightAndBeam()
            {
                var light = Utils.NewWithComponent<Light>("Spotlight and Beam HD");
                light.type = LightType.Spot;
                var gao = light.gameObject;
                gao.AddComponent<VolumetricLightBeamHD>().scalable = false;
                return gao;
            }
        }

        public static class LOD
        {
            public static GameObject NewLODBeamGroup() { return new GameObject("LOD Beam Group", typeof(LODBeamGroup)); }
        }

        public static void OnNewGameObjectCreated(GameObject gao, MenuCommand menuCommand)
        {
            Debug.Assert(gao != null);
            Debug.Assert(menuCommand != null);

            OnNewGameObjectCreated(gao, menuCommand.context as GameObject);
        }

        public static void OnNewGameObjectCreated(GameObject gao, GameObject parentGao)
        {
            Debug.Assert(gao != null);

            if (parentGao == null)
            {
#if UNITY_2021_2_OR_NEWER
                var currentPrefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
                var currentPrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
                if (currentPrefabStage != null)
                {
                    // The user is in prefab mode without any GAO selected in the hierarchy. Get the current prefab root as parent.
                    parentGao = currentPrefabStage.prefabContentsRoot;
                }
            }

            GameObjectUtility.SetParentAndAlign(gao, parentGao); // Ensure it gets reparented if this was a context click (otherwise does nothing)

            Undo.RegisterCreatedObjectUndo(gao, "Create " + gao.name); // Register the creation in the undo system
            Selection.activeObject = gao;
        }

        public static bool CanAddComponentFromEditor<TComp>(VolumetricLightBeamAbstractBase self) where TComp : Component
        {
            return !Application.isPlaying && self != null && self.GetComponent<TComp>() == null;
        }

        public static void AddComponentFromEditor<TComp>(VolumetricLightBeamAbstractBase self) where TComp : Component
        {
            if (CanAddComponentFromEditor<TComp>(self))
            {
                /*var comp =*/ Undo.AddComponent<TComp>(self.gameObject);
            }
        }

        /// <summary>
        /// Add a EditorGUILayout.ToggleLeft which properly handles multi-object editing
        /// </summary>
        public static void ToggleLeft(this SerializedProperty prop, GUIContent label, params GUILayoutOption[] options)
        {
            ToggleLeft(prop, label, prop.boolValue, options);
        }

        public static void ToggleLeft(this SerializedProperty prop, GUIContent label, bool forcedValue, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.ToggleLeft(label, forcedValue, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.boolValue = newValue;
        }


        public static bool HasAtLeastOneValue(this SerializedProperty prop, bool value)
        {
            return (prop.boolValue == value) || prop.hasMultipleDifferentValues;
        }

        public static bool HasAtLeastOnePositiveValue(this SerializedProperty prop)
        {
            return (prop.floatValue >= 0.0f) || prop.hasMultipleDifferentValues;
        }

        /// <summary>
        /// Create a EditorGUILayout.Slider which properly handles multi-object editing
        /// We apply the 'convIn' conversion to the SerializedProperty value before exposing it as a Slider.
        /// We apply the 'convOut' conversion to the Slider value to get the SerializedProperty back.
        /// </summary>
        /// <param name="prop">The value the slider shows.</param>
        /// <param name="label">Label in front of the slider.</param>
        /// <param name="leftValue">The value at the left end of the slider.</param>
        /// <param name="rightValue">The value at the right end of the slider.</param>
        /// <param name="convIn">Conversion applied on the SerializedProperty to get the Slider value</param>
        /// <param name="convOut">Conversion applied on the Slider value to get the SerializedProperty</param>
        public static bool FloatSlider(
            this SerializedProperty prop,
            GUIContent label,
            float leftValue, float rightValue,
            System.Func<float, float> convIn,
            System.Func<float, float> convOut,
            params GUILayoutOption[] options)
        {
            var floatValue = convIn(prop.floatValue);
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
                    floatValue = EditorGUILayout.Slider(label, floatValue, leftValue, rightValue, options);
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = convOut(floatValue);
                return true;
            }
            return false;
        }

        public static bool FloatSlider(
            this SerializedProperty prop,
            GUIContent label,
            float leftValue, float rightValue,
            params GUILayoutOption[] options)
        {
            var floatValue = prop.floatValue;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
                    floatValue = EditorGUILayout.Slider(label, floatValue, leftValue, rightValue, options);
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = floatValue;
                return true;
            }
            return false;
        }
/*
        public static void ToggleFromLight(this SerializedProperty prop)
        {
            ToggleLeft(
                prop,
                new GUIContent("From Spot", "Get the value from the Light Spot"),
                GUILayout.MaxWidth(80.0f));
        }
*/
        public static void ToggleUseGlobalNoise(this SerializedProperty prop)
        {
            ToggleLeft(
                prop,
                new GUIContent("Global", "Get the value from the Global 3D Noise"),
                GUILayout.MaxWidth(55.0f));
        }

        public static void CustomEnum<EnumType>(this SerializedProperty prop, GUIContent content, string[] descriptions = null)
        {
            if(descriptions == null)
                descriptions = System.Enum.GetNames(typeof(EnumType));

            Debug.Assert(System.Enum.GetNames(typeof(EnumType)).Length == descriptions.Length, string.Format("Enum '{0}' and the description array don't have the same size", typeof(EnumType)));

            int enumValueIndex = prop.enumValueIndex;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
#if UNITY_2018_1_OR_NEWER
                    enumValueIndex = EditorGUILayout.Popup(content, enumValueIndex, descriptions);
#else
                    enumValueIndex = EditorGUILayout.Popup(content.text, enumValueIndex, descriptions);
#endif
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
                prop.enumValueIndex = enumValueIndex;
        }

        public static void CustomMask<EnumType>(this SerializedProperty prop, GUIContent content, string[] descriptions = null)
        {
            if (descriptions == null)
                descriptions = System.Enum.GetNames(typeof(EnumType));

            Debug.Assert(System.Enum.GetNames(typeof(EnumType)).Length == descriptions.Length, string.Format("Enum '{0}' and the description array don't have the same size", typeof(EnumType)));

            int intValue = prop.intValue;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
                    intValue = EditorGUILayout.MaskField(content, intValue, descriptions);
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
                prop.intValue = intValue;
        }


        public static void DrawLineSeparator()
        {
            DrawLineSeparator(Color.grey, 1, 10);
        }

        static void DrawLineSeparator(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));

            r.x = 0;
            r.width = EditorGUIUtility.currentViewWidth;

            r.y += padding / 2;
            r.height = thickness;

            EditorGUI.DrawRect(r, color);
        }


        public static bool GlobalToggleButton(ref bool boolean, GUIContent content, string saveString, float maxWidth = 999.0f)
        {
            EditorGUI.BeginChangeCheck();
            boolean = GUILayout.Toggle(boolean, content, EditorStyles.miniButton, GUILayout.MaxWidth(maxWidth));
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(saveString, boolean);
                SceneView.RepaintAll();
                return true;
            }
            return false;
        }


        public abstract class EditorGUIWidth : System.IDisposable
        {
            protected abstract void ApplyWidth(float width);
            public EditorGUIWidth(float width) { ApplyWidth(width); }
            public void Dispose() { ApplyWidth(0.0f); }
        }

        public class LabelWidth : EditorGUIWidth
        {
            public LabelWidth(float width) : base(width) { }
            protected override void ApplyWidth(float width) { EditorGUIUtility.labelWidth = width; }
        }

        public class FieldWidth : EditorGUIWidth
        {
            public FieldWidth(float width) : base(width) { }
            protected override void ApplyWidth(float width) { EditorGUIUtility.fieldWidth = width; }
        }

        public class ShowMixedValue : System.IDisposable
        {
            public ShowMixedValue(bool? value) { m_PrevValue = EditorGUI.showMixedValue; EditorGUI.showMixedValue = value ?? false; }
            public ShowMixedValue(SerializedProperty prop) : this(prop?.hasMultipleDifferentValues) { }
            public void Dispose() { EditorGUI.showMixedValue = m_PrevValue; }
            bool m_PrevValue = false;
        }

        private static ArcHandle ms_ArcHandle = null;

        // HANDLES
        public static ArcHandle DrawHandleRadius(float radius)
        {
            if (ms_ArcHandle == null)
                ms_ArcHandle = new ArcHandle();

            ms_ArcHandle.SetColorWithRadiusHandle(Color.white, 0f);
            ms_ArcHandle.angle = 360f;
            ms_ArcHandle.angleHandleSizeFunction = null;
            ms_ArcHandle.angleHandleColor = Color.clear;
            ms_ArcHandle.radius = radius;
            ms_ArcHandle.DrawHandle();
            return ms_ArcHandle;
        }

        public static ArcHandle DrawHandleSpotAngle(float angle, float radius)
        {
            if (ms_ArcHandle == null)
                ms_ArcHandle = new ArcHandle();

            ms_ArcHandle.angleHandleSizeFunction = ArcHandle.DefaultAngleHandleSizeFunction;
            ms_ArcHandle.SetColorWithRadiusHandle(Handles.color, Handles.color.maxColorComponent < 0.5f ? 0.25f : 0.1f);
            ms_ArcHandle.angleHandleColor = Handles.color;
            ms_ArcHandle.radius = radius;
            ms_ArcHandle.angle = angle;
            ms_ArcHandle.DrawHandle();
            return ms_ArcHandle;
        }
    }
}
#endif