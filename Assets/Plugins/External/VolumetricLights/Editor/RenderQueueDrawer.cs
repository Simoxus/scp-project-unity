#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    class RenderQueueDrawer
    {
        bool isPropValueInEnumList
        {
            get
            {
                foreach (RenderQueue rq in System.Enum.GetValues(typeof(RenderQueue)))
                {
                    if (rq != RenderQueue.Custom && m_Prop.intValue == (int)rq)
                        return true;
                }
                return false;
            }
        }

        public RenderQueueDrawer(SerializedProperty sprop)
        {
            Debug.Assert(sprop != null);
            m_Prop = sprop;
            m_IsRenderQueueCustom = !isPropValueInEnumList;
        }

        public void Draw(GUIContent label)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.BeginChangeCheck();

                    if (!m_IsRenderQueueCustom && !isPropValueInEnumList)
                        m_IsRenderQueueCustom = true; // handle proper dropbox change to "custom" when resetting Config to default values

                    RenderQueue rq = m_IsRenderQueueCustom ? RenderQueue.Custom : (RenderQueue)m_Prop.intValue;
                    rq = (RenderQueue)EditorGUILayout.EnumPopup(label, rq);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_IsRenderQueueCustom = (rq == RenderQueue.Custom);

                        if (!m_IsRenderQueueCustom)
                            m_Prop.intValue = (int)rq;
                    }

                    EditorGUI.BeginDisabledGroup(!m_IsRenderQueueCustom);
                    {
                        m_Prop.intValue = EditorGUILayout.IntField(m_Prop.intValue, GUILayout.MaxWidth(65.0f));
                    }
                    EditorGUI.EndDisabledGroup();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Utils._EditorSetAllBeamGeomDirty(); // TODO switch sd / hd ?
                }
            }
        }

        SerializedProperty m_Prop;
        bool m_IsRenderQueueCustom = false;
    }
}
#endif
