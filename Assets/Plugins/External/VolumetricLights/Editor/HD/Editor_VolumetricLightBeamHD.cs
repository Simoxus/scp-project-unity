#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [CustomEditor(typeof(VolumetricLightBeamHD))]
    [CanEditMultipleObjects]
    public class Editor_VolumetricLightBeamHD : Editor_VolumetricLightBeamAbstractBase
    {
        SerializedProperty m_ColorFromLight = null, m_ColorMode = null, m_ColorFlat = null, m_ColorGradient = null;
        SerializedProperty m_Intensity = null, m_IntensityMultiplier = null;
        SerializedProperty m_BlendingMode = null;
        SerializedProperty m_SideSoftness = null;
        SerializedProperty m_SpotAngle = null, m_SpotAngleMultiplier = null;
        SerializedProperty m_ConeRadiusStart = null;
        SerializedProperty m_Scalable = null;
        SerializedProperty m_FallOffStart = null, m_FallOffEnd = null, m_FallOffEndMultiplier = null;
        SerializedProperty m_AttenuationEquation = null;
        SerializedProperty m_NoiseMode = null, m_NoiseIntensity = null, m_NoiseScaleUseGlobal = null, m_NoiseScaleLocal = null, m_NoiseVelocityUseGlobal = null, m_NoiseVelocityLocal = null;
        SerializedProperty m_JitteringFactor = null, m_JitteringFrameRate = null, m_JitteringLerpRange = null, m_RaymarchingQualityID = null;

        TargetList<VolumetricLightBeamHD> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            RetrieveSerializedProperties("m_");
            m_Targets = new TargetList<VolumetricLightBeamHD>(targets);
        }

        void DisplayDebugInfo(VolumetricLightBeamHD beam)
        {
            Debug.Assert(beam);
            var geom = beam._EDITOR_GetBeamGeometry();
            if(!geom)
            {
                EditorGUILayout.LabelField("No BeamGeometry");
                return;
            }

#if VLB_DEBUG
            string matInfo = string.Format("Material: {0} | ID: {1} / {2}"
                , geom._EDITOR_IsUsingCustomMaterial ? "CUSTOM" : "INSTANCED"
                , geom._EDITOR_InstancedMaterialID
                , MaterialManager.StaticPropertiesHD.staticPropertiesCount);
            EditorGUILayout.LabelField(matInfo);
#endif // VLB_DEBUG
        }

        protected virtual void DrawProperties(bool hasLightSpot)
        {
            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderBasic))
            {
                // Color
                using (ButtonToggleScope.FromLight(m_ColorFromLight, hasLightSpot))
                {
                    if (!hasLightSpot) EditorGUILayout.BeginHorizontal();    // mandatory to have the color picker on the same line (when the button "from light" is not here)
                    {
                        if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off)
                        {
                            EditorGUILayout.PropertyField(m_ColorFlat, EditorStrings.Beam.ColorMode);
                        }
                        else
                        {
                            using (new EditorExtensions.LabelWidth(65f))
                            {
                                EditorGUILayout.PropertyField(m_ColorMode, EditorStrings.Beam.ColorMode);
                            }

                            if (m_ColorMode.enumValueIndex == (int)ColorMode.Gradient)
                                EditorGUILayout.PropertyField(m_ColorGradient, EditorStrings.Beam.ColorGradient);
                            else
                                EditorGUILayout.PropertyField(m_ColorFlat, EditorStrings.Beam.ColorFlat);
                        }
                    }
                    if (!hasLightSpot) EditorGUILayout.EndHorizontal();
                }

                // Blending Mode
                EditorGUILayout.PropertyField(m_BlendingMode, EditorStrings.Beam.BlendingMode);

                // Intensity
                using (var lightDisabledGrp = ButtonToggleScope.FromLight(m_IntensityMultiplier, hasLightSpot))
                {
                    if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamHD beam) => { return beam.useIntensityFromAttachedLightSpot; }))
                    {
                        using (new EditorExtensions.ShowMixedValue(m_Intensity))
                        {
                            // display grayed out Unity's light intensity
                            EditorGUILayout.FloatField(EditorStrings.Beam.HD.Intensity, SpotLightHelper.GetIntensity(m_Targets[0].lightSpotAttached));
                        }

                        lightDisabledGrp?.EndDisabledGroup(); // muliplier factor should be available
                        DrawMultiplierProperty(m_IntensityMultiplier, EditorStrings.Beam.IntensityMultiplier); // multiplier property
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(m_Intensity, EditorStrings.Beam.HD.Intensity);
                    }
                }

                EditorGUILayout.Slider(m_SideSoftness, Consts.Beam.HD.SideSoftnessMin, Consts.Beam.HD.SideSoftnessMax, EditorStrings.Beam.HD.SideSoftness);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderShape))
            {
                // Fade End
                using (var lightDisabledGrp = ButtonToggleScope.FromLight(m_FallOffEndMultiplier, hasLightSpot))
                {
                    if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamHD beam) => { return beam.useFallOffEndFromAttachedLightSpot; }))
                    {
                        using (new EditorExtensions.ShowMixedValue(m_FallOffEnd))
                        {
                            // display grayed out Unity's light range
                            EditorGUILayout.FloatField(EditorStrings.Beam.FallOffEnd, SpotLightHelper.GetFallOffEnd(m_Targets[0].lightSpotAttached));
                        }

                        lightDisabledGrp?.EndDisabledGroup(); // muliplier factor should be available
                        DrawMultiplierProperty(m_FallOffEndMultiplier, EditorStrings.Beam.FallOffEndMultiplier); // multiplier property
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(m_FallOffEnd, EditorStrings.Beam.FallOffEnd);
                    }
                }

                // Spot Angle
                using (var lightDisabledGrp = ButtonToggleScope.FromLight(m_SpotAngleMultiplier, hasLightSpot))
                {
                    if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamHD beam) => { return beam.useSpotAngleFromAttachedLightSpot; }))
                    {
                        using (new EditorExtensions.ShowMixedValue(m_SpotAngle))
                        {
                            // display grayed out Unity's light angle
                            EditorGUILayout.FloatField(EditorStrings.Beam.SpotAngle, SpotLightHelper.GetSpotAngle(m_Targets[0].lightSpotAttached));
                        }

                        lightDisabledGrp?.EndDisabledGroup(); // muliplier factor should be available
                        DrawMultiplierProperty(m_SpotAngleMultiplier, EditorStrings.Beam.SpotAngleMultiplier); // multiplier property
                    }
                    else
                    {
                        EditorGUILayout.Slider(m_SpotAngle, Consts.Beam.SpotAngleMin, Consts.Beam.SpotAngleMax, EditorStrings.Beam.SpotAngle);
                    }
                }

                EditorGUILayout.PropertyField(m_ConeRadiusStart, EditorStrings.Beam.ConeRadiusStart);

                EditorGUILayout.PropertyField(m_Scalable, EditorStrings.Beam.Scalable);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HD.HeaderRaymarching))
            {
                Editor_Config.DrawRaymarchingQualitiesPopup(Config.Instance, m_RaymarchingQualityID, EditorStrings.Beam.HD.RaymarchingQuality);

                EditorGUILayout.PropertyField(m_JitteringFactor, EditorStrings.Beam.HD.JitteringFactor);

                if(m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamHD beam) => { return beam.jitteringFactor > 0.0f; }))
                {
                    EditorGUILayout.IntSlider(m_JitteringFrameRate, Consts.Beam.HD.JitteringFrameRateMin, Consts.Beam.HD.JitteringFrameRateMax, EditorStrings.Beam.HD.JitteringFrameRate);
                    EditorGUILayout.PropertyField(m_JitteringLerpRange, EditorStrings.Beam.HD.JitteringLerpRange);
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderAttenuation))
            {
                EditorGUILayout.PropertyField(m_AttenuationEquation, EditorStrings.Beam.HD.AttenuationEquation);

                if (m_FallOffEnd.hasMultipleDifferentValues)
                    EditorGUILayout.PropertyField(m_FallOffStart, EditorStrings.Beam.FallOffStart);
                else
                    m_FallOffStart.FloatSlider(EditorStrings.Beam.FallOffStart, 0f, m_FallOffEnd.floatValue - Consts.Beam.FallOffDistancesMinThreshold);

                EditorGUILayout.Separator();
            }
            FoldableHeader.End();

            if (Config.Instance.featureEnabledNoise3D)
            {
                if (FoldableHeader.Begin(this, EditorStrings.Beam.Header3DNoise))
                {
                    m_NoiseMode.CustomEnum<NoiseMode>(EditorStrings.Beam.NoiseMode, EditorStrings.Beam.NoiseModeEnumDescriptions);

                    bool showNoiseProps = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamHD beam) => { return beam.isNoiseEnabled; });
                    if (showNoiseProps)
                    {
                        EditorGUILayout.Slider(m_NoiseIntensity, Consts.Beam.NoiseIntensityMin, Consts.Beam.NoiseIntensityMax, EditorStrings.Beam.NoiseIntensity);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(m_NoiseScaleUseGlobal.boolValue))
                            {
                                EditorGUILayout.Slider(m_NoiseScaleLocal, Consts.Beam.NoiseScaleMin, Consts.Beam.NoiseScaleMax, EditorStrings.Beam.NoiseScale);
                            }
                            m_NoiseScaleUseGlobal.ToggleUseGlobalNoise();
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(m_NoiseVelocityUseGlobal.boolValue))
                            {
                                EditorGUILayout.PropertyField(m_NoiseVelocityLocal, EditorStrings.Beam.NoiseVelocity);
                            }
                            m_NoiseVelocityUseGlobal.ToggleUseGlobalNoise();
                        }

                        if (Noise3D.isSupported && !Noise3D.isProperlyLoaded)
                            EditorGUILayout.HelpBox(EditorStrings.Common.HelpNoiseLoadingFailed, MessageType.Error);

                        if (!Noise3D.isSupported)
                            EditorGUILayout.HelpBox(Noise3D.isNotSupportedString, MessageType.Info);
                    }
                }
                FoldableHeader.End();
            }
        }

        public sealed override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Debug.Assert(m_Targets.Count > 0);

            // Prevent from drawing inspector when editor props are dirty
            // in Unity 2022, inspector is drawn BEFORE calling Update & GenerateGeometry which set a valid raymarching ID
            // and that generates a error message in DrawRaymarchingQualitiesPopup
            foreach (var target in m_Targets)
            {
                // make sure to display inspector when selecting a prefab asset (since update is not called for prefab asset in project browser, its EditorDirtyFlags.Props will be always true)
                bool isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(target.gameObject);

                if(target.enabled // always display inspector for disabled component: when disabling a Beam, OnValidate is called which set EditorDirtyFlags.Props
                && !isPrefabAsset
                && target._EditorIsDirty())
                    return;
            }

            DisplayDebugInfo(m_Targets[0]);

            VolumetricLightBeamSD.AttachedLightType lightType;
            bool hasLightSpot = m_Targets[0].GetLightSpotAttachedSlow(out lightType) != null;
            if (lightType == VolumetricLightBeamSD.AttachedLightType.OtherLight)
            {
                EditorGUILayout.HelpBox(EditorStrings.Beam.HelpNoSpotlight, MessageType.Warning);
            }

            DrawProperties(hasLightSpot);

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

        void DrawCustomActionButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(EditorStrings.Beam.ButtonResetProperties, EditorStyles.miniButton))
                {
                    m_Targets.RecordUndoAction("Reset Light Beam Properties",
                        (VolumetricLightBeamHD beam) => { beam.Reset(); beam.GenerateGeometry(); } );
                }
            }
        }

        void DrawAdditionalFeatures()
        {
            if (Application.isPlaying) return; // do not support adding additional components at runtime

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawButtonAddComponentDust();

                bool showButtonCookie = Config.Instance.featureEnabledCookie && m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamHD beam) => { return beam.GetComponent<VolumetricCookieHD>() == null; });
                if (showButtonCookie && GUILayout.Button(EditorData.Instance.contentAddCookieHD, buttonAddComponentStyle))
                    AddComponentToTargets<VolumetricCookieHD>();

                bool showButtonShadow = Config.Instance.featureEnabledShadow && m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamHD beam) => { return beam.GetComponent<VolumetricShadowHD>() == null; });
                if (showButtonShadow && GUILayout.Button(EditorData.Instance.contentAddShadowHD, buttonAddComponentStyle))
                    AddComponentToTargets<VolumetricShadowHD>();

                DrawButtonAddComponentEffect();
                DrawButtonAddComponentTriggerZone();

                bool showButtonTrackRealtime = m_Targets.HaveAll((VolumetricLightBeamHD beam) => { return beam.GetComponent<Light>() != null && beam.GetComponent<TrackRealtimeChangesOnLightHD>() == null; });
                if (showButtonTrackRealtime && GUILayout.Button(EditorData.Instance.contentAddTrackRealtimeChangesOnLightHD, buttonAddComponentStyle))
                    AddComponentToTargets<TrackRealtimeChangesOnLightHD>();
            }
        }
        
        protected virtual void OnSceneGUI()
        {
            DrawEditInSceneHandles();
        }
    }
}
#endif
