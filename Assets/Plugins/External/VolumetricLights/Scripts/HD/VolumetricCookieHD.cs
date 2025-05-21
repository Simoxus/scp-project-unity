using UnityEngine;

namespace VLB
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VolumetricLightBeamHD))]
    [HelpURL(Consts.Help.HD.UrlCookie)]
    [AddComponentMenu(Consts.Help.HD.AddComponentMenuCookie)]
    public class VolumetricCookieHD : MonoBehaviour
    {
        public const string ClassName = "VolumetricCookieHD";

        /// <summary>
        /// How much the cookie texture will contribute to the beam rendering.
        /// </summary>
        public float contribution
        {
            get { return m_Contribution; }
            set { if (m_Contribution != value) { m_Contribution = value; SetDirty(); } }
        }

        /// <summary>
        /// Specify the texture mask asset.
        /// It can be a regular 'Cookie' texture or any other texture type.
        /// </summary>
        public Texture cookieTexture
        {
            get { return m_CookieTexture; }
            set { if (m_CookieTexture != value) { m_CookieTexture = value; SetDirty(); } }
        }

        /// <summary>
        /// Which channel(s) will be used to render the cookie.
        /// </summary>
        public CookieChannel channel
        {
            get { return m_Channel; }
            set { if (m_Channel != value) { m_Channel = value; SetDirty(); } }
        }

        /// <summary>
        /// - False: white/opaque value in chosen texture channel is visible.
        /// - True: white/opaque value in chosen texture channel is hidden.
        /// </summary>
        public bool negative
        {
            get { return m_Negative; }
            set { if (m_Negative != value) { m_Negative = value; SetDirty(); } }
        }

        /// <summary>
        /// 2D local translation applied to the cookie texture.
        /// </summary>
        public Vector2 translation
        {
            get { return m_Translation; }
            set { if (m_Translation != value) { m_Translation = value; SetDirty(); } }
        }

        /// <summary>
        /// Rotation angle of the cookie texture (in degrees).
        /// </summary>
        public float rotation
        {
            get { return m_Rotation; }
            set { if (m_Rotation != value) { m_Rotation = value; SetDirty(); } }
        }

        /// <summary>
        /// 2D local scale applied to the cookie texture.
        /// </summary>
        public Vector2 scale
        {
            get { return m_Scale; }
            set { if (m_Scale != value) { m_Scale = value; SetDirty(); } }
        }

        [SerializeField] float m_Contribution = Consts.Cookie.ContributionDefault;
        [SerializeField] Texture m_CookieTexture = Consts.Cookie.CookieTextureDefault;
        [SerializeField] CookieChannel m_Channel = Consts.Cookie.ChannelDefault;
        [SerializeField] bool m_Negative = Consts.Cookie.NegativeDefault;
        [SerializeField] Vector2 m_Translation = Consts.Cookie.TranslationDefault;
        [SerializeField] float m_Rotation = Consts.Cookie.RotationDefault;
        [SerializeField] Vector2 m_Scale = Consts.Cookie.ScaleDefault;

        VolumetricLightBeamHD m_Master = null;

        void SetDirty()
        {
            if (m_Master)
                m_Master.SetPropertyDirty(DirtyProps.CookieProps);
        }

        public static void ApplyMaterialProperties(VolumetricCookieHD instance, BeamGeometryHD geom)
        {
            Debug.Assert(geom != null);
            if (instance && instance.enabled && instance.cookieTexture != null)
            {
                geom.SetMaterialProp(ShaderProperties.HD.CookieTexture, instance.cookieTexture);
                geom.SetMaterialProp(ShaderProperties.HD.CookieProperties,
                    new Vector4(instance.negative ? instance.contribution : -instance.contribution
                    , (float)instance.channel
                    , Mathf.Cos(instance.rotation * Mathf.Deg2Rad)
                    , Mathf.Sin(instance.rotation * Mathf.Deg2Rad)
                    ));
                geom.SetMaterialProp(ShaderProperties.HD.CookiePosAndScale, new Vector4(instance.translation.x, instance.translation.y, instance.scale.x, instance.scale.y));
            }
            else
            {
                geom.SetMaterialProp(ShaderProperties.HD.CookieTexture, BeamGeometryHD.InvalidTexture.Null);
                geom.SetMaterialProp(ShaderProperties.HD.CookieProperties, Vector4.zero);
            }
        }

        void Awake()
        {
            m_Master = GetComponent<VolumetricLightBeamHD>();
            Debug.Assert(m_Master);
        }

        void OnEnable() { SetDirty(); }
        void OnDisable() { SetDirty(); }
        void OnDidApplyAnimationProperties() { SetDirty(); }

        void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                MakeBeamGeomDirty();
#endif // UNITY_EDITOR

            if (Application.isPlaying)
            {
                SetDirty();
            }
        }

        void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                MakeBeamGeomDirty();
#endif // UNITY_EDITOR

            if (Application.isPlaying)
            {
                SetDirty();
            }
        }

#if UNITY_EDITOR
        void MakeBeamGeomDirty()
        {
            var beam = GetComponent<VolumetricLightBeamHD>();
            if (beam)
                beam._EditorSetBeamGeomDirty(); // need to recall BeamGeometry.Initialize to force havin a custom material
        }

        void OnValidate()
        {
            SetDirty();
        }
#endif // UNITY_EDITOR
    }
}

