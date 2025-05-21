#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VLB
{
    public abstract class Editor_VolumetricLightBeamAbstractBase : Editor_Common
    {
        TargetList<VolumetricLightBeamAbstractBase> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<VolumetricLightBeamAbstractBase>(targets);
        }

        protected string GetBatchingReport()
        {
            if (m_Targets.Count > 1)
            {
                string reasons = "";
                for (int i = 1; i < m_Targets.Count; ++i)
                {
                    if (!BatchingHelper.CanBeBatched(m_Targets[0], m_Targets[i], ref reasons))
                    {
                        return "Selected beams can't be batched together:\n" + reasons;
                    }
                }
            }
            return null;
        }


        protected void AddComponentToTargets<T>() where T : MonoBehaviour
        {
            foreach (var target in m_Targets) EditorExtensions.AddComponentFromEditor<T>(target);
        }

        protected void DrawEditInSceneButton()
        {
            EditorGUI.BeginChangeCheck();
            bool editInScene = EditorPrefs.GetBool(EditorStrings.Beam.PrefEditInScene, false);
            editInScene = GUILayout.Toggle(editInScene, EditorStrings.Beam.ButtonEditInScene, EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(EditorStrings.Beam.PrefEditInScene, editInScene);
                SceneView.RepaintAll();
            }
        }

        protected bool DrawFallOffSliderHandle(Transform beamTransf, Vector3 beamGlobalForward, Vector3 beamScale, float lineStartOffset, float lineAlpha, Handles.CapFunction capFunction, string recordName, System.Func<float> getValue, System.Action<float> setValue)
        {
            Debug.Assert(beamTransf != null);

            EditorGUI.BeginChangeCheck();
            var sliderPos = beamTransf.position + getValue() * beamScale.z * beamGlobalForward;
            sliderPos = Handles.Slider(sliderPos, beamGlobalForward, HandleUtility.GetHandleSize(sliderPos) * 0.1f, capFunction, 0.5f);

            Handles.Label(sliderPos, string.Format("{0} {1:0.0}", recordName, getValue()), labelStyle);

            var savedCol = Handles.color;
            {
                var col = savedCol;
                col.a = lineAlpha;
                Handles.color = col;
                Handles.DrawLine(beamTransf.position + beamGlobalForward * lineStartOffset, sliderPos);
                Handles.color = savedCol;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, string.Format("Edit Beam '{0}'", recordName));

                var sliderVec = (sliderPos - beamTransf.position);
                var sliderLength = 0.0f;
                if (Vector3.Dot(sliderVec, beamGlobalForward) > 0.0f)
                    sliderLength = sliderVec.magnitude;

                sliderLength /= beamScale.z;
                setValue(sliderLength);
                return true;
            }
            return false;
        }

        protected GUIStyle labelStyle
        {
            get
            {
                var style = new GUIStyle(GUI.skin.GetStyle("Label"));
                style.normal.textColor = Handles.color;
                style.alignment = TextAnchor.LowerCenter;
                style.fixedWidth = 200.0f;
                return style;
            }
        }

        bool DrawRadiusHandle(Transform beamTransf, Vector3 beamGlobalForward, Vector3 beamScale, Vector3 pos, string recordName, System.Func<float> getValue, System.Action<float> setValue)
        {
            Debug.Assert(beamTransf != null);

            var rot = Quaternion.LookRotation(beamTransf.up, beamGlobalForward);

            UnityEditor.IMGUI.Controls.ArcHandle hdl = null;

            float maxScaleXY = Mathf.Max(beamScale.x, beamScale.y);

            EditorGUI.BeginChangeCheck();
            {
                using (new Handles.DrawingScope(Matrix4x4.TRS(pos, rot * Quaternion.Euler(0.0f, 45.0f, 0.0f), Vector3.one)))
                {
                    hdl = EditorExtensions.DrawHandleRadius(getValue() * maxScaleXY);
                }

                using (new Handles.DrawingScope(Matrix4x4.TRS(pos, rot, Vector3.one)))
                {
                    Handles.Label(getValue() * maxScaleXY * Vector3.forward, string.Format("{0} {1:0.00}", recordName, getValue()), labelStyle);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, string.Format("Edit Beam '{0}'", recordName));
                setValue(hdl.radius / maxScaleXY);
                return true;
            }
            return false;
        }

        bool DrawSpotAngleHandle(Transform beamTransf, Vector3 beamGlobalForward, Vector3 pos, float handleRadius, string recordName, System.Func<float> getValue, System.Action<float> setValue)
        {
            Debug.Assert(beamTransf != null);

            var left = Vector3.Cross(beamGlobalForward, beamTransf.up); // show angle handle properly with 2d beams
            var rot = Quaternion.LookRotation(beamGlobalForward, left);

            UnityEditor.IMGUI.Controls.ArcHandle hdl = null;
            EditorGUI.BeginChangeCheck();
            {
                using (new Handles.DrawingScope(Matrix4x4.TRS(pos, rot * Quaternion.Euler(0.0f, -getValue() / 2, 0.0f), Vector3.one)))
                {
                    hdl = EditorExtensions.DrawHandleSpotAngle(getValue(), handleRadius);
                }

                using (new Handles.DrawingScope(Matrix4x4.TRS(pos, rot, Vector3.one)))
                {
                    Handles.Label(Vector3.forward * handleRadius, string.Format("{0} {1:0.0}", recordName, getValue()), labelStyle);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, string.Format("Edit Beam '{0}'", recordName));
                setValue(hdl.angle);
                return true;
            }
            return false;
        }


        static Vector3 GetBeamGlobalForward(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.beamGlobalForward;
            if (beam is VolumetricLightBeamHD hd) return hd.beamGlobalForward;
            return Vector3.forward;
        }

        static Vector3 GetBeamLossyScaleIfScalable(VolumetricLightBeamAbstractBase beam)
        {
            if (beam.IsScalable())
                return beam.GetLossyScale();
            return Vector3.one;
        }

        static float GetBeamConeRadiusStart(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.coneRadiusStart;
            if (beam is VolumetricLightBeamHD hd) return hd.coneRadiusStart;
            return 0.0f;
        }

        static void SetBeamConeRadiusStart(VolumetricLightBeamAbstractBase beam, float radius)
        {
            if (beam is VolumetricLightBeamSD sd) sd.coneRadiusStart = radius;
            if (beam is VolumetricLightBeamHD hd) hd.coneRadiusStart = radius;
        }

        static float GetBeamConeRadiusEnd(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.coneRadiusEnd;
            if (beam is VolumetricLightBeamHD hd) return hd.coneRadiusEnd;
            return 1.0f;
        }

        static void SetBeamConeRadiusEnd(VolumetricLightBeamAbstractBase beam, float radius)
        {
            if (beam is VolumetricLightBeamSD sd) sd.coneRadiusEnd = radius;
            if (beam is VolumetricLightBeamHD hd) hd.coneRadiusEnd = radius;
        }

        static float GetBeamSpotAngle(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.spotAngle;
            if (beam is VolumetricLightBeamHD hd) return hd.spotAngle;
            return 1.0f;
        }

        static void SetBeamSpotAngle(VolumetricLightBeamAbstractBase beam, float radius)
        {
            if (beam is VolumetricLightBeamSD sd) sd.spotAngle = radius;
            if (beam is VolumetricLightBeamHD hd) hd.spotAngle = radius;
        }

        static float GetBeamFallOffEnd(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.fallOffEnd;
            if (beam is VolumetricLightBeamHD hd) return hd.fallOffEnd;
            return 1.0f;
        }

        static void SetBeamFallOffEnd(VolumetricLightBeamAbstractBase beam, float radius)
        {
            if (beam is VolumetricLightBeamSD sd) sd.fallOffEnd = radius;
            if (beam is VolumetricLightBeamHD hd) hd.fallOffEnd = radius;
        }

        static float GetBeamFallOffStart(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.fallOffStart;
            if (beam is VolumetricLightBeamHD hd) return hd.fallOffStart;
            return 1.0f;
        }

        static bool GetBeamHasMeshSkewing(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.hasMeshSkewing;
            return false;
        }

        static bool GetBeamUseSpotAngleFromLight(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.spotAngleFromLight;
            if (beam is VolumetricLightBeamHD hd) return hd.useSpotAngleFromAttachedLightSpot;
            return false;
        }

        static bool GetBeamUseFallOffEndFromLight(VolumetricLightBeamAbstractBase beam)
        {
            if (beam is VolumetricLightBeamSD sd) return sd.fallOffEndFromLight;
            if (beam is VolumetricLightBeamHD hd) return hd.useFallOffEndFromAttachedLightSpot;
            return false;
        }

        protected virtual bool DrawEditInSceneHandles()
        {
            bool editInScene = EditorPrefs.GetBool(EditorStrings.Beam.PrefEditInScene, false);
            if (!editInScene)
                return false;

            var beam = target as VolumetricLightBeamAbstractBase;
            Debug.Assert(beam != null);

            bool update = false;

            VolumetricLightBeamSD.AttachedLightType lightType;
            var hasSpotLight = beam.GetLightSpotAttachedSlow(out lightType) != null;

            Handles.color = beam.ComputeColorAtDepth(0.0f).ComputeComplementaryColor(true);
            {
                update |= DrawRadiusHandle(beam.transform, GetBeamGlobalForward(beam), GetBeamLossyScaleIfScalable(beam), beam.transform.position, "Source Radius", () => GetBeamConeRadiusStart(beam), (float v) => SetBeamConeRadiusStart(beam, v));

                if (!hasSpotLight || !GetBeamUseSpotAngleFromLight(beam))
                {
                    update |= DrawSpotAngleHandle(beam.transform, GetBeamGlobalForward(beam), beam.transform.position, GetBeamFallOffEnd(beam) / 2, "Spot Angle", () => GetBeamSpotAngle(beam), (float v) => SetBeamSpotAngle(beam, v));
                }
            }

            if (!GetBeamHasMeshSkewing(beam))
            {
                Handles.color = beam.ComputeColorAtDepth(1.0f).ComputeComplementaryColor(true);
                {
                    if (!hasSpotLight || !GetBeamUseFallOffEndFromLight(beam))
                    {
                        update |= DrawFallOffSliderHandle(beam.transform, GetBeamGlobalForward(beam), GetBeamLossyScaleIfScalable(beam), GetBeamFallOffStart(beam), 0.5f, Handles.CubeHandleCap, "Range Limit", () => GetBeamFallOffEnd(beam), (float v) => SetBeamFallOffEnd(beam, v));
                    }

                    if (!hasSpotLight || !GetBeamUseSpotAngleFromLight(beam))
                    {
                        Vector3 scale = GetBeamLossyScaleIfScalable(beam);
                        Vector3 globalForward = GetBeamGlobalForward(beam);
                        var fallOffEndPos = beam.transform.position + globalForward * GetBeamFallOffEnd(beam) * scale.z;
                        update |= DrawRadiusHandle(beam.transform, globalForward, scale, fallOffEndPos, "End Radius", () => GetBeamConeRadiusEnd(beam), (float v) => SetBeamConeRadiusEnd(beam, v));
                    }
                }
            }

            return update;
        }



        protected GUIStyle buttonAddComponentStyle { get => GUI.skin.button; }

        protected void DrawButtonAddComponentDust()
        {
            bool showButtonDust = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamAbstractBase beam) => { return beam.GetComponent<VolumetricDustParticles>() == null; });

            if (showButtonDust && GUILayout.Button(EditorData.Instance.contentAddDustParticles, buttonAddComponentStyle))
                AddComponentToTargets<VolumetricDustParticles>();
        }

        protected void DrawButtonAddComponentTriggerZone()
        {
            bool showButtonTriggerZone = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamAbstractBase beam) => { return beam.GetComponent<TriggerZone>() == null; });

            if (showButtonTriggerZone && GUILayout.Button(EditorData.Instance.contentAddTriggerZone, buttonAddComponentStyle))
                AddComponentToTargets<TriggerZone>();
        }

        protected void DrawButtonAddComponentEffect()
        {
            bool showButtonEffect = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeamAbstractBase beam) =>
            {
                return beam.GetComponent<EffectAbstractBase>() == null && beam.GetComponent<EffectFromProfile>() == null;
            });

            if (showButtonEffect && GUILayout.Button(EditorData.Instance.contentAddEffect, buttonAddComponentStyle))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddEffectFlicker), false, AddComponentToTargets<EffectFlicker>);
                menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddEffectPulse), false, AddComponentToTargets<EffectPulse>);
                menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddEffectFromProfile), false, AddComponentToTargets<EffectFromProfile>);
                menu.ShowAsContext();
            }
        }

        protected override void GetInfoTips(List<InfoTip> tips)
        {
            var gpuInstancingReport = GetBatchingReport();
            if (!string.IsNullOrEmpty(gpuInstancingReport))
                tips.Add(new InfoTip { type = MessageType.Warning, message = gpuInstancingReport });

            base.GetInfoTips(tips);
        }
    }
}
#endif // UNITY_EDITOR

