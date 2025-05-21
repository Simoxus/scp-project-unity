#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VLB
{
    [CustomEditor(typeof(TrackRealtimeChangesOnLightHD))]
    [CanEditMultipleObjects]
    public class Editor_TrackRealtimeChangesOnLightHD : Editor_CommonHD
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawInfos();
        }

        protected override void GetInfoTips(List<InfoTip> tips)
        {
            tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Beam.HD.TipTrackRealtimeChangesOnLight });
            base.GetInfoTips(tips);
        }
    }
}

#endif
