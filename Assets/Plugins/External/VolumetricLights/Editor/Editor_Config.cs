#if UNITY_EDITOR

#if UNITY_2019_3_OR_NEWER
#define VLB_LIGHT_TEMPERATURE_SUPPORT
#endif

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace VLB
{
    [CustomEditor(typeof(Config))]
    public class Editor_Config : Editor_Common
    {
        SerializedProperty geometryOverrideLayer = null, geometryLayerID = null, geometryTag = null, geometryRenderQueue = null, geometryRenderQueueHD = null, renderPipeline = null, renderingMode = null;
        SerializedProperty sharedMeshSides = null, sharedMeshSegments = null;
        SerializedProperty hdBeamsCameraBlendingDistance = null;
        SerializedProperty urpDepthCameraScriptableRendererIndex = null;
        SerializedProperty globalNoiseScale = null, globalNoiseVelocity = null;
        SerializedProperty fadeOutCameraTag = null;
        SerializedProperty noiseTexture3D = null;
        SerializedProperty dustParticlesPrefab = null;
        SerializedProperty ditheringFactor = null, ditheringNoiseTexture = null, jitteringNoiseTexture = null;
        SerializedProperty raymarchingQualities = null, defaultRaymarchingQualityUniqueID = null;
        SerializedProperty featureEnabledColorGradient = null, featureEnabledDepthBlend = null, featureEnabledNoise3D = null, featureEnabledDynamicOcclusion = null, featureEnabledMeshSkewing = null, featureEnabledShaderAccuracyHigh = null;
        SerializedProperty featureEnabledShadow = null, featureEnabledCookie = null;
#if VLB_LIGHT_TEMPERATURE_SUPPORT
        SerializedProperty useLightColorTemperature = null;
#endif

        Config m_TargetConfig = null;
        RenderQueueDrawer m_DrawerRenderQueue;
        RenderQueueDrawer m_DrawerRenderQueueHD;

        protected override void OnEnable()
        {
            base.OnEnable();
            RetrieveSerializedProperties("m_");

            m_DrawerRenderQueue = new RenderQueueDrawer(geometryRenderQueue);
            m_DrawerRenderQueueHD = new RenderQueueDrawer(geometryRenderQueueHD);

            Noise3D.LoadIfNeeded(); // Try to load Noise3D, maybe for the 1st time

            m_TargetConfig = this.target as Config;

            RaymarchingQualitiesInit();
        }

        void RenderingModeGUIDraw(SerializedProperty sprop, GUIContent label)
        {
            EditorGUILayout.PropertyField(sprop, label);

            if (renderPipeline.enumValueIndex == (int)RenderPipeline.BuiltIn)
            {
                if (sprop.enumValueIndex == (int)RenderingMode.SRPBatcher)
                    EditorGUILayout.HelpBox(EditorStrings.Config.GetErrorSrpBatcherOnlyCompatibleWithSrp(ShaderMode.SD), MessageType.Error);
            }
            else
            {
                if (sprop.enumValueIndex == (int)RenderingMode.MultiPass)
                    EditorGUILayout.HelpBox(EditorStrings.Config.GetErrorSrpAndMultiPassNotCompatible(ShaderMode.SD), MessageType.Error);
            }
        }

        protected override void OnHeaderGUI()
        {
            GUILayout.BeginVertical("In BigTitle");
            EditorGUILayout.Separator();

            var title = string.Format("Volumetric Light Beam - Plugin Configuration");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(string.Format("Current Version: {0}", Version.CurrentAsString), EditorStyles.miniBoldLabel);
            EditorGUILayout.Separator();
            GUILayout.EndVertical();
        }

        void OnAddConfigPerPlatform(object platform)
        {
            var p = (RuntimePlatform)platform;
            var clone = UnityEngine.Object.Instantiate(m_TargetConfig);
            Debug.Assert(clone);
            var path = AssetDatabase.GetAssetPath(m_TargetConfig).Replace(m_TargetConfig.name + Config.kAssetNameExt, "");
            Config.CreateAsset(clone, path + Config.kAssetName + p.ToString() + Config.kAssetNameExt);
            Selection.activeObject = clone;
        }

        [System.Flags]
        enum DirtyFlags
        {
            Target = 1 << 1,
            Shader = 1 << 2,
            Noise = 1 << 3,
            GlobalMesh = 1 << 4,
            AllBeamGeom = 1 << 5,
            AllMeshes = 1 << 6,
            DepthCameras = 1 << 7,
        }

        void SetDirty(DirtyFlags flags)
        {
            if (m_IsUsedInstance)
            {
                if (flags.HasFlag(DirtyFlags.Target)) EditorUtility.SetDirty(target);
                if (flags.HasFlag(DirtyFlags.Shader)) m_NeedToRefreshShader = true;
                if (flags.HasFlag(DirtyFlags.Noise)) m_NeedToReloadNoise = true;
                if (flags.HasFlag(DirtyFlags.GlobalMesh)) GlobalMeshSD.Destroy();
                if (flags.HasFlag(DirtyFlags.AllBeamGeom)) Utils._EditorSetAllBeamGeomDirty();
                if (flags.HasFlag(DirtyFlags.AllMeshes)) Utils._EditorSetAllMeshesDirty();
                if (flags.HasFlag(DirtyFlags.DepthCameras))
                {
                    var allDynOcclSD = Resources.FindObjectsOfTypeAll<DynamicOcclusionDepthBuffer>();
                    foreach (var instance in allDynOcclSD)
                        instance.ForceReinstantiateDepthCamera();

                    var allShadowsHD = Resources.FindObjectsOfTypeAll<VolumetricShadowHD>();
                    foreach (var instance in allShadowsHD)
                        instance.ForceReinstantiateDepthCamera();
                }
            }
        }

        bool m_NeedToReloadNoise = false;
        bool m_NeedToRefreshShader = false;
        bool m_IsUsedInstance = false;


        #region Raymarching Qualities
        ReorderableList m_ListQualities;

        void RaymarchingQualitiesInit()
        {
            m_ListQualities = new ReorderableList(serializedObject
                , raymarchingQualities
                , true // draggable
                , true // displayHeader
                , true // displayAddButton
                , true // displayRemoveButton
                );
            m_ListQualities.drawHeaderCallback = RaymarchingQualitiesDrawHeader;
            m_ListQualities.drawElementCallback = RaymarchingQualitiesDrawElement;
            m_ListQualities.onAddCallback = RaymarchingQualitiesOnAdd;
            m_ListQualities.onRemoveCallback = RaymarchingQualitiesOnRemove;
            m_ListQualities.onChangedCallback = RaymarchingQualitiesOnChanged;
            m_ListQualities.onCanRemoveCallback = RaymarchingQualitiesOnCanRemove;
        }

        void RaymarchingQualitiesDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, EditorStrings.Config.HD.TitleRaymarchingQuality);
        }

        void RaymarchingQualitiesDrawElement(Rect rect, int i, bool isActive, bool isFocused)
        {
            const float kBorder = 1.0f;
            rect.yMin += kBorder;
            rect.yMax -= kBorder * 2;

            const float kPropSeparator = 5.0f;
            const float kWidthSteps = 50.0f;

            Rect rectName = rect;
            rectName.width -= kWidthSteps + kPropSeparator;

            Rect rectSteps = rect;
            rectSteps.x = rect.xMax - kWidthSteps;
            rectSteps.width = kWidthSteps;

            var qual = m_TargetConfig.GetRaymarchingQualityForIndex(i);
            {
                EditorGUI.BeginChangeCheck();
                qual.name = EditorGUI.TextField(rectName, qual.name);
                if (EditorGUI.EndChangeCheck())
                    SetDirty(DirtyFlags.Target);

                EditorGUI.BeginChangeCheck();
                qual.stepCount = Mathf.Max(EditorGUI.IntField(rectSteps, qual.stepCount), Consts.Config.HD.RaymarchingQualitiesStepsMin);
                if (EditorGUI.EndChangeCheck())
                    SetDirty(DirtyFlags.Target | DirtyFlags.Shader);
            }
        }

        void RaymarchingQualitiesOnAdd(ReorderableList list)
        {
            var newQual = RaymarchingQuality.New();
            m_TargetConfig.AddRaymarchingQuality(newQual);
            SetDirty(DirtyFlags.Target);
        }

        void RaymarchingQualitiesOnRemove(ReorderableList list)
        {
            if (list.count < 1)
            {
                Debug.LogError("Having at least 1 RaymarchingQuality value is mandatory: cannot delete.");
                return;
            }

            var qual = m_TargetConfig.GetRaymarchingQualityForIndex(list.index);
            if (qual != null)
            {
                if (qual.uniqueID == m_TargetConfig.defaultRaymarchingQualityUniqueID)
                {
                    EditorUtility.DisplayDialog(string.Format("Can't remove Raymarching Quality '{0}'", qual.name)
                    , string.Format("We cannot remove Raymarching Quality '{0}' with {1} steps because it's the default quality", qual.name, qual.stepCount)
                    , "Ok");
                    return;
                }

                if (EditorUtility.DisplayDialog(string.Format("Remove Raymarching Quality '{0}'?", qual.name)
                    , string.Format("Do you really want to remove the Raymarching Quality '{0}' with {1} steps?\nAll Volumetric Light Beams using this quality will now use the default quality.", qual.name, qual.stepCount)
                    , "Ok"
                    , "Cancel"))
                {
                    m_TargetConfig.RemoveRaymarchingQualityAtIndex(list.index);
                    SetDirty(DirtyFlags.Target | DirtyFlags.AllBeamGeom); // force beams in opened scenes to regenerate with default quality in case they used removed quality
                }
            }
        }

        void RaymarchingQualitiesOnChanged(ReorderableList list)
        {
            SetDirty(DirtyFlags.Target | DirtyFlags.Shader);
        }

        bool RaymarchingQualitiesOnCanRemove(ReorderableList list)
        {
            var qual = m_TargetConfig.GetRaymarchingQualityForIndex(list.index);
            if (qual != null && qual.uniqueID == m_TargetConfig.defaultRaymarchingQualityUniqueID)
                return false; // cannot remove default quality

            return list.count > 1;
        }

        void RaymarchingQualitiesDraw()
        {
            Debug.Assert(m_ListQualities != null);
            m_ListQualities.DoLayoutList();

            DrawRaymarchingQualitiesPopup(Config.Instance, defaultRaymarchingQualityUniqueID, EditorStrings.Config.HD.DefaultRaymarchingQuality);

#if VLB_DEBUG
            for (int i = 0; i < m_TargetConfig.raymarchingQualitiesCount; ++i)
            {
                var qual = m_TargetConfig.GetRaymarchingQualityForIndex(i);
                if (qual != null)
                {
                    EditorGUILayout.LabelField(string.Format("#DEBUG# [{0}] {1} - {2} - {3}"
                        , i
                        , qual.uniqueID
                        , qual.name
                        , qual.stepCount
                        ));
                }
            }
#endif // VLB_DEBUG
        }

        public static void DrawRaymarchingQualitiesPopup(Config instance, SerializedProperty prop, GUIContent content)
        {
            Debug.Assert(instance != null);

            int selectedIndex = -1;
            var descriptions = new GUIContent[instance.raymarchingQualitiesCount];

            for (int i = 0; i < instance.raymarchingQualitiesCount; ++i)
            {
                var qual = instance.GetRaymarchingQualityForIndex(i);
                descriptions[i] = new GUIContent(string.Format("{0} ({1})", qual.name, qual.stepCount));

                if (qual.uniqueID == prop.intValue)
                    selectedIndex = i;
            }

            if (selectedIndex < 0)
            {   // in case we couldn't find the serialized ID, fallback to 0
                selectedIndex = 0;

                var fallback = instance.GetRaymarchingQualityForIndex(selectedIndex);
                Debug.LogErrorFormat("Failed to find default raymarching quality index in popup that fit with quality unique ID {0}.", prop.intValue);
            }

            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
                    selectedIndex = EditorGUILayout.Popup(content, selectedIndex, descriptions);
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                var newQual = instance.GetRaymarchingQualityForIndex(selectedIndex);
                prop.intValue = newQual.uniqueID;
            }

#if VLB_DEBUG
            {
                var qual = instance.GetRaymarchingQualityForIndex(selectedIndex);
                EditorGUILayout.LabelField(string.Format("#DEBUG# Serialized Unique ID: {0} | Found Unique ID: {1} / Steps: {2}", prop.intValue, qual.uniqueID, qual.stepCount));
            }
#endif // VLB_DEBUG
        }
        #endregion

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Debug.Assert(m_TargetConfig != null);

            m_NeedToReloadNoise = false;
            m_NeedToRefreshShader = false;
            m_IsUsedInstance = m_TargetConfig.IsCurrentlyUsedInstance();

            // Config per plaftorm
#if UNITY_2018_1_OR_NEWER
            {
                bool hasValidName = m_TargetConfig.HasValidAssetName();
                bool isCurrentPlatformSuffix = m_TargetConfig.GetAssetSuffix() == PlatformHelper.GetCurrentPlatformSuffix();

                var platformSuffix = m_TargetConfig.GetAssetSuffix();
                string platformStr = "Default Config asset";
                if(!string.IsNullOrEmpty(platformSuffix))
                    platformStr = string.Format("Config asset for platform '{0}'", m_TargetConfig.GetAssetSuffix());
                if (!hasValidName)
                    platformStr += " (INVALID)";
                EditorGUILayout.LabelField(platformStr, EditorStyles.boldLabel);

                if (GUILayout.Button(EditorStrings.Beam.ButtonCreateOverridePerPlatform, EditorStyles.miniButton))
                {
                    var menu = new GenericMenu();
                    foreach (var platform in System.Enum.GetValues(typeof(RuntimePlatform)))
                        menu.AddItem(new GUIContent(platform.ToString()), false, OnAddConfigPerPlatform, platform);
                    menu.ShowAsContext();
                }

                if (!hasValidName)
                {
                    EditorGUILayout.Separator();
                    EditorGUILayout.HelpBox(EditorStrings.Config.InvalidPlatformOverride, MessageType.Error);
                    ButtonOpenConfig();
                }
                else if (!m_IsUsedInstance)
                {
                    EditorGUILayout.Separator();

                    if (isCurrentPlatformSuffix)
                        EditorGUILayout.HelpBox(EditorStrings.Config.WrongAssetLocation, MessageType.Error);
                    else
                        EditorGUILayout.HelpBox(EditorStrings.Config.NotCurrentAssetInUse, MessageType.Warning);

                    ButtonOpenConfig();
                }

                EditorExtensions.DrawLineSeparator();
            }
#endif

            {
                EditorGUI.BeginChangeCheck();
                {
                    if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderBeamGeometry))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            geometryOverrideLayer.boolValue = EditorGUILayout.Toggle(EditorStrings.Config.GeometryOverrideLayer, geometryOverrideLayer.boolValue);
                            using (new EditorGUI.DisabledGroupScope(!geometryOverrideLayer.boolValue))
                            {
                                geometryLayerID.intValue = EditorGUILayout.LayerField(geometryLayerID.intValue);
                            }
                        }

                        geometryTag.stringValue = EditorGUILayout.TagField(EditorStrings.Config.GeometryTag, geometryTag.stringValue);
                    }
                    FoldableHeader.End();

                    if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderRendering))
                    {
                        m_DrawerRenderQueue.Draw(EditorStrings.Config.GeometryRenderQueueSD);
                        m_DrawerRenderQueueHD.Draw(EditorStrings.Config.GeometryRenderQueueHD);

                        if (BeamGeometrySD.isCustomRenderPipelineSupported)
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                renderPipeline.CustomEnum<RenderPipeline>(EditorStrings.Config.GeometryRenderPipeline, EditorStrings.Config.GeometryRenderPipelineEnumDescriptions);
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetDirty(DirtyFlags.AllBeamGeom | DirtyFlags.Shader); // need to fully reset the BeamGeom to update the shader
                                SRPHelper.SetScriptingDefineSymbolsForRenderPipeline((RenderPipeline)renderPipeline.enumValueIndex);
                            }
                        }

                        if (m_TargetConfig.hasRenderPipelineMismatch)
                            EditorGUILayout.HelpBox(EditorStrings.Config.ErrorRenderPipelineMismatch, MessageType.Error);

#if VLB_DEBUG
                        EditorGUILayout.LabelField("#DEBUG# RP Scripting Define Symbol: " + SRPHelper.renderPipelineScriptingDefineSymbolAsString);
#endif

                        EditorGUI.BeginChangeCheck();
                        {
                            RenderingModeGUIDraw(renderingMode, EditorStrings.Config.GeometryRenderingMode);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            SetDirty(DirtyFlags.AllBeamGeom | DirtyFlags.GlobalMesh | DirtyFlags.Shader); // need to fully reset the BeamGeom to update the shader
                        }

                        if (m_TargetConfig.GetBeamShader(ShaderMode.SD) == null)
                            EditorGUILayout.HelpBox(EditorStrings.Config.GetErrorInvalidShader(), MessageType.Error);

                        if (m_TargetConfig.GetBeamShader(ShaderMode.HD) == null)
                            EditorGUILayout.HelpBox(EditorStrings.Config.GetErrorInvalidShader(), MessageType.Error);

                        if (ditheringFactor.FloatSlider(EditorStrings.Config.DitheringFactor, 0.0f, 1.0f))
                        {
                            SetDirty(DirtyFlags.Shader);
                        }

#if VLB_LIGHT_TEMPERATURE_SUPPORT
                        EditorGUILayout.PropertyField(useLightColorTemperature, EditorStrings.Config.UseLightColorTemperature);
#endif
                    }
                    FoldableHeader.End();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Utils._EditorSetAllMeshesDirty();
                }

                if (m_TargetConfig.renderPipeline == RenderPipeline.URP)
                {
                    if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderURPSpecific))
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(urpDepthCameraScriptableRendererIndex, EditorStrings.Config.URPDepthCameraScriptableRendererIndex);
                        if (EditorGUI.EndChangeCheck()) { SetDirty(DirtyFlags.DepthCameras); }
                    }
                    FoldableHeader.End();
                }

                if (FoldableHeader.Begin(this, EditorStrings.Config.HD.HDSpecific))
                {
                    RaymarchingQualitiesDraw();

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(hdBeamsCameraBlendingDistance, EditorStrings.Config.HD.CameraBlendingDistance);
                    if (EditorGUI.EndChangeCheck()) { SetDirty(DirtyFlags.Shader); }
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderSharedMesh))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(sharedMeshSides, EditorStrings.Config.SharedMeshSides);
                    EditorGUILayout.PropertyField(sharedMeshSegments, EditorStrings.Config.SharedMeshSegments);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetDirty(DirtyFlags.GlobalMesh | DirtyFlags.AllMeshes);
                    }

                    var meshInfo = "These properties will change the mesh tessellation of each Volumetric Light Beam with 'Shared' MeshType.\nAdjust them carefully since they could impact performance.";
                    meshInfo += string.Format("\nShared Mesh stats: {0} vertices, {1} triangles", MeshGenerator.GetSharedMeshVertexCount(), MeshGenerator.GetSharedMeshIndicesCount() / 3);
                    EditorGUILayout.HelpBox(meshInfo, MessageType.Info);
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderGlobal3DNoise))
                {
                    EditorGUILayout.PropertyField(globalNoiseScale, EditorStrings.Config.GlobalNoiseScale);
                    EditorGUILayout.PropertyField(globalNoiseVelocity, EditorStrings.Config.GlobalNoiseVelocity);
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderFadeOutCamera))
                {
                    EditorGUI.BeginChangeCheck();
                    fadeOutCameraTag.stringValue = EditorGUILayout.TagField(EditorStrings.Config.FadeOutCameraTag, fadeOutCameraTag.stringValue);
                    if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                        m_TargetConfig.ForceUpdateFadeOutCamera();
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderFeaturesEnabled))
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(featureEnabledColorGradient, EditorStrings.Config.FeatureEnabledColorGradient);
                        EditorGUILayout.PropertyField(featureEnabledNoise3D, EditorStrings.Config.FeatureEnabledNoise3D);
                        EditorGUILayout.PropertyField(featureEnabledDepthBlend, EditorStrings.Config.SD.FeatureEnabledDepthBlend);
                        EditorGUILayout.PropertyField(featureEnabledDynamicOcclusion, EditorStrings.Config.SD.FeatureEnabledDynamicOcclusion);
                        EditorGUILayout.PropertyField(featureEnabledMeshSkewing, EditorStrings.Config.SD.FeatureEnabledMeshSkewing);
                        EditorGUILayout.PropertyField(featureEnabledShaderAccuracyHigh, EditorStrings.Config.SD.FeatureEnabledShaderAccuracyHigh);
                        EditorGUILayout.PropertyField(featureEnabledShadow, EditorStrings.Config.HD.FeatureEnabledShadow);
                        EditorGUILayout.PropertyField(featureEnabledCookie, EditorStrings.Config.HD.FeatureEnabledCookie);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetDirty(DirtyFlags.Shader | DirtyFlags.AllBeamGeom);
                    }
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderInternalData))
                {
                    EditorGUILayout.PropertyField(dustParticlesPrefab, EditorStrings.Config.DustParticlesPrefab);

                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(noiseTexture3D, EditorStrings.Config.NoiseTexture3D);
                        if (EditorGUI.EndChangeCheck())
                            SetDirty(DirtyFlags.Noise);

                        if (Noise3D.isSupported && !Noise3D.isProperlyLoaded)
                            EditorGUILayout.HelpBox(EditorStrings.Common.HelpNoiseLoadingFailed, MessageType.Error);
                    }

                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(ditheringNoiseTexture, EditorStrings.Config.DitheringNoiseTexture);
                        EditorGUILayout.PropertyField(jitteringNoiseTexture, EditorStrings.Config.HD.JitteringNoiseTexture);
                        if (EditorGUI.EndChangeCheck())
                            SetDirty(DirtyFlags.Shader);
                    }
                }
                FoldableHeader.End();

                if (GUILayout.Button(EditorStrings.Config.OpenDocumentation, EditorStyles.miniButton))
                {
                    UnityEditor.Help.BrowseURL(Consts.Help.UrlConfig);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(EditorStrings.Config.CopyDebugInfo, EditorStyles.miniButton))
                    {
                        var debugInfo = m_TargetConfig.GetDebugInfo();
                        GUIUtility.systemCopyBuffer = debugInfo;
                        Debug.Log("Copied to clipboard:\n" + debugInfo);
                    }


                    if (GUILayout.Button(EditorStrings.Config.ClearAssetStoreCache, EditorStyles.miniButton))
                    {
                        ClearAssetStoreCache();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(EditorStrings.Config.ResetToDefaultButton, EditorStyles.miniButton))
                    {
                        UnityEditor.Undo.RecordObject(target, "Reset Config Properties");
                        m_TargetConfig.Reset();
                        SetDirty(DirtyFlags.Target | DirtyFlags.Noise);
                    }

                    if (GUILayout.Button(EditorStrings.Config.ResetInternalDataButton, EditorStyles.miniButton))
                    {
                        UnityEditor.Undo.RecordObject(target, "Reset Internal Data");
                        m_TargetConfig.ResetInternalData();
                        SetDirty(DirtyFlags.Target | DirtyFlags.Noise);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (m_NeedToRefreshShader)
                m_TargetConfig.RefreshShaders(Config.RefreshShaderFlags.All); // need to be done AFTER ApplyModifiedProperties

            if (m_NeedToReloadNoise)
                Noise3D._EditorForceReloadData(); // Should be called AFTER ApplyModifiedProperties so the Config instance has the proper values when reloading data
        }

        static string GetAssetStoreCacheFolder()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                {
                    const string kAppData = "AppData";
                    var appDataPath = Application.persistentDataPath;
                    appDataPath = appDataPath.Substring(0, appDataPath.IndexOf(kAppData) + kAppData.Length);
                    return System.IO.Path.Combine(appDataPath, "Roaming/Unity/Asset Store-5.x/Tech Salad/");
                }

                case RuntimePlatform.OSXEditor:
                    return "~/Library/Unity/Asset Store-5.x/Tech Salad/";

                case RuntimePlatform.LinuxEditor:
                    return  "~/.local/share/unity3d/Asset Store-5.x/Tech Salad/";

                default:
                    return null;
            }
        }

        static void ClearAssetStoreCache()
        {
            string path = GetAssetStoreCacheFolder();
            if (path == string.Empty)
            {
                EditorUtility.DisplayDialog("Clear Asset Store Cache"
                    , "Failed to compute Asset Store cache folder"
                    , "Ok");
                return;
            }

            if(FileUtil.DeleteFileOrDirectory(path))
            {
                EditorUtility.DisplayDialog("Clear Asset Store Cache"
                    , string.Format("Folder '{0}' has been cleared successfully", path)
                    , "Ok");
            }
            else
            {
                EditorUtility.DisplayDialog("Clear Asset Store Cache"
                    , string.Format("Failed to clear folder '{0}'", path)
                    , "Ok");
            }
        }
    }
}
#endif
