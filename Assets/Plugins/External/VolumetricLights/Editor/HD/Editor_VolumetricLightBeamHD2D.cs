#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace VLB
{
    [CustomEditor(typeof(VolumetricLightBeamHD2D))]
    [CanEditMultipleObjects]
    public class Editor_VolumetricLightBeamHD2D : Editor_VolumetricLightBeamHD
    {
        SerializedProperty m_SortingLayerID = null, m_SortingOrder = null;
        string[] m_SortingLayerNames;

        TargetList<VolumetricLightBeamHD2D> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<VolumetricLightBeamHD2D>(targets);
            m_SortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
        }

        protected override void DrawProperties(bool hasLightSpot)
        {
            base.DrawProperties(hasLightSpot);
          
            if (FoldableHeader.Begin(this, EditorStrings.Beam.Header2D))
            {
                DrawSortingLayerAndOrder();
            }
            FoldableHeader.End();
        }

        void DrawSortingLayerAndOrder()
        {
            var updatedProperties = SortingLayerAndOrderDrawer.Draw(m_SortingLayerID, m_SortingOrder);

            if (updatedProperties.HasFlag(SortingLayerAndOrderDrawer.UpdatedProperties.SortingLayerID))
                m_Targets.RecordUndoAction("Edit Sorting Layer", (VolumetricLightBeamHD2D beam) => beam.sortingLayerID = m_SortingLayerID.intValue); // call setters

            if (updatedProperties.HasFlag(SortingLayerAndOrderDrawer.UpdatedProperties.SortingOrder))
                m_Targets.RecordUndoAction("Edit Sorting Order", (VolumetricLightBeamHD2D beam) => beam.sortingOrder = m_SortingOrder.intValue); // call setters
        }
    }
}
#endif
