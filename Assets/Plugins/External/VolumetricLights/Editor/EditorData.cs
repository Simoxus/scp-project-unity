#if UNITY_EDITOR
using UnityEngine;

namespace VLB
{
    public class EditorData : ScriptableObject
    {
        [SerializeField] Texture2D buttonAddDustParticles = null;
        [SerializeField] Texture2D buttonAddDynamicOcclusion = null;
        [SerializeField] Texture2D buttonAddTriggerZone = null;
        [SerializeField] Texture2D buttonAddEffect = null;
        [SerializeField] Texture2D buttonFromSpotLight = null;

        [SerializeField] Texture2D buttonAddCookieHD = null;
        [SerializeField] Texture2D buttonAddShadowHD = null;
        [SerializeField] Texture2D buttonAddTrackRealtimeChangesOnLightHD = null;

        public GUIContent contentAddDustParticles       { get { return new GUIContent(Instance.buttonAddDustParticles, EditorStrings.Beam.ButtonAddDustParticles); } } 
        public GUIContent contentAddDynamicOcclusion    { get { return new GUIContent(Instance.buttonAddDynamicOcclusion, EditorStrings.Beam.ButtonAddDynamicOcclusion); } } 
        public GUIContent contentAddTriggerZone         { get { return new GUIContent(Instance.buttonAddTriggerZone, EditorStrings.Beam.ButtonAddTriggerZone); } } 
        public GUIContent contentAddEffect              { get { return new GUIContent(Instance.buttonAddEffect, EditorStrings.Beam.ButtonAddEffect); } } 
        public GUIContent contentFromSpotLight          { get { return new GUIContent(Instance.buttonFromSpotLight, EditorStrings.Beam.FromSpotLight); } }

        public GUIContent contentAddCookieHD            { get { return new GUIContent(Instance.buttonAddCookieHD, EditorStrings.Beam.HD.ButtonAddCookie); } } 
        public GUIContent contentAddShadowHD            { get { return new GUIContent(Instance.buttonAddShadowHD, EditorStrings.Beam.HD.ButtonAddShadow); } }
        public GUIContent contentAddTrackRealtimeChangesOnLightHD { get { return new GUIContent(Instance.buttonAddTrackRealtimeChangesOnLightHD, EditorStrings.Beam.HD.ButtonAddTrackRealtimeChangesOnLight); } }

        static EditorData ms_Instance = null;
        public static EditorData Instance
        {
            get
            {
                if (ms_Instance == null)
                {
                    ms_Instance = Resources.Load<EditorData>("VLBEditorData");
                    Debug.Assert(ms_Instance != null, "Failed to find asset 'VLBEditorData', please reinstall the 'Volumetric Light Beam' plugin.");
                }
                return ms_Instance;
            }
        }
    }
}
#endif

