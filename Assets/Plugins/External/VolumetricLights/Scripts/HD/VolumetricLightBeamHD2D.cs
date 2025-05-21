using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace VLB
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [SelectionBase]
    [HelpURL(Consts.Help.HD.UrlBeam)]
    [AddComponentMenu(Consts.Help.HD.AddComponentMenuBeam2D)]
    public partial class VolumetricLightBeamHD2D : VolumetricLightBeamHD
    {
        /// <summary>
        /// Unique ID of the beam's sorting layer.
        /// </summary>
        public int sortingLayerID
        {
            get { return m_SortingLayerID; }
            set {
                m_SortingLayerID = value;
                if (m_BeamGeom) m_BeamGeom.sortingLayerID = value;
            }
        }

        /// <summary>
        /// Name of the beam's sorting layer.
        /// </summary>
        public string sortingLayerName
        {
            get { return SortingLayer.IDToName(sortingLayerID); }
            set { sortingLayerID = SortingLayer.NameToID(value); }
        }

        /// <summary>
        /// The overlay priority within its layer.
        /// Lower numbers are rendered first and subsequent numbers overlay those below.
        /// </summary>
        public int sortingOrder
        {
            get { return m_SortingOrder; }
            set
            {
                m_SortingOrder = value;
                if (m_BeamGeom) m_BeamGeom.sortingOrder = value;
            }
        }

        public override Dimensions GetDimensions() { return Dimensions.Dim2D; }
        public override bool DoesSupportSorting2D() { return true; }
        public override int GetSortingLayerID() { return sortingLayerID; }
        public override int GetSortingOrder() { return sortingOrder; }

        [SerializeField] int m_SortingLayerID = 0;
        [SerializeField] int m_SortingOrder = 0;

        public override void CopyPropsFrom(VolumetricLightBeamAbstractBase beamSrc, BeamProps beamProps)
        {
            base.CopyPropsFrom(beamSrc, beamProps);

            if (beamSrc is VolumetricLightBeamSD)
            {
                var beamSD = beamSrc as VolumetricLightBeamSD;
                if (beamProps.HasFlag(BeamProps.Props2D)) { sortingLayerID = beamSD.sortingLayerID; sortingOrder = beamSD.sortingOrder; }

            }
            else if (beamSrc is VolumetricLightBeamHD2D)
            {
                var beamHD2D = beamSrc as VolumetricLightBeamHD2D;
                if (beamProps.HasFlag(BeamProps.Props2D)) { sortingLayerID = beamHD2D.sortingLayerID; sortingOrder = beamHD2D.sortingOrder; }
            }
        }

#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();

            sortingLayerID = 0;
            sortingOrder = 0;
        }
#endif // UNITY_EDITOR
    }
}
