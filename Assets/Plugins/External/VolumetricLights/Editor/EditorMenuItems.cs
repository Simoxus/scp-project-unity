#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace VLB
{
    public static class EditorMenuItems
    {
        const string kCategoryName = "\U0001F4A1 Volumetric Light Beam/";
        const string kSceneMenuPrefix = "GameObject/Light/" + kCategoryName;

        static class SD
        {
            const string kNewBeamPrefix = kSceneMenuPrefix + "SD Beam/";

            [MenuItem(kNewBeamPrefix + "3D Beam", false, 100)]
            static void Menu_CreateNewBeam(MenuCommand menuCommand) { EditorExtensions.OnNewGameObjectCreated(EditorExtensions.SD.NewBeam(), menuCommand); }

            [MenuItem(kNewBeamPrefix + "3D Beam and Spotlight", false, 101)]
            static void Menu_CreateSpotLightAndBeam(MenuCommand menuCommand) { EditorExtensions.OnNewGameObjectCreated(EditorExtensions.SD.NewSpotLightAndBeam(), menuCommand); }

            [MenuItem(kNewBeamPrefix + "2D Beam", false, 102)]
            static void Menu_CreateNewBeam2D(MenuCommand menuCommand) { EditorExtensions.OnNewGameObjectCreated(EditorExtensions.SD.NewBeam2D(), menuCommand); }
        }

        static class HD
        {
            const string kNewBeamPrefix = kSceneMenuPrefix + "HD Beam/";

            [MenuItem(kNewBeamPrefix + "3D Beam", false, 200)]
            static void Menu_CreateNewBeam(MenuCommand menuCommand) { EditorExtensions.OnNewGameObjectCreated(EditorExtensions.HD.NewBeam(), menuCommand); }

            [MenuItem(kNewBeamPrefix + "3D Beam and Spotlight", false, 201)]
            static void Menu_CreateSpotLightAndBeam(MenuCommand menuCommand) { EditorExtensions.OnNewGameObjectCreated(EditorExtensions.HD.NewSpotLightAndBeam(), menuCommand); }

            [MenuItem(kNewBeamPrefix + "2D Beam", false, 202)]
            static void Menu_CreateNewBeam2D(MenuCommand menuCommand) { EditorExtensions.OnNewGameObjectCreated(EditorExtensions.HD.NewBeam2D(), menuCommand); }
        }

        static class LOD
        {
            [MenuItem(kSceneMenuPrefix + "LOD Beam Group", false, 300)]
            static void Menu_CreateNewLODBeamGroup(MenuCommand menuCommand) { EditorExtensions.OnNewGameObjectCreated(EditorExtensions.LOD.NewLODBeamGroup(), menuCommand); }
        }



        const string kAddVolumetricBeam = "CONTEXT/Light/\U0001F4A1 Attach a Volumetric Beam ";
        static bool CanAddVolumetricBeam(Light light) { return !Application.isPlaying && light != null && light.type == LightType.Spot && light.GetComponent<VolumetricLightBeamAbstractBase>() == null; }

        static T Menu_AttachBeam_Command<T>(MenuCommand menuCommand) where T : VolumetricLightBeamAbstractBase
        {
            var light = menuCommand.context as Light;
            T comp = null;
            if (CanAddVolumetricBeam(light))
                comp = Undo.AddComponent<T>(light.gameObject);
            return comp;
        }

        [MenuItem(kAddVolumetricBeam + "SD", false)]
        static void Menu_AttachBeamSD_Command(MenuCommand menuCommand) { Menu_AttachBeam_Command<VolumetricLightBeamSD>(menuCommand); }

        [MenuItem(kAddVolumetricBeam + "HD", false)]
        static void Menu_AttachBeamHD_Command(MenuCommand menuCommand) {
            var beamHD = Menu_AttachBeam_Command<VolumetricLightBeamHD>(menuCommand);
            if (beamHD) beamHD.scalable = false;
        }

        [MenuItem(kAddVolumetricBeam + "SD", true)]
        [MenuItem(kAddVolumetricBeam + "HD", true)]
        static bool Menu_AttachBeam_Validate() { return CanAddVolumetricBeam(GetActiveLight()); }


        const int kMenuItemPriorityBase = 1000;

        /////////////////////////////
        // DOCUMENTATION
        /////////////////////////////
        const int kMenuItemPriorityDocumentation = kMenuItemPriorityBase + 1;
        const string kDocumentationSuffix = "/\u2754 Documentation";

        [MenuItem("CONTEXT/" + VolumetricLightBeamSD.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_BeamSD_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.SD.UrlBeam); }

        [MenuItem("CONTEXT/" + VolumetricLightBeamHD.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_BeamHD_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.HD.UrlBeam); }

        [MenuItem("CONTEXT/" + VolumetricDustParticles.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_DustParticles_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlDustParticles); }

        [MenuItem("CONTEXT/" + DynamicOcclusionRaycasting.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_DynamicOcclusionRaycasting_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.SD.UrlDynamicOcclusionRaycasting); }

        [MenuItem("CONTEXT/" + DynamicOcclusionDepthBuffer.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_DynamicOcclusionDepthBuffer_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.SD.UrlDynamicOcclusionDepthBuffer); }

        [MenuItem("CONTEXT/" + SkewingHandleSD.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_SkewingHandle_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.SD.UrlSkewingHandle); }

        [MenuItem("CONTEXT/" + TriggerZone.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_TriggerZone_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlTriggerZone); }

        [MenuItem("CONTEXT/" + EffectFlicker.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_EffectFlicker_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlEffectFlicker); }

        [MenuItem("CONTEXT/" + EffectPulse.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_EffectPulse_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlEffectPulse); }

        [MenuItem("CONTEXT/" + EffectFromProfile.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_EffectFromProfile_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlEffectPulse); }

        [MenuItem("CONTEXT/" + VolumetricCookieHD.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_CookieHD_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.HD.UrlCookie); }

        [MenuItem("CONTEXT/" + VolumetricShadowHD.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_ShadowHD_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.HD.UrlShadow); }

        [MenuItem("CONTEXT/" + TrackRealtimeChangesOnLightHD.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_TrackRealtimeChangesOnLight_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.HD.UrlTrackRealtimeChangesOnLight); }

        [MenuItem("CONTEXT/" + Config.ClassName + kDocumentationSuffix, false, kMenuItemPriorityDocumentation)]
        static void Menu_Config_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlConfig); }

        /////////////////////////////
        // GLOBAL CONFIG
        /////////////////////////////
        const int kMenuItemPriorityOpenConfig = kMenuItemPriorityBase + 2;
        const string kOpenConfigSuffix = "/\u2699 Open Global Config";

        [MenuItem("CONTEXT/" + VolumetricLightBeamAbstractBase.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + VolumetricDustParticles.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + DynamicOcclusionAbstractBase.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + SkewingHandleSD.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + TriggerZone.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + EffectAbstractBase.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + EffectFromProfile.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + VolumetricCookieHD.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + VolumetricShadowHD.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        [MenuItem("CONTEXT/" + TrackRealtimeChangesOnLightHD.ClassName + kOpenConfigSuffix, false, kMenuItemPriorityOpenConfig)]
        public static void Menu_Beam_Config(MenuCommand menuCommand) { Config.EditorSelectInstance(); }

        /////////////////////////////
        // ADDITIONAL COMPONENTS
        /////////////////////////////
        const string kAddParticlesSD = "CONTEXT/" + VolumetricLightBeamSD.ClassName + "/Add Dust Particles";
        [MenuItem(kAddParticlesSD, false)] static void Menu_AddDustParticles_CommandSD(MenuCommand menuCommand) => Menu_AddDustParticles_Command_Common(menuCommand);
        [MenuItem(kAddParticlesSD, true)] static bool Menu_AddDustParticles_ValidateSD() => Menu_AddDustParticles_Validate_Common();

        const string kAddParticlesHD = "CONTEXT/" + VolumetricLightBeamHD.ClassName + "/Add Dust Particles";
        [MenuItem(kAddParticlesHD, false)] static void Menu_AddDustParticles_CommandHD(MenuCommand menuCommand) => Menu_AddDustParticles_Command_Common(menuCommand);
        [MenuItem(kAddParticlesHD, true)] static bool Menu_AddDustParticles_ValidateHD() => Menu_AddDustParticles_Validate_Common();

        static void Menu_AddDustParticles_Command_Common(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<VolumetricDustParticles>(menuCommand.context as VolumetricLightBeamAbstractBase); }
        static bool Menu_AddDustParticles_Validate_Common() { return EditorExtensions.CanAddComponentFromEditor<VolumetricDustParticles>(GetActiveVolumetricLightBeam()); }

        const string kAddDynamicOcclusionRaycasting = "CONTEXT/" + VolumetricLightBeamSD.ClassName + "/Add Dynamic Occlusion (Raycasting)";
        [MenuItem(kAddDynamicOcclusionRaycasting, false)] static void Menu_AddDynamicOcclusionRaycasting_Command(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<DynamicOcclusionRaycasting>(menuCommand.context as VolumetricLightBeamSD); }
        [MenuItem(kAddDynamicOcclusionRaycasting, true)] static bool Menu_AddDynamicOcclusionRaycasting_Validate() { return Config.Instance.featureEnabledDynamicOcclusion && EditorExtensions.CanAddComponentFromEditor<DynamicOcclusionAbstractBase>(GetActiveVolumetricLightBeam()); }

        const string kAddDynamicOcclusionDepthBuffer = "CONTEXT/" + VolumetricLightBeamSD.ClassName + "/Add Dynamic Occlusion (Depth Buffer)";
        [MenuItem(kAddDynamicOcclusionDepthBuffer, false)] static void Menu_AddDynamicOcclusionDepthBuffer_Command(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<DynamicOcclusionDepthBuffer>(menuCommand.context as VolumetricLightBeamSD); }
        [MenuItem(kAddDynamicOcclusionDepthBuffer, true)] static bool Menu_AddDynamicOcclusionDepthBuffer_Validate() { return Config.Instance.featureEnabledDynamicOcclusion && EditorExtensions.CanAddComponentFromEditor<DynamicOcclusionAbstractBase>(GetActiveVolumetricLightBeam()); }

        const string kAddTriggerZoneSD = "CONTEXT/" + VolumetricLightBeamSD.ClassName + "/Add Trigger Zone";
        [MenuItem(kAddTriggerZoneSD, false)] static void Menu_AddTriggerZone_CommandSD(MenuCommand menuCommand) => Menu_AddTriggerZone_Command_Common(menuCommand);
        [MenuItem(kAddTriggerZoneSD, true)] static bool Menu_AddTriggerZone_ValidateSD() => Menu_AddTriggerZone_Validate_Common();

        const string kAddTriggerZoneHD = "CONTEXT/" + VolumetricLightBeamHD.ClassName + "/Add Trigger Zone";
        [MenuItem(kAddTriggerZoneHD, false)] static void Menu_AddTriggerZone_CommandHD(MenuCommand menuCommand) => Menu_AddTriggerZone_Command_Common(menuCommand);
        [MenuItem(kAddTriggerZoneHD, true)] static bool Menu_AddTriggerZone_ValidateHD() => Menu_AddTriggerZone_Validate_Common();

        static void Menu_AddTriggerZone_Command_Common(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<TriggerZone>(menuCommand.context as VolumetricLightBeamAbstractBase); }
        static bool Menu_AddTriggerZone_Validate_Common() { return EditorExtensions.CanAddComponentFromEditor<TriggerZone>(GetActiveVolumetricLightBeam()); }

        const string kAddEffectFlickerSD = "CONTEXT/" + VolumetricLightBeamSD.ClassName + "/Add Effect Flicker";
        [MenuItem(kAddEffectFlickerSD, false)] static void Menu_EffectFlicker_CommandSD(MenuCommand menuCommand) => Menu_EffectFlicker_Command_Common(menuCommand);
        [MenuItem(kAddEffectFlickerSD, true)] static bool Menu_EffectFlicker_ValidateSD() => Menu_Effect_Validate_Common();

        const string kAddEffectFlickerHD = "CONTEXT/" + VolumetricLightBeamHD.ClassName + "/Add Effect Flicker";
        [MenuItem(kAddEffectFlickerHD, false)] static void Menu_EffectFlicker_CommandHD(MenuCommand menuCommand) => Menu_EffectFlicker_Command_Common(menuCommand);
        [MenuItem(kAddEffectFlickerHD, true)] static bool Menu_EffectFlicker_ValidateHD() => Menu_Effect_Validate_Common();

        const string kAddEffectFromProfileSD = "CONTEXT/" + VolumetricLightBeamSD.ClassName + "/Add Effect from Profile";
        [MenuItem(kAddEffectFromProfileSD, false)] static void Menu_EffectFromProfile_CommandSD(MenuCommand menuCommand) => Menu_EffectFromProfile_Command_Common(menuCommand);
        [MenuItem(kAddEffectFromProfileSD, true)] static bool Menu_EffectFromProfile_ValidateSD() => Menu_Effect_Validate_Common();

        static void Menu_EffectFlicker_Command_Common(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<EffectFlicker>(menuCommand.context as VolumetricLightBeamAbstractBase); }

        const string kAddEffectPulseSD = "CONTEXT/" + VolumetricLightBeamSD.ClassName + "/Add Effect Pulse";
        [MenuItem(kAddEffectPulseSD, false)] static void Menu_EffectPulse_CommandSD(MenuCommand menuCommand) => Menu_EffectPulse_Command_Common(menuCommand);
        [MenuItem(kAddEffectPulseSD, true)] static bool Menu_EffectPulse_ValidateSD() => Menu_Effect_Validate_Common();

        const string kAddEffectPulseHD = "CONTEXT/" + VolumetricLightBeamHD.ClassName + "/Add Effect Pulse";
        [MenuItem(kAddEffectPulseHD, false)] static void Menu_EffectPulse_CommandHD(MenuCommand menuCommand) => Menu_EffectPulse_Command_Common(menuCommand);
        [MenuItem(kAddEffectPulseHD, true)] static bool Menu_EffectPulse_ValidateHD() => Menu_Effect_Validate_Common();

        const string kAddEffectFromProfileHD = "CONTEXT/" + VolumetricLightBeamHD.ClassName + "/Add Effect from Profile";
        [MenuItem(kAddEffectFromProfileHD, false)] static void Menu_EffectFromProfile_CommandHD(MenuCommand menuCommand) => Menu_EffectFromProfile_Command_Common(menuCommand);
        [MenuItem(kAddEffectFromProfileHD, true)] static bool Menu_EffectFromProfile_ValidateHD() => Menu_Effect_Validate_Common();

        static void Menu_EffectPulse_Command_Common(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<EffectPulse>(menuCommand.context as VolumetricLightBeamAbstractBase); }
        static void Menu_EffectFromProfile_Command_Common(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<EffectFromProfile>(menuCommand.context as VolumetricLightBeamAbstractBase); }

        static bool Menu_Effect_Validate_Common()
        {
            var activeBeam = GetActiveVolumetricLightBeam();
            return !Application.isPlaying
                && activeBeam != null
                && activeBeam.GetComponent<VolumetricLightBeamAbstractBase>() == null
                && activeBeam.GetComponent<EffectFromProfile>() == null
                ;
        }

        const string kAddShadow = "CONTEXT/" + VolumetricLightBeamHD.ClassName + "/Add Volumetric Shadow";
        [MenuItem(kAddShadow, false)] static void Menu_AddShadow_Command(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<VolumetricShadowHD>(menuCommand.context as VolumetricLightBeamHD); }
        [MenuItem(kAddShadow, true)] static bool Menu_AddShadow_Validate() { return Config.Instance.featureEnabledShadow && EditorExtensions.CanAddComponentFromEditor<VolumetricShadowHD>(GetActiveVolumetricLightBeam()); }

        const string kAddCookie = "CONTEXT/" + VolumetricLightBeamHD.ClassName + "/Add Volumetric Cookie";
        [MenuItem(kAddCookie, false)] static void Menu_AddCookie_Command(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<VolumetricCookieHD>(menuCommand.context as VolumetricLightBeamHD); }
        [MenuItem(kAddCookie, true)] static bool Menu_AddCookie_Validate() { return Config.Instance.featureEnabledCookie && EditorExtensions.CanAddComponentFromEditor<VolumetricCookieHD>(GetActiveVolumetricLightBeam()); }

        const string kAddTrackRealtime = "CONTEXT/" + VolumetricLightBeamHD.ClassName + "/Add Track Realtime Changes on Light";
        [MenuItem(kAddTrackRealtime, false)] static void Menu_AddTrackRealtime_Command(MenuCommand menuCommand) { EditorExtensions.AddComponentFromEditor<TrackRealtimeChangesOnLightHD>(menuCommand.context as VolumetricLightBeamHD); }
        [MenuItem(kAddTrackRealtime, true)] static bool Menu_AddTrackRealtime_Validate() { return EditorExtensions.CanAddComponentFromEditor<TrackRealtimeChangesOnLightHD>(GetActiveVolumetricLightBeam()) && GetActiveVolumetricLightBeam().GetComponent<Light>() != null; }

        static Light GetActiveLight() { return Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Light>() : null; }
        static VolumetricLightBeamAbstractBase GetActiveVolumetricLightBeam() { return Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<VolumetricLightBeamAbstractBase>() : null; }




        /////////////////////////////
        // EDIT MENU
        /////////////////////////////
        const string kEditMenu = "Edit/" + kCategoryName;

        [MenuItem(kEditMenu + "\u2699 Open Config", false, 20001)]
        static void Menu_EditOpenConfig() { Config.EditorSelectInstance(); }

#if UNITY_2019_3_OR_NEWER
        [MenuItem(kEditMenu + "Enable scene Pickability on all beams", false, 20101)]
        static void Menu_SetAllBeamsPickabilityEnabled() { SetAllBeamsPickability(true); }

        [MenuItem(kEditMenu + "Disable scene Pickability on all beams", false, 20102)]
        static void Menu_SetAllBeamsPickabilityDisable() { SetAllBeamsPickability(false); }

        [MenuItem(kEditMenu + "Enable scene Visibility on all beams", false, 20201)]
        static void Menu_SetAllBeamsVisibilityEnabled() { SetAllBeamsVisibility(true); }

        [MenuItem(kEditMenu + "Disable scene Visibility on all beams", false, 20202)]
        static void Menu_SetAllBeamsVisibilityDisable() { SetAllBeamsVisibility(false); }

        static void SetAllBeamsVisibility(bool enabled)
        {
            var beams = Resources.FindObjectsOfTypeAll<VolumetricLightBeamAbstractBase>();

            foreach (var beam in beams)
                beam.gameObject.SetSceneVisibilityState(enabled);
        }

        static void SetAllBeamsPickability(bool enabled)
        {
            var beams = Resources.FindObjectsOfTypeAll<VolumetricLightBeamAbstractBase>();

            foreach (var beam in beams)
                beam.gameObject.SetScenePickabilityState(enabled);
        }
#endif // UNITY_2019_3_OR_NEWER


        /////////////////////////////
        // PROJECT BROWSER
        /////////////////////////////
        const string kProjectBrowserMenuPrefix = "Assets/Create/" + kCategoryName;

        static string CurrentProjectFolderPath
        {
            get
            {
                var projectWindowUtilType = typeof(ProjectWindowUtil);
                MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
                if (getActiveFolderPath != null)
                {
                    object obj = getActiveFolderPath.Invoke(null, new object[0]);
                    if(obj != null)
                        return obj.ToString();
                }
                return "";
            }
        }

        [MenuItem(kProjectBrowserMenuPrefix + "Flicker Effect Profile", false, 1000)]
        public static void CreateEffectProfileFlicker() { CreateEffectProfile<EffectFlicker>("VLBEffectProfile_Flicker"); }

        [MenuItem(kProjectBrowserMenuPrefix + "Pulse Effect Profile", false, 1001)]
        public static void CreateEffectProfilePulse() { CreateEffectProfile<EffectPulse>("VLBEffectProfile_Pulse"); }

        static void CreateEffectProfile<T>(string name) where T : Component
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Can't create new prefab during playmode");
                return;
            }

            var currentProjectPath = CurrentProjectFolderPath;
            if(!currentProjectPath.StartsWith("Assets", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.LogErrorFormat("Can't create new asset under folder '{0}'", currentProjectPath);
                return;
            }

            var assetPath = System.IO.Path.Combine(currentProjectPath, name + ".prefab");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            var gao = new GameObject();
            gao.AddComponent<T>();

            bool result;
            PrefabUtility.SaveAsPrefabAsset(gao, assetPath, out result);

            if (result)
                Debug.LogFormat("Prefab '{0}' was saved successfully", assetPath);
            else
                Debug.LogFormat("Prefab '{0}' failed to save", assetPath);

            GameObject.DestroyImmediate(gao);
        }
    }
}
#endif

