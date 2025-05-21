using UnityEngine;

namespace VLB
{
    [HelpURL(Consts.Help.UrlEffectFromProfile)]
    [AddComponentMenu(Consts.Help.AddComponentMenuEffectFromProfile)]
    public class EffectFromProfile : MonoBehaviour
    {
        public const string ClassName = "EffectFromProfile";

        public EffectAbstractBase effectProfile
        {
            get { return m_EffectProfile; }
            set
            { 
                m_EffectProfile = value;
                InitInstanceFromProfile();
            }
        }

        public void InitInstanceFromProfile()
        {
            if (m_EffectInstance)
            {
                if (m_EffectProfile)
                    m_EffectInstance.InitFrom(m_EffectProfile);
                else
                    m_EffectInstance.enabled = false;
            }
        }

        void OnEnable()
        {
            if (m_EffectInstance)
            {
                m_EffectInstance.enabled = true;
            }
            else if (m_EffectProfile)
            {
                m_EffectInstance = (gameObject.AddComponent(m_EffectProfile.GetType()) as EffectAbstractBase);
                InitInstanceFromProfile();
            }
        }

        void OnDisable()
        {
            if (m_EffectInstance)
            {
                m_EffectInstance.enabled = false;
            }
        }

        [SerializeField]
        EffectAbstractBase m_EffectProfile = null;

        EffectAbstractBase m_EffectInstance = null;
    }
}
