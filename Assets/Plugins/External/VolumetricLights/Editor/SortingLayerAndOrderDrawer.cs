#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace VLB
{
    static class SortingLayerAndOrderDrawer
    {
        [System.Flags]
        public enum UpdatedProperties
        {
            None = 0,
            SortingLayerID = 1 << 1,
            SortingOrder = 1 << 2,
        }

        public static UpdatedProperties Draw(SerializedProperty sortingLayerID, SerializedProperty sortingOrder)
        {
            string[] m_SortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();

            var updatedProperties = UpdatedProperties.None;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = sortingLayerID.hasMultipleDifferentValues;
                int layerIndex = System.Array.IndexOf(m_SortingLayerNames, SortingLayer.IDToName(sortingLayerID.intValue));
                layerIndex = EditorGUILayout.Popup(EditorStrings.Beam.SortingLayer, layerIndex, m_SortingLayerNames);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    sortingLayerID.intValue = SortingLayer.NameToID(m_SortingLayerNames[layerIndex]);

                    updatedProperties |= UpdatedProperties.SortingLayerID;
                }

                using (new EditorExtensions.LabelWidth(40f))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(sortingOrder, EditorStrings.Beam.SortingOrder, GUILayout.MaxWidth(90f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        updatedProperties |= UpdatedProperties.SortingOrder;
                    }
                }
            }

            return updatedProperties;
        }
    }
}
#endif
