#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VLB
{
    [CustomEditor(typeof(VolumetricDustParticles))]
    [CanEditMultipleObjects]
    public class Editor_VolumetricDustParticles : Editor_CommonSD
    {
        SerializedProperty alpha = null;
        SerializedProperty size = null;
        SerializedProperty direction = null;
        SerializedProperty velocity = null;
        SerializedProperty density = null;
        SerializedProperty spawnDistanceRange = null;
        SerializedProperty cullingEnabled = null;
        SerializedProperty cullingMaxDistance = null;

        static bool AreParticlesInfosUpdated() { return Application.isPlaying; }
        public override bool RequiresConstantRepaint() { return AreParticlesInfosUpdated(); }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var particles = target as VolumetricDustParticles;

            if (particles.gameObject.activeSelf && particles.enabled && !particles.particlesAreInstantiated)
            {
                EditorGUILayout.HelpBox(EditorStrings.DustParticles.HelpFailToInstantiate, MessageType.Error);
                ButtonOpenConfig();
            }

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderRendering))
            {
                EditorGUILayout.PropertyField(alpha, EditorStrings.DustParticles.Alpha);
                EditorGUILayout.PropertyField(size, EditorStrings.DustParticles.Size);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderDirectionAndVelocity))
            {
                EditorGUILayout.PropertyField(direction, EditorStrings.DustParticles.Direction);

                if (particles.direction == ParticlesDirection.Random)
                {
                    var vec = velocity.vector3Value;
                    vec.z = EditorGUILayout.FloatField(EditorStrings.DustParticles.Velocity, vec.z);
                    velocity.vector3Value = vec;
                }
                else
                {
                    EditorGUILayout.PropertyField(velocity, EditorStrings.DustParticles.Velocity);
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderCulling))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(cullingEnabled, EditorStrings.DustParticles.CullingEnabled);
                    if (AreParticlesInfosUpdated())
                    {
                        EditorGUILayout.LabelField(string.Format("(Camera: '{0}')", Config.Instance.fadeOutCameraName));
                    }
                }

                if (cullingEnabled.boolValue)
                    EditorGUILayout.PropertyField(cullingMaxDistance, EditorStrings.DustParticles.CullingMaxDistance);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderSpawning))
            {
                EditorGUILayout.PropertyField(density, EditorStrings.DustParticles.Density);
                EditorGUILayout.PropertyField(spawnDistanceRange, EditorStrings.DustParticles.SpawnDistanceRange);

                {
                    var infos = "Current particles count: ";
                    if (AreParticlesInfosUpdated()) infos += particles.particlesCurrentCount;
                    else infos += "(playtime only)";
                    if (particles.isCulled)
                        infos += string.Format(" (culled by '{0}')", Config.Instance.fadeOutCameraName);
                    infos += string.Format("\nMax particles count: {0}", particles.particlesMaxCount);
                    EditorGUILayout.HelpBox(infos, MessageType.Info);
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderInfos))
            {
                EditorGUILayout.HelpBox(EditorStrings.DustParticles.HelpRecommendation, MessageType.Info);
            }
            FoldableHeader.End();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
