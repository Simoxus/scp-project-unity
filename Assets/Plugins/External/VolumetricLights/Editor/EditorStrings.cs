#if UNITY_EDITOR
using UnityEngine;

namespace VLB
{
    public static class EditorStrings
    {
        public static class Common
        {
            public static readonly GUIContent ButtonOpenGlobalConfig = new GUIContent("Open Config asset in use");
            public const string HelpNoiseLoadingFailed = "Fail to load 3D noise texture. Please check your Config.";

            public static readonly string[] DimensionsEnumDescriptions = new string[]
            {
                "3D",
                "2D",
            };
        }

        public static class Beam
        {
            public static readonly GUIContent HeaderBasic = new GUIContent("Basic", "Basic beam's properties (color, thickness...)");
            public static readonly GUIContent HeaderShape = new GUIContent("Shape", "Tweak the shape of the cone (sport angle, range distance...)");
            public static readonly GUIContent HeaderAttenuation = new GUIContent("Fall-Off Attenuation", "Control the beam's range distance and the light fall-off behaviour");
            public static readonly GUIContent Header3DNoise = new GUIContent("3D Noise", "Simulate animated volumetric fog / mist / smoke effects.\nIt makes the volumetric lights look less 'perfect' and so much more realistic.\nTo achieve that, a tiled 3D noise texture is internally loaded and used by the beam shader.");
            public static readonly GUIContent HeaderBlendingDistances = new GUIContent("Soft Intersections Blending Distances", "Because the volumetric beams are rendered using cone geometry, it is possible that it intersects with the camera's near plane or with the world's geometry, which could produce unwanted artifacts.\nThese properties are designed to fix this issue.");
            public static readonly GUIContent HeaderGeometry = new GUIContent("Cone Geometry", "Control how the beam's geometry is generated.");
            public static readonly GUIContent HeaderFadeOut = new GUIContent("Fade Out");
            public static readonly GUIContent Header2D = new GUIContent("2D", "Tweak and combine the order when beams are rendered with 2D objects (such as 2D sprites)");
            public static readonly GUIContent HeaderInfos = new GUIContent("Infos");

            public const string FromSpotLight = "Get the value from the Light Spot";

            public static readonly GUIContent ColorMode = new GUIContent("Color", "Apply a flat/plain/single color, or a gradient.");
            public static readonly GUIContent ColorGradient = new GUIContent("", "Use the gradient editor to set color and alpha variations along the beam.");
            public static readonly GUIContent ColorFlat = new GUIContent("", "Use the color picker to set a plain RGBA color (takes account of the alpha value).");

            public const string IntensityMultiplier = "Multiplier to modulate the intensity from the attached Unity's spotlight.";

            public static readonly GUIContent HDRPExposureWeight = new GUIContent("Exposure Weight", "Use this slider to set how much effect the exposure has on the beam intensity.");

            public static readonly GUIContent BlendingMode = new GUIContent("Blending Mode", "Additive: highly recommended blending mode\nSoftAdditive: softer additive\nTraditional Transparency: support dark/black colors");
            public static readonly GUIContent ShaderAccuracy = new GUIContent("Shader Accuracy", "- Fast: a lot of computation are done on the vertex shader to maximize performance.\n- High: most of the computation are done on the pixel shader to maximize graphical quality at some performance cost.\n\nWe recommend to keep the default 'Fast' shader accuracy to ensure best performance, except when using the 'Tilt Factor' feature or when using a very rich 'Gradient Color'.");

            public static readonly GUIContent SpotAngle = new GUIContent("Spot Angle", "Define the angle (in degrees) at the base of the beam's cone");
            public const string SpotAngleMultiplier = "Multiplier to modulate the angle from the attached Unity's spotlight.";

            public static readonly GUIContent GlareFrontal = new GUIContent("Glare (frontal)", "Boost intensity factor when looking at the beam from the inside directly at the source.");
            public static readonly GUIContent GlareBehind = new GUIContent("Glare (from behind)", "Boost intensity factor when looking at the beam from behind.");

            public static readonly GUIContent TrackChanges = new GUIContent(
                " Track changes during Playtime",
                "Check this box to be able to modify properties during Playtime via Script, Animator and/or Timeline.\nEnabling this feature is at very minor performance cost. So keep it disabled if you don't plan to modify this light beam during playtime.");

            public static readonly GUIContent FallOffStart = new GUIContent("Attenuation Start Distance", "Distance from the light source (in units) the beam intensity will start to fall-off.");
            public static readonly GUIContent FallOffEnd = new GUIContent("Light Range Max Distance", "Distance from the light source (in units) the beam is entirely faded out.");
            public const string FallOffEndMultiplier = "Distance multiplier to modulate the range from the attached Unity's spotlight.";

            public static readonly GUIContent NoiseMode = new GUIContent("Enabled", "Enable 3D Noise effect and choose the mode:\n- World Space: the noise will look 'grounded' in the world\n- Local Space: the noise will look 'tied' to the beam");
            public static readonly GUIContent NoiseIntensity = new GUIContent("Intensity", "Higher intensity means the noise contribution is stronger and more visible");
            public static readonly GUIContent NoiseScale = new GUIContent("Scale", "3D Noise texture scaling: higher scale make the noise more visible, but potentially less realistic");
            public static readonly GUIContent NoiseVelocity = new GUIContent("Velocity", "Direction and speed of the noise scrolling, simulating the fog/smoke movement");
            public static readonly string[] NoiseModeEnumDescriptions = new string[]
            {
                "Disabled",
                "Enabled (World Space)",
                "Enabled (Local Space)",
            };

            public static readonly GUIContent CameraClippingDistance = new GUIContent("Camera", "Distance from the camera the beam will fade with.\n- 0.0: hard intersection\n- Higher values produce soft intersection when the camera is near the cone triangles.\n\nThis distance is NOT scaled according to the beam's GameObject scale. You'll have to set a consistent distance value when scaling your beams, specially with very tiny or very large scaling factors.");
            public static readonly GUIContent DepthBlendDistance = new GUIContent("Opaque geometry", "Distance from the world geometry the beam will fade with.\n- 0.0 (feature disabled): hard intersection but faster (doesn't require to update the depth texture).\n- Higher values produce soft intersection when the beam intersects world's geometry, but require to update the camera's depth texture.\n\nThis distance is NOT scaled according to the beam's GameObject scale. You'll have to set a consistent distance value when scaling your beams, specially with very tiny or very large scaling factors.");

            public static readonly GUIContent ConeRadiusStart = new GUIContent("Light Source Radius", "Radius (in units) at the beam's source (the top of the cone).\n0 will generate a perfect cone geometry.\nHigher values will generate truncated cones.");

            public static readonly GUIContent GeomMeshType = new GUIContent("Mesh Type", "- Shared: Use the global shared mesh (recommended setting, since it will save a lot of memory). Will use the geometry properties set on configuration file.\n- Custom: Use a custom mesh instead. Will use the geometry properties set on the beam.");
            public static readonly GUIContent GeomCap = new GUIContent("Cap", "Show Cap Geometry (only visible from inside)");
            public static readonly GUIContent GeomSides = new GUIContent("Sides", "Number of Sides of the cone.\nHigher values make the beam looks more 'round', but require more memory and graphic performance.\nA recommended value for a decent quality while keeping the poly count low is 18.");
            public static readonly GUIContent GeomSegments = new GUIContent("Segments", "Number of Segments of the cone.\nHigher values give better looking results but require more performance. We recommend at least 3 segments, specially regarding Attenuation and Gradient, otherwise the approximation could become inaccurate.\nThe longer the beam, the more segments we recommend to set.\nA recommended value is 4.");

            public static readonly GUIContent Scalable = new GUIContent("Is Scalable?", "The scaling of the beam's GameObject (or any of its parent in the hierarchy)...\n-True: ...will be applied to the beam itself and will change its size.\n-False: ...won't be applied to the beam itself, so the beam won't have its size changed.\n\nIn short, we recommend to set the Scalable property at:\n-True when there is no Unity Light attached to the same GameObject, so you will be able to scale your beam easily.\n- False when there is a Unity Light attached to the same GameObject, because the Unity Light are NOT scalable: this way your beam will always fit the Unity Light size.");

            public static readonly GUIContent FadeOutEnabled = new GUIContent("Enabled", "Enable the fade out of the beam according to the distance to the camera.");
            public static readonly GUIContent FadeOutBegin = new GUIContent("Begin Distance", "Fade out starting distance. Beyond this distance, the beam intensity will start to be dimmed.");
            public static readonly GUIContent FadeOutEnd = new GUIContent("End", "Fade out ending distance. Beyond this distance, the beam will be culled off to save on performance.");

            public static readonly GUIContent Dimensions = new GUIContent("Dimensions", "- 3D: beam along the Z axis.\n- 2D: beam along the X axis, so you won't have to rotate it to see it in 2D.");
            public const string SortingLayer = "Sorting Layer";
            public static readonly GUIContent SortingOrder = new GUIContent("Order", "The overlay priority within its layer. Lower numbers are rendered first and subsequent numbers overlay those below.");

            // BUTTONS
            public static readonly GUIContent ButtonCreateOverridePerPlatform = new GUIContent("Create another Config asset for a specific platform", "");
            public static readonly GUIContent ButtonResetProperties = new GUIContent("Default values", "Reset properties to their default values.");
            public static readonly GUIContent ButtonGenerateGeometry = new GUIContent("Regenerate geometry", "Force to re-create the Beam Geometry GameObject.");
            public static readonly GUIContent ButtonEditInScene = new GUIContent("Edit in Scene View", "Edit some beam's properties directly in the Scene View using convenient gizmos.");
            public const string ButtonAddDustParticles = "+ Volumetric Dust Particles\nAdd highly detailed dustlight/mote particles on your beam.";
            public const string ButtonAddDynamicOcclusion = "+ Dynamic Occlusion\nGives awareness to your beam so it reacts to changes in the world: it could be occluded by environment geometry.";
            public const string ButtonAddDynamicOcclusionRaycasting = "+ Dynamic Occlusion (Raycasting)";
            public const string ButtonAddDynamicOcclusionDepthBuffer = "+ Dynamic Occlusion (Depth Buffer)";
            public const string ButtonAddTriggerZone = "+ Trigger Zone\nTrack objects passing through the light beam and track when the beam is passing over them.";
            public const string ButtonAddEffect = "+ Effect\nAdd runtime FX to easily animate your light and/or beam with effects such as strobe/flickering and pulsing/flashing.";
            public const string ButtonAddEffectFlicker = "+ Effect Flicker";
            public const string ButtonAddEffectPulse = "+ Effect Pulse";
            public const string ButtonAddEffectFromProfile = "+ Effect from Profile";

            // HELP BOXES
            public const string HelpNoSpotlight = "To bind properties from the Light and the Beam together, this component must be attached to a Light of type 'Spot'";
            public static string HelpFadeOutNoMainCamera { get { return string.Format("Fail to retrieve the camera with tag '{0}' (specified in VLB Config's 'fadeOutCameraTag') for the FadeOut feature.", VLB.Config.Instance.fadeOutCameraTag); } }

            public const string PrefEditInScene = "VLB_BEAM_EDITINSCENE";

            public static class SD
            {
                public static readonly GUIContent IntensityModeAdvanced = new GUIContent("Adv", "Advanced Mode: control inside and outside intensity values independently.");
                public static readonly GUIContent IntensityGlobal = new GUIContent("Intensity", "Global beam intensity. If you want to control values for inside and outside independently, use the advanced mode.");
                public static readonly GUIContent IntensityOutside = new GUIContent("Intensity (outside)", "Beam outside intensity (when looking at the beam from behind).");
                public static readonly GUIContent IntensityInside = new GUIContent("Intensity (inside)", "Beam inside intensity (when looking at the beam from the inside directly at the source).");

                public static readonly GUIContent SideThickness = new GUIContent("Side Thickness", "Thickness of the beam when looking at it from the side.\n1 = the beam is fully visible (no difference between the center and the edges), but produces hard edges.\nLower values produce softer transition at beam edges.");

                public static readonly GUIContent AttenuationEquation = new GUIContent("Attenuation Equation", "Attenuation equation used to compute fading between 'Attenuation Start Distance' and 'Light Range Max Distance'.\n- Linear: Simple linear attenuation\n- Quadratic: Quadratic attenuation, which usually gives more realistic results\n- Blend: Custom blending mix between linear (0.0) and quadratic attenuation (1.0)");
                public static readonly GUIContent AttenuationCustomBlending = new GUIContent("", "Blending value between Linear (0.0) and Quadratic (1.0) attenuation equations.");

                public static readonly GUIContent SkewingLocalForwardDirection = new GUIContent("Skewing Factor", "Distort the shape of the beam horizontally and vertically while keeping its circular slice unchanged.");
                public static readonly GUIContent ClippingPlane = new GUIContent("Clipping Plane", "Additional clipping plane transform.  Will cut the beam according to the GameObject's position and rotation.");
                public static readonly GUIContent EditorShowClippingPlane = new GUIContent("Debug", "Display the additional clipping plane.");

                public static readonly GUIContent TiltFactor = new GUIContent("Tilt Factor", "Tilt the color and attenuation gradient compared to the global beam's direction.\nCan only be used with 'High' shader accuracy mode.");
                public static readonly GUIContent EditorShowTiltDirection = new GUIContent("Debug", "Display the direction of the tilt factor in editor.");

                public const string HelpDepthMobile = "On mobile platforms, the depth buffer precision can be pretty low. Try to keep a small depth range on your cameras: the difference between the near and far clip planes should stay as low as possible.";
                public const string HelpTrackChangesEnabled = "This beam will keep track of the changes of its own properties and the spotlight attached to it (if any) during playtime. You can modify every properties except 'geomSides'.";
                public const string HelpAnimatorWarning = "If you want to animate your light beam in real-time, you should enable the 'Track Changes During Playtime' property.";
                public const string HelpTiltedWithShaderAccuracyFast = "We highly recommend to set the 'Shader Accuracy' property to 'High' when using 'Tilt Factor'.";
                public const string PrefShowAddClippingPlane = "VLB_BEAM_SHOWADDCLIPPINGPLANE";
            }

            public static class HD
            {
                public static readonly GUIContent HeaderRaymarching = new GUIContent("Raymarching", "");
                public static readonly GUIContent HeaderCookie = new GUIContent("Cookie", "");

                public static readonly GUIContent SideSoftness = new GUIContent("Side Softness", "Smoothness of the beam when looking at it from the side, controlling how soft it looks on the edges.");
                public static readonly GUIContent Intensity = new GUIContent("Intensity", "Controls the brightness of the beam.");

                public static readonly GUIContent AttenuationEquation = new GUIContent("Attenuation Equation", "Attenuation equation used to compute fading between 'Attenuation Start Distance' and 'Light Range Max Distance'.\n- Linear: Simple linear attenuation\n- Quadratic: Quadratic attenuation, which usually gives more realistic results");

                public static readonly GUIContent RaymarchingQuality = new GUIContent("Quality", "Specify which raymarching quality you want to use for this beam (the higher the steps, the better quality, the slower the performances).\n\nThe different qualities names and steps count can be configured from plugin's config.");

                public static readonly GUIContent JitteringFactor = new GUIContent("Jittering Factor", "When using Shadow or Cookie with a raymarching quality with too low steps count, some banding artifacts can appear.\nIn this case, increase jittering to add noise to smooth raymarching inaccuracy.");
                public static readonly GUIContent JitteringFrameRate = new GUIContent("Jittering Frame Rate", "Animate the jittering noise texture over the time:\n0 = the jittering won't be animated.\nhigher values = the jittering will be animated more often.");
                public static readonly GUIContent JitteringLerpRange = new GUIContent("Jittering Lerp Range", "Configure where the jittering will be visible along the beam. This range is specified between 0 (the tip of the beam) and 1 (the end of the beam):\n- before the range: no jittering\n- in the range: jittering will lerp from 0 to 'jittering factor' value\n- after the range: 'jittering factor' value");

                public const string TipJittering = "When using this feature, we highly recommend to increase the 'Jittering Factor' property on your HD beam.\nAdding jittering allows to have a decent rendering quality by smoothing banding artifact while keeping raymarching steps as low as possible(and so good performance).";


                public const string ButtonAddShadow = "+ Volumetric Shadow\nGives awareness to your beam so it reacts to changes in the world: it could be occluded by environment geometry.";
                public const string ButtonAddCookie = "+ Volumetric Cookie\nSpecify a cookie texture to the beam, which is a mask to simulate a shadow with a specific shape or color.";

                public const string ButtonAddTrackRealtimeChangesOnLight = "+Track Realtime Changes On Light\nAdd this script be able to modify properties on the Unity's Light component during Playtime via Script, Animator and/or Timeline.\nEnabling this feature is at very minor performance cost. So keep it disabled if you don't plan to modify the Unity's Light component during playtime.";
                public const string TipTrackRealtimeChangesOnLight = "Realtime changes on the Unity's Light component properties (via Script, Animator and/or Timeline) will be taken into account.";
            }
        }

        public static class DustParticles
        {
            public static readonly GUIContent HeaderRendering = new GUIContent("Rendering");
            public static readonly GUIContent HeaderDirectionAndVelocity = new GUIContent("Direction & Velocity");
            public static readonly GUIContent HeaderCulling = new GUIContent("Culling");
            public static readonly GUIContent HeaderSpawning = new GUIContent("Spawning");
            public static readonly GUIContent HeaderInfos = new GUIContent("Infos");

            public static readonly GUIContent Alpha = new GUIContent("Alpha", "Max alpha of the particles");
            public static readonly GUIContent Size = new GUIContent("Size", "Max size of the particles");

            public static readonly GUIContent Direction = new GUIContent("Direction", "Direction of the particles\n- Random: random direction.\n- Local Space: particles follow the velicity direction in local space (Z is along the beam).\n- World Space: particles follow the velicity direction in world space.");
            public static readonly GUIContent Velocity = new GUIContent("Velocity", "Movement speed of the particles along the chosen direction");

            public static readonly GUIContent CullingEnabled = new GUIContent("Enabled", "Enable particles culling based on the distance with the 'FadeOut Camera' identified through the VLB Config's 'fadeOutCameraTag' property.\nWe highly recommend to enable this feature to keep good runtime performances.");
            public static readonly GUIContent CullingMaxDistance = new GUIContent("Max Distance", "The particles will not be rendered if they are further than cullingMaxDistance to the 'FadeOut Camera'.");

            public static readonly GUIContent Density = new GUIContent("Density", "Control how many particles are spawned. The higher the density, the more particles are spawned, the higher the performance cost is");
            public static readonly GUIContent SpawnDistanceRange = new GUIContent("Distance Range", "The distance range (from the light source) where the particles are spawned.\n- Min bound: the higher it is, the more the particles are spawned away from the light source.\n- Max bound: the lower it is, the more the particles are gathered near the light source.");

            // HELP BOXES
            public const string HelpFailToInstantiate = "Fail to instantiate the Particles. Please check your Config.";
            public const string HelpRecommendation = "We do not recommend to use this feature if you plan to move or change properties of the beam during playtime.";
        }

        public static class DynOcclusion
        {
            public static readonly GUIContent HeaderUpdateRate = new GUIContent("Update Rate");
            public static readonly GUIContent HeaderRaycasting = new GUIContent("Raycasting");
            public static readonly GUIContent HeaderOccluderSurface = new GUIContent("Occluder Surface");
            public static readonly GUIContent HeaderClippingPlane = new GUIContent("Clipping Plane");
            public static readonly GUIContent HeaderCamera = new GUIContent("Camera");
            public static readonly GUIContent HeaderEditorDebug = new GUIContent("Editor Debug");

            public static readonly GUIContent Dimensions = new GUIContent("Dimensions", "Should it interact with 2D or 3D occluders?\n- 3D: the beam will react against 3D Occluders.\n- 2D: the beam will react against 2D Occluders. This is useful when using the beams with 2D objects (such as 2D Sprites).");

            public static readonly GUIContent LayerMask = new GUIContent("Layer Mask",
                "The beam can only be occluded by objects located on the layers matching this mask.\nIt's very important to set it as restrictive as possible (checking only the layers which are necessary) to perform a more efficient process in order to increase the performance.");
            public static readonly GUIContent ConsiderTriggers = new GUIContent("Consider Triggers",
                "Should this beam be occluded by triggers or not?");
            public const string ConsiderTriggersNoPossible = "In order to be able to consider triggers as 2D occluders, you should tick the 'Queries Hit Triggers' checkbox under the 'Physics 2D' settings menu.";
            public static readonly GUIContent MinOccluderArea = new GUIContent("Min Occluder Area",
                "Minimum 'area' of the collider to become an occluder.\nColliders smaller than this value will not block the beam.");
            public static readonly GUIContent UpdateRate = new GUIContent("Update Rate", "How often will the occlusion be processed?\nTry to update the occlusion as rarely as possible to keep good performance.");
            public static readonly string[] UpdateRateDescriptions = new string[]
            {
                "Never",
                "On Enable (only once)",
                "On Beam Move",
                "Every X Frames",
                "On Beam Move and Every X Frames",
            };

            public static readonly GUIContent WaitXFrames = new GUIContent("X frames to wait",
                "How many frames we wait between 2 occlusion tests?\nIf you want your beam to be super responsive to the changes of your environment, update it every frame by setting 1.\nIf you want to save on performance, we recommend to wait few frames between each update by setting a higher value.");
            public static readonly GUIContent MinSurfaceRatio = new GUIContent("Min Occluded %", "Approximated percentage of the beam to collide with the surface in order to be considered as occluder.");
            public static readonly GUIContent MaxSurfaceDot = new GUIContent("Max Angle", "Max angle (in degrees) between the beam and the surface in order to be considered as occluder.");
            public static readonly GUIContent PlaneAlignment = new GUIContent("Alignment", "Alignment of the computed clipping plane:\n- Surface: align to the surface normal which blocks the beam. Works better for large occluders such as floors and walls.\n- Beam: keep the plane aligned with the beam direction. Works better with more complex occluders or with corners.");
            public static readonly GUIContent PlaneOffset = new GUIContent("Offset Units", "Translate the plane. We recommend to set a small positive offset in order to handle non-flat surface better.");
            public static readonly GUIContent FadeDistanceToSurface = new GUIContent("Fade Distance Units", "Fade out the beam before the occlusion surface in order to soften the transition.");
            public static readonly GUIContent EditorShowDebugPlane = new GUIContent("Show Debug Plane", "Draw debug plane on the scene view.");
            public static readonly GUIContent EditorRaycastAtEachFrame = new GUIContent("Update in Editor", "Perform occlusion tests and raycasts in Editor.");

            public static string GetUpdateRateAdvice<T>(DynamicOcclusionUpdateRate value)
            {
                switch (value)
                {
                    case DynamicOcclusionUpdateRate.Never: return string.Format("The occlusion will never be updated.\nThe only way to update it is to manually call '{0}.ProcessOcclusionManually()' from script whenever you need.", typeof(T).Name);
                    case DynamicOcclusionUpdateRate.OnEnable: return "The occlusion will only be updated once on start, and each time the beam is enabled/activated (after being disabled/deactivated).\nIt's suitable for static beams located in static environment.";
                    case DynamicOcclusionUpdateRate.OnBeamMove: return "The occlusion will only be updated when the beam will move.\nIt's suitable for moving beams located in static environment.";
                    case DynamicOcclusionUpdateRate.EveryXFrames: return "The occlusion will be updated every {0} frame(s).\nIt's suitable for static beams located in moving environment.";
                    case DynamicOcclusionUpdateRate.OnBeamMoveAndEveryXFrames: return "The occlusion will be updated when the beam will move in addition to every {0} frame(s).\nIt's suitable for moving beams located in moving environment.";
                    default: return null;
                }
            }

            public static readonly GUIContent DepthBufferOcclusionCulling = new GUIContent("Occlusion Culling", "Whether or not the virtual camera will use occlusion culling during rendering from the beam's POV.");
            public static readonly GUIContent DepthBufferDepthMapResolution = new GUIContent("Depth Map Resolution", "Controls how large the depth texture captured by the virtual camera is.\nThe lower the resolution, the better the performance, but the less accurate the rendering.");

            public const string HelpDepthBufferAndBeam2D = "'Dynamic Occlusion (Depth Buffer)' doesn't work with 2D sprites nor 2D colliders. It will only track 3D objects.";

            public const string HelpOverrideLayer = "To keep good performance, it's highly recommended to set an 'Override Layer' in the Config when using this feature, to prevent from having a LayerMark including any Volumetric Beam.";
            public static string HelpLayerMaskIssues { get { return string.Format("The beams are generated on the layer '{0}' (set in the Config), but this LayerMask includes this layer.\nTo keep good performance, it's highly recommended to set a LayerMask which doesn't include this layer!", UnityEngine.LayerMask.LayerToName(VLB.Config.Instance.geometryLayerID)); } }

            public const string HelpFeatureDisabled = "The Dynamic Occlusion features have been disabled in the plugin's Config.";
        }

        public static class TriggerZone
        {
            public static readonly GUIContent HeaderInfos = new GUIContent("Infos");

            public static readonly GUIContent SetIsTrigger = new GUIContent("Set Is Trigger", "Define if the Collider will be created as a convex trigger (not physical, most common behavior) or as a regular collider (physical).");
            public static readonly GUIContent RangeMultiplier = new GUIContent("Range Multiplier", "Change the length of the Collider.\nFor example, set 2.0 to make the Collider 2x longer than the beam.");

            public const string HelpDescription2D = "Generate a 2D Polygon Collider with the same shape than the beam, supporting dynamic occlusion.";
            public const string HelpDescription3D = "Generate a 3D Mesh Collider with the same shape than the beam. The collider doesn't support occlusion though.";
            public const string HelpTrackChangesDuringPlaytimeEnabled = "The TriggerZone collider cannot be changed in realtime.\nIf you animate a property which change the shape of the beam, the collider shape won't fit anymore.";
        }

        public static class SkewingHandle
        {
            public static readonly GUIContent Beam = new GUIContent("Beam", "The Volumetric Light Beam you want to modify.");
            public static readonly GUIContent ShouldUpdateEachFrame = new GUIContent("Should Update Each Frame", "Should the beam's skewing property be updated each frame or only once?");

            public const string ErrorAttachedToSelf = "You should attach the 'SkewingHandle' component to another GameObject than the beam itself.";
            public const string ErrorCannotSkew = "This beam can't be skewed because it doesn't use 'Custom' mesh type.";
            public const string ErrorCannotUpdate = "This beam can't be updated each frame since its property 'Track Changes During Playtime' is disabled.";

            public const string HelpFeatureDisabled = "The 'Mesh Skewing' feature has been disabled in the plugin's Config.";
        }


        public static class Shadow
        {
            public static readonly GUIContent HeaderUpdateRate = new GUIContent("Update Rate");
            public static readonly GUIContent HeaderVisual = new GUIContent("Visual");
            public static readonly GUIContent HeaderCamera = new GUIContent("Depth Camera");

            public static readonly GUIContent Strength = new GUIContent("Strength", "Controls how dark the shadow cast by this Light Beam will be.\nThe bigger the value, the more the shadow will affect the visual.");
            public static readonly GUIContent LayerMask = new GUIContent("Layer Mask",
                "The beam can only be occluded by objects located on the layers matching this mask.\nIt's very important to set it as restrictive as possible (checking only the layers which are necessary) to perform a more efficient process in order to increase the performance.");
            public static readonly GUIContent UpdateRate = new GUIContent("Update Rate", "How often will the occlusion be processed?\nTry to update the occlusion as rarely as possible to keep good performance.");
            public static readonly string[] UpdateRateDescriptions = new string[]
            {
                "Never",
                "On Enable (only once)",
                "On Beam Move",
                "Every X Frames",
                "On Beam Move and Every X Frames",
            };

            public static readonly GUIContent WaitXFrames = new GUIContent("X frames to wait",
                "How many frames we wait between 2 occlusion tests?\nIf you want your beam to be super responsive to the changes of your environment, update it every frame by setting 1.\nIf you want to save on performance, we recommend to wait few frames between each update by setting a higher value.");

            public static string GetUpdateRateAdvice<T>(ShadowUpdateRate value)
            {
                switch (value)
                {
                    case ShadowUpdateRate.Never: return string.Format("The occlusion will never be updated.\nThe only way to update it is to manually call '{0}.ProcessOcclusionManually()' from script whenever you need.", typeof(T).Name);
                    case ShadowUpdateRate.OnEnable: return "The occlusion will only be updated once on start, and each time the beam is enabled/activated (after being disabled/deactivated).\nIt's suitable for static beams located in static environment.";
                    case ShadowUpdateRate.OnBeamMove: return "The occlusion will only be updated when the beam will move.\nIt's suitable for moving beams located in static environment.";
                    case ShadowUpdateRate.EveryXFrames: return "The occlusion will be updated every {0} frame(s).\nIt's suitable for static beams located in moving environment.";
                    case ShadowUpdateRate.OnBeamMoveAndEveryXFrames: return "The occlusion will be updated when the beam will move in addition to every {0} frame(s).\nIt's suitable for moving beams located in moving environment.";
                    default: return null;
                }
            }

            public static readonly GUIContent OcclusionCulling = new GUIContent("Occlusion Culling", "Whether or not the virtual camera will use occlusion culling during rendering from the beam's POV.");
            public static readonly GUIContent DepthMapResolution = new GUIContent("Depth Map Resolution", "Controls how large the depth texture captured by the virtual camera is.\nThe lower the resolution, the better the performance, but the less accurate the rendering.");

            public const string HelpOverrideLayer = "To keep good performance, it's highly recommended to set an 'Override Layer' in the Config when using this feature, to prevent from having a LayerMark including any Volumetric Beam.";
            public static string HelpLayerMaskIssues { get { return string.Format("The beams are generated on the layer '{0}' (set in the Config), but this LayerMask includes this layer.\nTo keep good performance, it's highly recommended to set a LayerMask which doesn't include this layer!", UnityEngine.LayerMask.LayerToName(VLB.Config.Instance.geometryLayerID)); } }

            public const string HelpFeatureDisabled = "The Shadow feature has been disabled in the plugin's Config.";
        }

        public static class Cookie
        {
            public static readonly GUIContent HeaderVisual = new GUIContent("Visual");
            public static readonly GUIContent HeaderTexture = new GUIContent("Texture");
            public static readonly GUIContent HeaderTransform = new GUIContent("Transform");

            public static readonly GUIContent Contribution = new GUIContent("Contribution", "How much the cookie texture will contribute to the beam rendering.");
            public static readonly GUIContent CookieTexture = new GUIContent("Cookie Texture", "Specify the texture mask asset.\nIt can be a regular 'Cookie' texture or any other texture type.");
            public static readonly GUIContent Channel = new GUIContent("Texture Channel", "Which channel(s) will be used to render the cookie.\nSpecify a unique channel with a black & white texture to render a regular cookie.\nSpecify RGBA with a colored texture to simulate light passing through a stained glass.");
            public static readonly GUIContent Negative = new GUIContent("Negative", "- False: white/opaque value in chosen texture channel is visible.\n- True: white/opaque value in chosen texture channel is hidden.");

            public static readonly GUIContent Translation = new GUIContent("Translation", "2D local translation applied to the cookie texture.");
            public static readonly GUIContent Rotation = new GUIContent("Rotation", "Local rotation angle of the cookie texture (in degrees).");
            public static readonly GUIContent Scale = new GUIContent("Scale", "2D local scale applied to the cookie texture.");

            public const string TipCookieMipMaps = "The specified cookie texture generates mip maps.\nWe highly recommend to disable mip maps generation on it to prevent from having rendering artifacts around objects edges in front or inside volumetric light beams.";
        }

        public static class Config
        {
            public static readonly GUIContent HeaderBeamGeometry = new GUIContent("Beam Geometry", "");
            public static readonly GUIContent HeaderRendering = new GUIContent("Rendering", "");
            public static readonly GUIContent HeaderURPSpecific = new GUIContent("URP Specific Options", "");
            public static readonly GUIContent HeaderSharedMesh = new GUIContent("Shared Mesh", "");
            public static readonly GUIContent HeaderGlobal3DNoise = new GUIContent("Global 3D Noise", "");
            public static readonly GUIContent HeaderFadeOutCamera = new GUIContent("Camera to compute Fade Out", "");
            public static readonly GUIContent HeaderFeaturesEnabled = new GUIContent("Features Enabled", "Disable the features you don't use in your project to speed up export time, specially with URP and HDRP.");
            public static readonly GUIContent HeaderInternalData = new GUIContent("Internal Data (do not change)", "");

            public static readonly GUIContent GeometryOverrideLayer = new GUIContent("Override Layer", "- If enabled, we force the procedural beam geometry GameObjects to be created on the Layer specified here.\n- If disabled, the procedural beam geometry GameObjects inherit the Layer of their beam (the GameObject containing the 'VolumetricLightBeam' component).");
            public static readonly GUIContent GeometryTag = new GUIContent("Tag", "The tag applied on the procedural geometry GameObjects");
            public static readonly GUIContent GeometryRenderQueueSD = new GUIContent("Render Queue (SD beams)", "Determine in which order SD beams are rendered compared to other objects.\nThis way for example transparent objects are rendered after opaque objects, and so on.");
            public static readonly GUIContent GeometryRenderQueueHD = new GUIContent("Render Queue (HD beams)", "Determine in which order HD beams are rendered compared to other objects.\nThis way for example transparent objects are rendered after opaque objects, and so on.");
            public static readonly GUIContent GeometryRenderPipeline = new GUIContent("Render Pipeline", "Select the Render Pipeline (Built-In or SRP) in use.");
            public static readonly GUIContent GeometryRenderingMode = new GUIContent("Rendering Mode",
@"- Multi-Pass: Use the 2 pass shader for SD beams (will generate 2 drawcalls per beam) and the 1 pass shader for HD beams (will generate 1 drawcall per beam). Not compatible with Scriptable Render Pipelines such as HDRP and URP.
- Default: Use the 1 pass shader. Will generate 1 drawcall per beam.
- GPU Instancing: Dynamically batch multiple beams to combine and reduce draw calls.
- SRP Batcher: Use the SRP Batcher to automatically batch multiple beams and reduce draw calls. Only available when using SRP.");

            public static string GetErrorSrpAndMultiPassNotCompatible(ShaderMode shaderMode)    { return string.Format("Using a Scriptable Render Pipeline with 'Multi-Pass' Rendering Mode is not supported: please choose another Rendering Mode, or '{0}' will be used.", VLB.Config.Instance.GetActualRenderingMode(shaderMode)); }
            public static string GetErrorSrpBatcherOnlyCompatibleWithSrp(ShaderMode shaderMode) { return string.Format("The 'SRP Batcher' Rendering Mode is only compatible when using a SRP: please choose another Rendering Mode, or '{0}' will be used.", VLB.Config.Instance.GetActualRenderingMode(shaderMode)); }
            public const string ErrorRenderPipelineMismatch = "It looks like the 'Render Pipeline' is not correctly set.\nPlease make sure to select the proper value depending on your pipeline in use.";

            public static readonly GUIContent URPDepthCameraScriptableRendererIndex = new GUIContent("Custom renderer index for Depth Camera", "When using URP, specify a custom Renderer index used by the depth cameras for the 'Dynamic Occlusion (Depth Buffer)' and 'HD Shadow' features.\nThe 'Renderer list' is editable in the URP asset.\n\nWe recommend to specify a custom index referencing the URP default 'ForwardRenderer' when you are using a custom renderer that doesn't support writing to depth render texture. This is the case if you encounter errors like:\n- 'RenderTexture.Create failed: colorFormat & depthStencilFormat cannot both be none'\n- 'You can only call cameraColorTarget inside the scope of a ScriptableRenderPass'\n\nSet -1 to disable this feature.");

            public static readonly GUIContent FadeOutCameraTag = new GUIContent("Fade Out Camera Tag", "Tag used to retrieve the camera used to compute the fade out factor on SD Beams, and the culling of 'VolumetricDustParticles'.");

            public static readonly GUIContent SharedMeshSides = new GUIContent("Cone Mesh Sides", "Number of Sides of the cone.\nHigher values make the beam looks more 'round', but require more memory and graphic performance.\nA recommended value for a decent quality while keeping the poly count low is 18.");
            public static readonly GUIContent SharedMeshSegments = new GUIContent("Cone Mesh Segments", "Number of Segments of the cone.\nHigher values give better looking results but require more performance. We recommend at least 3 segments, specially regarding Attenuation and Gradient, otherwise the approximation could become inaccurate.\nThe longer the beam, the more segments we recommend to set.\nA recommended value is 4.");
            public static readonly GUIContent GlobalNoiseScale = new GUIContent("Scale", "Global 3D Noise texture scaling: higher scale make the noise more visible, but potentially less realistic");
            public static readonly GUIContent GlobalNoiseVelocity = new GUIContent("Velocity", "Global World Space direction and speed of the noise scrolling, simulating the fog/smoke movement");
            public static readonly GUIContent NoiseTexture3D = new GUIContent("3D Noise Texture", "3D Texture storing noise data.");
            public static readonly GUIContent DustParticlesPrefab = new GUIContent("Dust Particles Prefab", "ParticleSystem prefab instantiated for the Volumetric Dust Particles feature (Unity 5.5 or above)");
            public static readonly GUIContent DitheringFactor = new GUIContent("Screen Space Dithering", "Depending on the quality of your screen, you might see some artifacts with high contrast visual (like a white beam over a black background).\nThese is a very common problem known as color banding.\nTo help with this issue, the plugin offers a Dithering factor: it smooths the banding by introducing a subtle pattern of noise.");
            public static readonly GUIContent DitheringNoiseTexture = new GUIContent("Dithering Noise Texture", "Noise texture for dithering feature.");
            public static readonly GUIContent UseLightColorTemperature = new GUIContent("Use Light Color Temperature", "Contribution of the attached spotlight temperature to the final beam color.\nOnly useful when:\n- The beams is attached to a Unity spotlight.\n- The beams color is linked to the Unity Light color.\n- The Unity light uses 'color temperature mode' and is specified with 'Filter' and 'Temperature' properties.");
            public static readonly GUIContent OpenDocumentation = new GUIContent("Documentation", "Open the online documentation.");
            public static readonly GUIContent CopyDebugInfo = new GUIContent("Copy debug info", "Copy to the clipboard system information useful for debugging. Please send use this data when you encounter a problem.");
            public static readonly GUIContent ClearAssetStoreCache = new GUIContent("Clear Asset Store cache", "Delete the AssetStore download cache to force Unity to re-download the proper package.\nCan solve issues when changing Unity version or updating the asset.");
            public static readonly GUIContent ResetToDefaultButton = new GUIContent("Default values", "Reset properties to their default values.");
            public static readonly GUIContent ResetInternalDataButton = new GUIContent("Reset internal data", "Reset internal data to their default values.");

            public static readonly GUIContent FeatureEnabledColorGradient = new GUIContent("Color Gradient", "- Off: do not support having a gradient as color.\n- High Only: support gradient color only for devices with Shader Level = 35 or higher.\n- High and Low: support gradient color for all devices");
            public static readonly GUIContent FeatureEnabledNoise3D = new GUIContent("Noise 3D", "Support 'Noise 3D' feature or not.");

            public static readonly string[] GeometryRenderPipelineEnumDescriptions = new string[]
            {
                "Built-In",
                "URP",
                "HDRP",
            };

            public static string GetErrorInvalidShader() { return string.Format("Fail to generate shader asset. Please try to reset the Config asset or reinstall the plugin."); }

            public static readonly string InvalidPlatformOverride = string.Format(
                "This Config asset has an invalid name. It should be named either:\n- '{0}{1}' for the default config.\n- '{0}PlatformName{1}' for a config specific to a platform, e.g. '{0}Android{1}'."
                , VLB.Config.kAssetName
                , VLB.Config.kAssetNameExt
                );

            public static readonly string WrongAssetLocation = string.Format(
                "This Config asset is not the one in use, please make sure: it's directly located under a 'Resources' folder."
                );

            public static readonly string NotCurrentAssetInUse = string.Format(
                "This Config asset is not the one in use for current Target Platform '{0}'."
                , PlatformHelper.GetCurrentPlatformSuffix()
                );
            public static class SD
            {
                public static readonly GUIContent FeatureEnabledDepthBlend = new GUIContent("Soft Intersect w/ Opaque Geometry (SD only)", "Support having soft intersection when the beam intersects other opaque geometry or not.");
                public static readonly GUIContent FeatureEnabledDynamicOcclusion = new GUIContent("Dynamic Occlusion (SD only)", "Support 'Dynamic Occlusion' features or not.");
                public static readonly GUIContent FeatureEnabledMeshSkewing = new GUIContent("Mesh Skewing (SD only)", "Support 'Mesh Skewing' feature or not.");
                public static readonly GUIContent FeatureEnabledShaderAccuracyHigh = new GUIContent("High Shader Accuracy (SD only)", "Support 'Shader Accuracy' property set to 'High' or not.");
            }

            public static class HD
            {
                public static readonly GUIContent HDSpecific                    = new GUIContent("HD Specific Options", "");

                public static readonly GUIContent CameraBlendingDistance        = new GUIContent("Camera Blending Distance", "Distance from the camera the beam will fade with (HD only. For SD beams, this option can be configured per beam).\n- 0.0: hard intersection\n- Higher values produce soft intersection when the camera is near the cone triangles.");
                public static readonly GUIContent JitteringNoiseTexture         = new GUIContent("Jittering Noise Texture (HD only)", "");

                public static readonly GUIContent TitleRaymarchingQuality       = new GUIContent("Raymarching Qualities (Name / Steps Count)", "Specify each different raymarching quality with:\n- an understandable name (to be chosen per HD beam)\n- a number specifying how many raymarching steps will be computed per pixel (the higher, the better quality, the slower the performance)\n\nA new HD beam shader variant will be generated per raymarching quality for the HD beam shader: the more different qualities you'll have, the slower it will get to compile all shader variants and to export standalone builds.");
                public static readonly GUIContent DefaultRaymarchingQuality     = new GUIContent("Default Raymarching Quality", "Raymarching Quality applied by default to newly created HD Volumetric Light Beams");

                public static readonly GUIContent FeatureEnabledShadow          = new GUIContent("Shadow (HD only)", "Support 'Volumetric Shadow' feature or not.");
                public static readonly GUIContent FeatureEnabledCookie          = new GUIContent("Cookie (HD only)", "Support 'Volumetric Cookie' featur or not.");
            }
        }

        public static class Effects
        {
            public static readonly GUIContent HeaderTimings = new GUIContent("Timings", "");
            public static readonly GUIContent HeaderPause = new GUIContent("Pause", "");
            public static readonly GUIContent HeaderVisual = new GUIContent("Visual", "");
            public static readonly GUIContent HeaderMisc = new GUIContent("Misc", "");

            public static readonly GUIContent PerformPauses = new GUIContent("Perform Pauses", "If enabled, pauses will be added between 2 flickering sequences.");
            public static readonly GUIContent FlickeringDuration = new GUIContent("Flickering Duration", "The duration of a flickering sequence.\nA random value will be picked each time inside this range.");
            public static readonly GUIContent PauseDuration = new GUIContent("Pause Duration", "The duration of a pause sequence.\nA random value will be picked each time inside this range.");
            public static readonly GUIContent RestoreIntensityOnPause = new GUIContent("Restore Intensity On Pause", "Restore the default intensity during the pause sequences.");
            public static readonly GUIContent FrequencyFlicker = new GUIContent("Frequency", "Frequency of flickering.\nHigher value means the flickering will occur faster.");
            public static readonly GUIContent FrequencyPulse = new GUIContent("Frequency", "Frequency of pulsing.\nHigher value means the pulsing will occur faster.");
            public static readonly GUIContent IntensityAmplitude = new GUIContent("Intensity Amplitude", "The amplitude of intensity change which will be applied to the Light and/or Beam.\nA random value will be picked each time inside that range.");
            public static readonly GUIContent Smoothing = new GUIContent("Smoothing", "How much intensity change will be smoothed.\nHigher value means the more smoothing.");
            public static readonly GUIContent RestoreIntensityOnDisable = new GUIContent("Restore Intensity On Disable", "Restore the default intensity when this component is disabled.");
            public static readonly GUIContent ComponentsToChange = new GUIContent("Components To Change", "Decide which component to change among:\n- Unity's Light\n- Volumetric Light Beam\n- Volumetric Dust Particles");
            public static readonly GUIContent ChangeVolumetricBeamIntensity = new GUIContent("Change Volumetric Beam Intensity", "Apply the flicker on the Volumetric Beam Intensity.");

            public static readonly GUIContent EffectProfile = new GUIContent("Effect Profile", "The Effect Profile asset to use");
            public static readonly GUIContent ButtonNewProfileFlicker = new GUIContent("New Flicker profile", "Create a new flicker profile asset");
            public static readonly GUIContent ButtonNewProfilePulse = new GUIContent("New Pulse profile", "Create a new pulse profile asset");


            public const string HelpNoValidComponents = "This component must be applied with a 'Unity Light' and/or a 'Volumetric Light Beam SD/HD'.";
            public const string HelpLightNotChangeable = "To apply this effect on the Unity Light's intensity, it must use 'Realtime' or 'Mixed' mode.";
            public const string HelpBeamNotChangeable = "To apply this effect on the Volumetric Light Beam's intensity, it must have its 'Track Changes During Playtime' property enabled.";

            public const string HelpEffectProfile = "Specify an Effect Profile asset to be applied on this 'Unity Light' and/or a 'Volumetric Light Beam SD/HD' at runtime.\nEffect Profile assets use Unity prefabs workflow to offer hierarchy and override systems to your assets.";
        }

        public static class LOD
        {
            public const string Info = "Use the Unity's 'LODGroup' component to handle LODs count and bounds, but not the Renderers.\nUse this component to specify which beam is associated to each LOD.";
            public static readonly GUIContent TitleLODArray = new GUIContent("Volumetric Light Beam per LOD", "Specify which Volumetric Light Beam will be chosen for each Level Of Detail.");
            public static readonly GUIContent ResetAllLODsLocalTransform = new GUIContent("Reset all LODs local transforms", "Force all the Volumetric Light Beams LODs to be recentered according to their parent.\nUseful if all LODs are children of the LODGroup GameObject.");
            public static readonly GUIContent LOD0PropsToCopy = new GUIContent("LOD0 props to copy", "List of props to copy from the Volumetric Light Beam LOD0 to the other Volumetric Light Beams LODs");
            public static readonly GUIContent CopyLOD0PropsEachFrame = new GUIContent("Copy LOD0 props each frame", "- False: the LOD0 Beam's props will be copied only once on start (enough if the LOD0 beam doesn't change during playtime).\n- True: the LOD0 Beam's props will be copied every frame (useful if the LOD0 is supposed to change during playtime).");
            public static readonly GUIContent CullVolumetricDustParticles = new GUIContent("Cull Volumetric Dust Particles", "If a 'VolumetricDustParticles' component is attached to this VolumetricLightBeam, it will be also culled like the beam geometry.");

            public static readonly GUIContent ButtonNewBeam = new GUIContent("+", "Create a new Beam for this LOD, as child of the LOD Beam Group");
            public const string ButtonNewBeamSD = "Add new SD Beam";
            public const string ButtonNewBeamHD = "Add new HD Beam";
        }
    }
}
#endif
