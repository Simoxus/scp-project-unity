#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [CustomEditor(typeof(VolumetricLightBeamSD))]
    [CanEditMultipleObjects]
    public class Editor_VolumetricLightBeamSD : Editor_VolumetricLightBeamAbstractBase
    {
        SerializedProperty trackChangesDuringPlaytime = null;
        SerializedProperty colorFromLight = null, colorMode = null, color = null, colorGradient = null;
        SerializedProperty intensityFromLight = null, intensityModeAdvanced = null, intensityInside = null, intensityOutside = null, intensityMultiplier = null;
        SerializedProperty blendingMode = null, shaderAccuracy = null;
        SerializedProperty fresnelPow = null, glareFrontal = null, glareBehind = null;
        SerializedProperty spotAngleFromLight = null, spotAngle = null, spotAngleMultiplier = null;
        SerializedProperty coneRadiusStart = null, geomMeshType = null, geomCustomSides = null, geomCustomSegments = null, geomCap = null;
        SerializedProperty fallOffEndFromLight = null, fallOffStart = null, fallOffEnd = null, fallOffEndMultiplier = null;
        SerializedProperty attenuationEquation = null, attenuationCustomBlending = null;
        SerializedProperty depthBlendDistance = null, cameraClippingDistance = null;
        SerializedProperty noiseMode = null, noiseIntensity = null, noiseScaleUseGlobal = null, noiseScaleLocal = null, noiseVelocityUseGlobal = null, noiseVelocityLocal = null;
        SerializedProperty fadeOutBegin = null, fadeOutEnd = null;
        SerializedProperty dimensions = null, sortingLayerID = null, sortingOrder = null;
        SerializedProperty skewingLocalForwardDirection = null, clippingPlaneTransform = null, tiltFactor = null;
        SerializedProperty hdrpExposureWeight = null;

        TargetList<VolumetricLightBeamSD> m_Targets;
        string[] m_SortingLayerNames;

        protected override void OnEnable()
        {
            base.OnEnable();
            RetrieveSerializedProperties("_");

            m_SortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
            m_Targets = new TargetList<VolumetricLightBeamSD>(targets);
        }

        static void PropertyThickness(SerializedProperty sp)
        {
            sp.FloatSlider(
                EditorStrings.Beam.SD.SideThickness,
                0, 1,
                (value) => Mathf.Clamp01(1 - (value / Consts.Beam.SD.FresnelPowMaxValue)),    // conversion value to slider
                (value) => (1 - value) * Consts.Beam.SD.FresnelPowMaxValue                    // conversion slider to value
                );
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Debug.Assert(m_Targets.Count > 0);

#if VLB_DEBUG
            if (m_Targets.Count == 1)
            {
                string msg = "";
                var geom = m_Targets[0].GetComponentInChildren<BeamGeometrySD>();
                if (geom == null)
                    msg = "No BeamGeometry";
                else
                    msg = string.Format("Material: {0} | ID: {1} / {2}"
                    , geom._EDITOR_IsUsingCustomMaterial ? "CUSTOM" : "INSTANCED"
                    , geom._EDITOR_InstancedMaterialID
                    , MaterialManager.StaticPropertiesSD.staticPropertiesCount);
                EditorGUILayout.LabelField(msg);
            }
#endif // VLB_DEBUG

            VolumetricLightBeamSD.AttachedLightType lightType;
            bool hasLightSpot = m_Targets[0].GetLightSpotAttachedSlow(out lightType) != null;
            if (lightType == VolumetricLightBeamSD.AttachedLightType.OtherLight)
            {
                EditorGUILayout.HelpBox(EditorStrings.Beam.HelpNoSpotlight, MessageType.Warning);
            }

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderBasic))
            {
                // Color
                using (ButtonToggleScope.FromLight(colorFromLight, hasLightSpot))
                {
                    if (!hasLightSpot) EditorGUILayout.BeginHorizontal();    // mandatory to have the color picker on the same line (when the button "from light" is not here)
                    {
                        if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off)
                        {
                            EditorGUILayout.PropertyField(color, EditorStrings.Beam.ColorMode);
                        }
                        else
                        {
                            using (new EditorExtensions.LabelWidth(65f))
                            {
                                EditorGUILayout.PropertyField(colorMode, EditorStrings.Beam.ColorMode);
                            }

                            if (colorMode.enumValueIndex == (int)ColorMode.Gradient)
                                EditorGUILayout.PropertyField(colorGradient, EditorStrings.Beam.ColorGradient);
                            else
                                EditorGUILayout.PropertyField(color, EditorStrings.Beam.ColorFlat);
                        }
                    }
                    if (!hasLightSpot) EditorGUILayout.EndHorizontal();
                }
                
                // Blending Mode
                EditorGUILayout.PropertyField(blendingMode, EditorStrings.Beam.BlendingMode);

                EditorGUILayout.Separator();

                // Intensity
                bool advancedModeEnabled = false;
                using (var lightDisabledGrp = ButtonToggleScope.FromLight(intensityFromLight, hasLightSpot))
                {
                    bool advancedModeButton = !hasLightSpot || intensityFromLight.HasAtLeastOneValue(false);
                    using (ButtonToggleScope.Advanced(intensityModeAdvanced, advancedModeButton))
                    {
                        advancedModeEnabled = intensityModeAdvanced.HasAtLeastOneValue(true);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.useIntensityFromAttachedLightSpot; }))
                            {
                                using (new EditorExtensions.ShowMixedValue(intensityOutside))
                                {
                                    // display grayed out Unity's light intensity
                                    EditorGUILayout.FloatField(EditorStrings.Beam.SD.IntensityGlobal, SpotLightHelper.GetIntensity(m_Targets[0].lightSpotAttached));
                                }

                                lightDisabledGrp?.EndDisabledGroup(); // muliplier factor should be available
                                DrawMultiplierProperty(intensityMultiplier, EditorStrings.Beam.IntensityMultiplier); // multiplier property
                            }
                            else
                            {
                                EditorGUILayout.PropertyField(intensityOutside, advancedModeEnabled ? EditorStrings.Beam.SD.IntensityOutside : EditorStrings.Beam.SD.IntensityGlobal);
                            }
                        }
                    }
                }

                if (advancedModeEnabled)
                    EditorGUILayout.PropertyField(intensityInside, EditorStrings.Beam.SD.IntensityInside);
                else
                    intensityInside.floatValue = intensityOutside.floatValue;

                if(Config.Instance.isHDRPExposureWeightSupported)
                {
                    EditorGUILayout.PropertyField(hdrpExposureWeight, EditorStrings.Beam.HDRPExposureWeight);
                }

                PropertyThickness(fresnelPow);

                EditorGUILayout.Separator();

                EditorGUILayout.PropertyField(glareFrontal, EditorStrings.Beam.GlareFrontal);
                EditorGUILayout.PropertyField(glareBehind, EditorStrings.Beam.GlareBehind);

                EditorGUILayout.Separator();

                if (Config.Instance.featureEnabledShaderAccuracyHigh)
                {
                    EditorGUILayout.PropertyField(shaderAccuracy, EditorStrings.Beam.ShaderAccuracy);
                    EditorGUILayout.Separator();
                }

                trackChangesDuringPlaytime.ToggleLeft(EditorStrings.Beam.TrackChanges);
                DrawAnimatorWarning();
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderShape))
            {
                // Fade End
                using (var lightDisabledGrp = ButtonToggleScope.FromLight(fallOffEndFromLight, hasLightSpot))
                {
                    if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.useFallOffEndFromAttachedLightSpot; }))
                    {
                        using (new EditorExtensions.ShowMixedValue(fallOffEnd))
                        {
                            // display grayed out Unity's light range
                            EditorGUILayout.FloatField(EditorStrings.Beam.FallOffEnd, SpotLightHelper.GetFallOffEnd(m_Targets[0].lightSpotAttached));
                        }

                        lightDisabledGrp?.EndDisabledGroup(); // muliplier factor should be available
                        DrawMultiplierProperty(fallOffEndMultiplier, EditorStrings.Beam.FallOffEndMultiplier); // multiplier property
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(fallOffEnd, EditorStrings.Beam.FallOffEnd);
                    }
                }

                // Spot Angle
                using (var lightDisabledGrp = ButtonToggleScope.FromLight(spotAngleFromLight, hasLightSpot))
                {
                    if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.useSpotAngleFromAttachedLightSpot; }))
                    {
                        using (new EditorExtensions.ShowMixedValue(spotAngle))
                        {
                            // display grayed out Unity's light angle
                            EditorGUILayout.FloatField(EditorStrings.Beam.SpotAngle, SpotLightHelper.GetSpotAngle(m_Targets[0].lightSpotAttached));
                        }

                        lightDisabledGrp?.EndDisabledGroup(); // muliplier factor should be available
                        DrawMultiplierProperty(spotAngleMultiplier, EditorStrings.Beam.SpotAngleMultiplier); // multiplier property
                    }
                    else
                    {
                        EditorGUILayout.Slider(spotAngle, Consts.Beam.SpotAngleMin, Consts.Beam.SpotAngleMax, EditorStrings.Beam.SpotAngle);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(coneRadiusStart, EditorStrings.Beam.ConeRadiusStart);
                    EditorGUI.BeginChangeCheck();
                    {
                        geomCap.ToggleLeft(EditorStrings.Beam.GeomCap, GUILayout.MaxWidth(42.0f));
                    }
                    if (EditorGUI.EndChangeCheck()) { SetMeshesDirty(); }
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderAttenuation))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(attenuationEquation, EditorStrings.Beam.SD.AttenuationEquation);
                    if (attenuationEquation.enumValueIndex == (int)AttenuationEquation.Blend)
                        EditorGUILayout.PropertyField(attenuationCustomBlending, EditorStrings.Beam.SD.AttenuationCustomBlending);
                }
                EditorGUILayout.EndHorizontal();

                if (fallOffEnd.hasMultipleDifferentValues)
                    EditorGUILayout.PropertyField(fallOffStart, EditorStrings.Beam.FallOffStart);
                else
                    fallOffStart.FloatSlider(EditorStrings.Beam.FallOffStart, 0f, fallOffEnd.floatValue - Consts.Beam.FallOffDistancesMinThreshold);

                EditorGUILayout.Separator();

                // Tilt
                if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.shaderAccuracy == ShaderAccuracy.High; }))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(tiltFactor, EditorStrings.Beam.SD.TiltFactor);
                        EditorExtensions.GlobalToggleButton(ref VolumetricLightBeamSD.editorShowTiltFactor, EditorStrings.Beam.SD.EditorShowTiltDirection, EditorPrefsStrings.Beam.PrefShowTiltDir, 50f);
                    }
                }

                if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.isTilted && beam.shaderAccuracy != ShaderAccuracy.High; }))
                    EditorGUILayout.HelpBox(EditorStrings.Beam.SD.HelpTiltedWithShaderAccuracyFast, MessageType.Warning);
            }
            FoldableHeader.End();

            if (Config.Instance.featureEnabledNoise3D)
            {
                if (FoldableHeader.Begin(this, EditorStrings.Beam.Header3DNoise))
                {
                    noiseMode.CustomEnum<NoiseMode>(EditorStrings.Beam.NoiseMode, EditorStrings.Beam.NoiseModeEnumDescriptions);

                    bool showNoiseProps = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.isNoiseEnabled; });
                    if (showNoiseProps)
                    {
                        EditorGUILayout.PropertyField(noiseIntensity, EditorStrings.Beam.NoiseIntensity);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(noiseScaleUseGlobal.boolValue))
                            {
                                EditorGUILayout.PropertyField(noiseScaleLocal, EditorStrings.Beam.NoiseScale);
                            }
                            noiseScaleUseGlobal.ToggleUseGlobalNoise();
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(noiseVelocityUseGlobal.boolValue))
                            {
                                EditorGUILayout.PropertyField(noiseVelocityLocal, EditorStrings.Beam.NoiseVelocity);
                            }
                            noiseVelocityUseGlobal.ToggleUseGlobalNoise();
                        }

                        if (Noise3D.isSupported && !Noise3D.isProperlyLoaded)
                            EditorGUILayout.HelpBox(EditorStrings.Common.HelpNoiseLoadingFailed, MessageType.Error);

                        if (!Noise3D.isSupported)
                            EditorGUILayout.HelpBox(Noise3D.isNotSupportedString, MessageType.Info);
                    }
                }
                FoldableHeader.End();
            }

            if(FoldableHeader.Begin(this, EditorStrings.Beam.HeaderBlendingDistances))
            {
                {
                    var content = AddEnabledStatusToContentText(EditorStrings.Beam.CameraClippingDistance, cameraClippingDistance);
                    EditorGUILayout.PropertyField(cameraClippingDistance, content);
                }

                {
                    var content = AddEnabledStatusToContentText(EditorStrings.Beam.DepthBlendDistance, depthBlendDistance);
                    EditorGUILayout.PropertyField(depthBlendDistance, content);
                }
            }
            FoldableHeader.End();

            if(FoldableHeader.Begin(this, EditorStrings.Beam.HeaderGeometry))
            {
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(geomMeshType, EditorStrings.Beam.GeomMeshType);
                }
                if (EditorGUI.EndChangeCheck()) { SetMeshesDirty(); }

                if (geomMeshType.intValue == (int)MeshType.Custom)
                {
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(geomCustomSides, EditorStrings.Beam.GeomSides);
                        EditorGUILayout.PropertyField(geomCustomSegments, EditorStrings.Beam.GeomSegments);
                    }
                    if (EditorGUI.EndChangeCheck()) { SetMeshesDirty(); }

                    if (Config.Instance.featureEnabledMeshSkewing)
                    {
                        var vec3 = skewingLocalForwardDirection.vector3Value;
                        var vec2 = Vector2.zero;
                        EditorGUI.BeginChangeCheck();
                        {
                            vec2 = EditorGUILayout.Vector2Field(EditorStrings.Beam.SD.SkewingLocalForwardDirection, vec3.xy());
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            vec3 = new Vector3(vec2.x, vec2.y, 1.0f);
                            skewingLocalForwardDirection.vector3Value = vec3;
                            SetMeshesDirty();
                        }
                    }

                    if (m_Targets.Count == 1)
                    {
                        EditorGUILayout.HelpBox(m_Targets[0].meshStats, MessageType.Info);
                    }

                    EditorGUI.indentLevel--;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(clippingPlaneTransform, EditorStrings.Beam.SD.ClippingPlane);

                    if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.clippingPlaneTransform != null; }))
                    {
                        EditorExtensions.GlobalToggleButton(ref VolumetricLightBeamSD.editorShowClippingPlane, EditorStrings.Beam.SD.EditorShowClippingPlane, EditorStrings.Beam.SD.PrefShowAddClippingPlane, 50f);
                    }
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderFadeOut))
            {
                bool wasEnabled = fadeOutBegin.floatValue <= fadeOutEnd.floatValue;

                if(m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return (beam.fadeOutBegin <= beam.fadeOutEnd) != wasEnabled; }))
                {
                    wasEnabled = true;
                    EditorGUI.showMixedValue = true;
                }

                System.Action<float> setFadeOutBegin = value =>
                {
                    fadeOutBegin.floatValue = value;
                    m_Targets.RecordUndoAction("Change Fade Out Begin Distance",
                        (VolumetricLightBeamSD beam) => { beam.fadeOutBegin = value; });
                };

                System.Action<float> setFadeOutEnd = value =>
                {
                    fadeOutEnd.floatValue = value;
                    m_Targets.RecordUndoAction("Change Fade Out End Distance",
                        (VolumetricLightBeamSD beam) => { beam.fadeOutEnd = value; });
                };

                EditorGUI.BeginChangeCheck();
                bool isEnabled = EditorGUILayout.Toggle(EditorStrings.Beam.FadeOutEnabled, wasEnabled);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    float invValue = isEnabled ? 1 : -1;
                    float valueA = Mathf.Abs(fadeOutBegin.floatValue);
                    float valueB = Mathf.Abs(fadeOutEnd.floatValue);
                    setFadeOutBegin(invValue * Mathf.Min(valueA, valueB));
                    setFadeOutEnd  (invValue * Mathf.Max(valueA, valueB));
                }

                if (isEnabled)
                {
                    const float kEpsilon = 0.1f;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(fadeOutBegin, EditorStrings.Beam.FadeOutBegin);
                        if (EditorGUI.EndChangeCheck())
                        {
                            setFadeOutBegin(Mathf.Clamp(fadeOutBegin.floatValue, 0, fadeOutEnd.floatValue - kEpsilon));
                        }

                        using (new EditorExtensions.LabelWidth(30f))
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(fadeOutEnd, EditorStrings.Beam.FadeOutEnd);
                            if (EditorGUI.EndChangeCheck())
                            {
                                setFadeOutEnd(Mathf.Max(fadeOutBegin.floatValue + kEpsilon, fadeOutEnd.floatValue));
                            }
                        }
                    }
                    if (Application.isPlaying)
                    {
                        if(Config.Instance.fadeOutCameraTransform == null)
                        {
                            EditorGUILayout.HelpBox(EditorStrings.Beam.HelpFadeOutNoMainCamera, MessageType.Error);
                        }
                    }
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.Header2D))
            {
                dimensions.CustomEnum<Dimensions>(EditorStrings.Beam.Dimensions, EditorStrings.Common.DimensionsEnumDescriptions);
                DrawSortingLayerAndOrder();
            }
            FoldableHeader.End();

            DrawInfos();
            DrawEditInSceneButton();
            DrawCustomActionButtons();
            DrawAdditionalFeatures();
            
            serializedObject.ApplyModifiedProperties();
        }

        GUIContent AddEnabledStatusToContentText(GUIContent inContent, SerializedProperty prop)
        {
            Debug.Assert(prop.propertyType == SerializedPropertyType.Float);

            var content = new GUIContent(inContent);
            if (prop.hasMultipleDifferentValues)
                content.text += " (-)";
            else
                content.text += prop.floatValue > 0.0 ? " (on)" : " (off)";
            return content;
        }

        void SetMeshesDirty()
        {
            foreach (var entity in m_Targets) entity._EditorSetMeshDirty();
        }

        void DrawSortingLayerAndOrder()
        {
            var updatedProperties = SortingLayerAndOrderDrawer.Draw(sortingLayerID, sortingOrder);

            if(updatedProperties.HasFlag(SortingLayerAndOrderDrawer.UpdatedProperties.SortingLayerID))
                m_Targets.RecordUndoAction("Edit Sorting Layer", (VolumetricLightBeamSD beam) => beam.sortingLayerID = sortingLayerID.intValue); // call setters

            if (updatedProperties.HasFlag(SortingLayerAndOrderDrawer.UpdatedProperties.SortingOrder))
                m_Targets.RecordUndoAction("Edit Sorting Order", (VolumetricLightBeamSD beam) => beam.sortingOrder = sortingOrder.intValue); // call setters
        }

        void DrawAnimatorWarning()
        {
            var showAnimatorWarning = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.GetComponent<Animator>() != null && beam.trackChangesDuringPlaytime == false; });

            if (showAnimatorWarning)
                EditorGUILayout.HelpBox(EditorStrings.Beam.SD.HelpAnimatorWarning, MessageType.Warning);
        }

        void DrawCustomActionButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(EditorStrings.Beam.ButtonResetProperties, EditorStyles.miniButton))
                {
                    m_Targets.RecordUndoAction("Reset Light Beam Properties",
                        (VolumetricLightBeamSD beam) => { beam.Reset(); beam.GenerateGeometry(); } );
                }

                if(m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.geomMeshType == MeshType.Custom; }))
                {
                    if (GUILayout.Button(EditorStrings.Beam.ButtonGenerateGeometry, EditorStyles.miniButton))
                    {
                        foreach (var entity in m_Targets) entity.GenerateGeometry();
                    }
                }
            }
        }

        void DrawAdditionalFeatures()
        {
            if (Application.isPlaying) return; // do not support adding additional components at runtime

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawButtonAddComponentDust();

                bool showButtonOcclusion = Config.Instance.featureEnabledDynamicOcclusion && m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamSD beam) => { return beam.GetComponent<DynamicOcclusionAbstractBase>() == null; });
                if (showButtonOcclusion && GUILayout.Button(EditorData.Instance.contentAddDynamicOcclusion, buttonAddComponentStyle))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddDynamicOcclusionRaycasting),  false, AddComponentToTargets<DynamicOcclusionRaycasting>);
                    menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddDynamicOcclusionDepthBuffer), false, AddComponentToTargets<DynamicOcclusionDepthBuffer>);
                    menu.ShowAsContext();
                }

                DrawButtonAddComponentEffect();
                DrawButtonAddComponentTriggerZone();
            }
        }

        protected override void GetInfoTips(List<InfoTip> tips)
        {
            if (m_Targets.Count == 1)
            {
                if (depthBlendDistance.floatValue > 0f || !Noise3D.isSupported || trackChangesDuringPlaytime.boolValue)
                {
                    if (depthBlendDistance.floatValue > 0f)
                    {
#if UNITY_IPHONE || UNITY_IOS || UNITY_ANDROID
                        tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Beam.SD.HelpDepthMobile });
#endif
                    }

                    if (trackChangesDuringPlaytime.boolValue)
                        tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Beam.SD.HelpTrackChangesEnabled });
                }
            }

            base.GetInfoTips(tips);
        }

        protected virtual void OnSceneGUI()
        {
            if(DrawEditInSceneHandles())
            {
                var beam = target as VolumetricLightBeamSD;
                Debug.Assert(beam != null);
                beam.UpdateAfterManualPropertyChange();
            }
        }
    }
}
#endif
