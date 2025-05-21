#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace VLB
{
    [CustomEditor(typeof(LODBeamGroup))]
    public class Editor_LODBeamGroup : Editor_CommonHD
    {
        SerializedProperty m_LODBeams = null;
        SerializedProperty m_ResetAllLODsLocalTransform = null;
        SerializedProperty m_LOD0PropsToCopy = null;
        SerializedProperty m_CopyLOD0PropsEachFrame = null;
        SerializedProperty m_CullVolumetricDustParticles = null;

        LODBeamGroup m_Target = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            LODBeamsArrayInit();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_Target = target as LODBeamGroup;
            Debug.Assert(m_Target);

            EditorGUILayout.HelpBox(EditorStrings.LOD.Info, MessageType.Info);
            EditorGUILayout.Separator();

            DrawLODBeamsList();
            EditorGUILayout.PropertyField(m_ResetAllLODsLocalTransform, EditorStrings.LOD.ResetAllLODsLocalTransform);
            EditorGUILayout.PropertyField(m_LOD0PropsToCopy, EditorStrings.LOD.LOD0PropsToCopy);
            EditorGUILayout.PropertyField(m_CopyLOD0PropsEachFrame, EditorStrings.LOD.CopyLOD0PropsEachFrame);
            EditorGUILayout.PropertyField(m_CullVolumetricDustParticles, EditorStrings.LOD.CullVolumetricDustParticles);

            serializedObject.ApplyModifiedProperties();
        }

        #region LOD Beams Array
        ReorderableList m_LODBeamsList;

        void LODBeamsArrayInit()
        {
            m_LODBeamsList = new ReorderableList(serializedObject
                , m_LODBeams
                , true // draggable
                , true // displayHeader
                , true // displayAddButton
                , true // displayRemoveButton
                );
            m_LODBeamsList.drawHeaderCallback = LODBeamsListDrawHeader;
            m_LODBeamsList.drawElementCallback = LODBeamsListDrawElement;
            m_LODBeamsList.onCanRemoveCallback = LODBeamsListOnCanRemove;
        }

        void LODBeamsListDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, EditorStrings.LOD.TitleLODArray);
        }

        static string GetLODElementName(LODBeamGroup LODBeamGroup, int LODi)
        {
            Debug.Assert(LODBeamGroup);

            string details = string.Format("LOD {0}", LODi);
            var lodData = new LOD();
            if (LODBeamGroup.GetLODFromLODGroup(LODi, ref lodData))
            {
                int boundMin = Mathf.RoundToInt(100 * lodData.screenRelativeTransitionHeight);
                int boundMax = 100;

                var lodDataPrev = new LOD();
                if (LODBeamGroup.GetLODFromLODGroup(LODi - 1, ref lodDataPrev))
                {
                    boundMax = Mathf.RoundToInt(100 * lodDataPrev.screenRelativeTransitionHeight);
                }

                details += string.Format(" [{0}-{1}%]", boundMax, boundMin);
            }
            else
            {
                details += " [UNDEFINED]";
            }
            return details;
        }

        protected void AddChildBeam<T>(object LODiAsObj) where T : VolumetricLightBeamAbstractBase
        {
            int LODi = (int)LODiAsObj;
            var beamName = string.Format("LOD{0} Beam", LODi);
            var newBeam = new GameObject(beamName, typeof(T));
            EditorExtensions.OnNewGameObjectCreated(newBeam, m_Target.gameObject);
            m_Target.SetLODBeamComponent(LODi, newBeam.GetComponent<T>());
        }

        void LODBeamsListDrawElement(Rect rect, int i, bool isActive, bool isFocused)
        {
            bool hasReference = m_LODBeams.GetArrayElementAtIndex(i).objectReferenceValue;

            string details = GetLODElementName(m_Target, i);

            Rect rectProperty = rect;
            Rect rectSteps = rect;

            if (!hasReference)
            {
                const float kPropSeparator = 5.0f;
                const float kWidthButton = 20.0f;

                const float kBorder = 1.0f;
                rect.yMin += kBorder;
                rect.yMax -= kBorder * 2;

                rectProperty.width -= kWidthButton + kPropSeparator;

                rectSteps.x = rect.xMax - kWidthButton;
                rectSteps.width = kWidthButton;
            }

            EditorGUI.PropertyField(rectProperty, m_LODBeams.GetArrayElementAtIndex(i), new GUIContent(details));

            if(!hasReference)
            {
                if (GUI.Button(rectSteps, EditorStrings.LOD.ButtonNewBeam))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(EditorStrings.LOD.ButtonNewBeamSD), false, AddChildBeam<VolumetricLightBeamSD>, i);
                    menu.AddItem(new GUIContent(EditorStrings.LOD.ButtonNewBeamHD), false, AddChildBeam<VolumetricLightBeamHD>, i);
                    menu.ShowAsContext();
                }
            }
        }
       
        bool LODBeamsListOnCanRemove(ReorderableList list)
        {
            var lods = m_Target.GetLODsFromLODGroup();
            return lods.Length < list.count;
        }

        void DrawLODBeamsList()
        {
            if (m_Target.IsPropertlyLoaded()) // fix errors when Unity's LODGroup component is disabled
            {
                Debug.Assert(m_LODBeamsList != null);
                m_LODBeamsList.DoLayoutList();
            }
        }
        #endregion
    }
}
#endif
