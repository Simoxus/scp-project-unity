using System;
using UnityEngine;

namespace VLB
{
    public static class Utils
    {
        public static float ComputeConeRadiusEnd(float fallOffEnd, float spotAngle)
        {
            return fallOffEnd * Mathf.Tan(spotAngle * Mathf.Deg2Rad * 0.5f);
        }

        public static float ComputeSpotAngle(float fallOffEnd, float coneRadiusEnd)
        {
            return Mathf.Atan2(coneRadiusEnd, fallOffEnd) * Mathf.Rad2Deg * 2.0f;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        public static void ResizeArray<T>(ref T[] array, int newSize)
        {
            System.Array.Resize<T>(ref array, newSize);
        }

        public static bool IsValidIndex<T>(this T[] array, int idx)
        {
            return idx >= 0 && idx < array.Length;
        }

        public static string GetPath(Transform current)
        {
            if (current.parent == null)
                return "/" + current.name;
            return GetPath(current.parent) + "/" + current.name;
        }

        public static T NewWithComponent<T>(string name) where T : Component
        {
            return (new GameObject(name, typeof(T))).GetComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            var component = self.GetComponent<T>();
            if (component == null)
                component = self.AddComponent<T>();
            return component;
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
        {
            return self.gameObject.GetOrAddComponent<T>();
        }

        // Call the function for each component placed on any children (every depth), but not on itself
        public static void ForeachComponentsInAnyChildrenOnly<T>(this GameObject self, Action<T> lambda, bool includeInactive = false) where T : Component
        {
            var components = self.GetComponentsInChildren<T>(includeInactive);
            foreach (var comp in components)
            {
                if (comp.gameObject != self)
                {
                    lambda(comp);
                }
            }
        }

        // Call the function for each component placed on any direct children on this GameObject
        public static void ForeachComponentsInDirectChildrenOnly<T>(this GameObject self, Action<T> lambda, bool includeInactive = false) where T : Component
        {
            var components = self.GetComponentsInChildren<T>(includeInactive);
            foreach (var comp in components)
            {
                if (comp.transform.parent == self.transform)
                {
                    lambda(comp);
                }
            }
        }

        public static void SetupDepthCamera(Camera depthCamera
            , float coneApexOffsetZ, float maxGeometryDistance, float coneRadiusStart, float coneRadiusEnd
            , Vector3 beamLocalForward, Vector3 lossyScale, bool isScalable, Quaternion beamInternalLocalRotation
            , bool shouldScaleMinNearClipPlane)
        {
            Debug.Assert(depthCamera);

            if(!isScalable)
                lossyScale.x = lossyScale.y = 1.0f;

            float apexDist = coneApexOffsetZ;
            bool isPersp = apexDist >= 0.0f;
            apexDist = Mathf.Max(apexDist, 0.0f);

            depthCamera.orthographic = !isPersp;

            depthCamera.transform.localPosition = beamLocalForward * (-apexDist);

            var localRot = beamInternalLocalRotation;
            if (Mathf.Sign(lossyScale.z) < 0f) localRot *= Quaternion.Euler(0f, 180f, 0f);
            depthCamera.transform.localRotation = localRot;

            if (!Mathf.Approximately(lossyScale.y * lossyScale.z, 0))
            {
                float kMinNearClipPlane = isPersp ? 0.1f : 0.0f; // should be the same than in shader
                float absScaleZ = Mathf.Abs(lossyScale.z);
                depthCamera.nearClipPlane = Mathf.Max(apexDist * absScaleZ, kMinNearClipPlane * (shouldScaleMinNearClipPlane ? absScaleZ : 1.0f));
                depthCamera.farClipPlane = (maxGeometryDistance + apexDist * (isScalable ? 1 : absScaleZ)) * (isScalable ? absScaleZ : 1);
                depthCamera.aspect = Mathf.Abs(lossyScale.x / lossyScale.y);

                if (isPersp)
                {
                    float fov = Mathf.Atan2(coneRadiusEnd * Mathf.Abs(lossyScale.y), depthCamera.farClipPlane) * Mathf.Rad2Deg * 2.0f;
                    Debug.Assert(fov < 180.0f);
                    depthCamera.fieldOfView = fov;
                }
                else
                {
                    depthCamera.orthographicSize = coneRadiusStart * lossyScale.y;
                }
            }
        }

        /// <summary>
        /// true if the bit field or bit fields that are set in flags are also set in the current instance; otherwise, false.
        /// </summary>
        public static bool HasFlag(this Enum mask, Enum flags) // Same behavior than Enum.HasFlag is .NET 4
        {
#if DEBUG
            if (mask.GetType() != flags.GetType())
                throw new System.ArgumentException(string.Format("The argument type, '{0}', is not the same as the enum type '{1}'.", flags.GetType(), mask.GetType()));
#endif
            return ((int)(IConvertible)mask & (int)(IConvertible)flags) == (int)(IConvertible)flags;
        }

        /// <summary>
        /// Returns this vector divided by the vector passed as argument
        /// </summary>
        public static Vector3 Divide(this Vector3 aVector, Vector3 scale)
        {
            if(Mathf.Approximately(scale.x * scale.y * scale.z, 0.0f))
                return Vector3.zero;
            return new Vector3(aVector.x / scale.x, aVector.y / scale.y, aVector.z / scale.z);
        }

        public static Vector2 xy(this Vector3 aVector) { return new Vector2(aVector.x, aVector.y); }
        public static Vector2 xz(this Vector3 aVector) { return new Vector2(aVector.x, aVector.z); }
        public static Vector2 yz(this Vector3 aVector) { return new Vector2(aVector.y, aVector.z); }
        public static Vector2 yx(this Vector3 aVector) { return new Vector2(aVector.y, aVector.x); }
        public static Vector2 zx(this Vector3 aVector) { return new Vector2(aVector.z, aVector.x); }
        public static Vector2 zy(this Vector3 aVector) { return new Vector2(aVector.z, aVector.y); }

        const float kEpsilon = 0.00001f;
        public static bool Approximately(this float a, float b, float epsilon = kEpsilon)       { return Mathf.Abs(a - b) < epsilon; }
        public static bool Approximately(this Vector2 a, Vector2 b, float epsilon = kEpsilon)   { return Vector2.SqrMagnitude(a - b) < epsilon; }
        public static bool Approximately(this Vector3 a, Vector3 b, float epsilon = kEpsilon)   { return Vector3.SqrMagnitude(a - b) < epsilon; }
        public static bool Approximately(this Vector4 a, Vector4 b, float epsilon = kEpsilon)   { return Vector4.SqrMagnitude(a - b) < epsilon; }

        public static Vector4 AsVector4(this Vector3 vec3, float w) { return new Vector4(vec3.x, vec3.y, vec3.z, w); }
        public static Vector4 PlaneEquation(Vector3 normalizedNormal, Vector3 pt) { return normalizedNormal.AsVector4(-Vector3.Dot(normalizedNormal, pt)); }

        public static float GetVolumeCubic(this Bounds self) { return self.size.x * self.size.y * self.size.z; }
        public static float GetMaxArea2D(this Bounds self) { return Mathf.Max(Mathf.Max(self.size.x * self.size.y, self.size.y * self.size.z), self.size.x * self.size.z); }

        public static Color Opaque(this Color self) { return new Color(self.r, self.g, self.b, 1f); }

        public static Color ComputeComplementaryColor(this Color self, bool blackAndWhite)
        {
            float offset = 0.5f;
            Color outColor = Color.white;

            for(int i = 0; i<3; ++i)
            {
                outColor[i] = self[i] + (self[i] < (1 - offset) ? offset : -offset);
            }

            return outColor;
        }

#if UNITY_EDITOR
        public static void GizmosDrawPlane(Vector3 normal, Vector3 position, Color color, Matrix4x4 mat, float size = 1f, float normalLength = 0.0f)
        {
            normal = normal.normalized;
            var prevMat = UnityEditor.Handles.matrix;
            var prevColor = UnityEditor.Handles.color;

            UnityEditor.Handles.matrix = mat;
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.RectangleHandleCap(0, position, Quaternion.LookRotation(normal), size, EventType.Repaint);

            if (normalLength > 0.0f)
            {
                UnityEditor.Handles.DrawLine(position, position + normal * normalLength);
                UnityEditor.Handles.ConeHandleCap(0, position + normal * normalLength, Quaternion.LookRotation(normal), normalLength * 0.25f, EventType.Repaint);
            }

            UnityEditor.Handles.matrix = prevMat;
            UnityEditor.Handles.color = prevColor;
        }
#endif // UNITY_EDITOR

        // Plane.Translate is not available in Unity 5
        public static Plane TranslateCustom(this Plane plane, Vector3 translation)
        {
            plane.distance += Vector3.Dot(translation.normalized, plane.normal) * translation.magnitude;
            return plane;
        }

        // Plane.ClosestPointOnPlaneCustom is not available in Unity 5
        public static Vector3 ClosestPointOnPlaneCustom(this Plane plane, Vector3 point)
        {
            return point - plane.GetDistanceToPoint(point) * plane.normal;
        }

        public static bool IsAlmostZero(float f) { return Mathf.Abs(f) < 0.001f; }

        public static bool IsValid(this Plane plane)
        {
            return plane.normal.sqrMagnitude > 0.5f;
        }

        public static void SetKeywordEnabled(this Material mat, string name, bool enabled)
        {
            if(enabled) mat.EnableKeyword(name);
            else mat.DisableKeyword(name);
        }

        public static void SetShaderKeywordEnabled(string name, bool enabled)
        {
            if (enabled) Shader.EnableKeyword(name);
            else Shader.DisableKeyword(name);
        }

        public static Matrix4x4 SampleInMatrix(this Gradient self, int floatPackingPrecision)
        {
            const int kSamplesCount = 16;
            var mat = new Matrix4x4();
            for (int i = 0; i < kSamplesCount; ++i)
            {
                var color = self.Evaluate(Mathf.Clamp01((float)(i) / (kSamplesCount - 1)));
                mat[i] = color.PackToFloat(floatPackingPrecision);
            }
            return mat;
        }

        public static Color[] SampleInArray(this Gradient self, int samplesCount)
        {
            var array = new Color[samplesCount];
            for (int i = 0; i < samplesCount; ++i)
                array[i] = self.Evaluate(Mathf.Clamp01((float)(i) / (samplesCount - 1)));
            return array;
        }

        static Vector4 Vector4_Floor(Vector4 vec) { return new Vector4(Mathf.Floor(vec.x), Mathf.Floor(vec.y), Mathf.Floor(vec.z), Mathf.Floor(vec.w)); }

        public static float PackToFloat(this Color color, int floatPackingPrecision)
        {
            Vector4 iVal = Vector4_Floor(color * (floatPackingPrecision - 1));

            float output = 0;

            output += iVal.x * floatPackingPrecision * floatPackingPrecision * floatPackingPrecision;
            output += iVal.y * floatPackingPrecision * floatPackingPrecision;
            output += iVal.z * floatPackingPrecision;
            output += iVal.w;

            return output;
        }

        public enum FloatPackingPrecision { High = 64, Low = 8, Undef = 0 }
        static FloatPackingPrecision ms_FloatPackingPrecision = FloatPackingPrecision.Undef;

        // OpenGL ES 2.0 GPU (graphicsShaderLevel = 30) usually have low float precision (16 bits on fragments)
        // So we lower the float packing precision on them (8 seems fine on Adreno (TM) 220, NVIDIA Tegra 3 and on Mali-450 MP)
        // https://docs.unity3d.com/Manual/SL-DataTypesAndPrecision.html
        const int kFloatPackingHighMinShaderLevel = 35;

        public static FloatPackingPrecision GetFloatPackingPrecision()
        {
            if (ms_FloatPackingPrecision == FloatPackingPrecision.Undef)
            {
                ms_FloatPackingPrecision = SystemInfo.graphicsShaderLevel >= kFloatPackingHighMinShaderLevel ? FloatPackingPrecision.High : FloatPackingPrecision.Low;
            }
            return ms_FloatPackingPrecision;
        }

        /// <summary>
        /// true if at least one of the bit of 'flags' is also set in the current instance; otherwise, false.
        /// </summary>
        public static bool HasAtLeastOneFlag(this Enum mask, Enum flags)
        {
#if DEBUG
            if (mask.GetType() != flags.GetType())
                throw new System.ArgumentException(string.Format("The argument type, '{0}', is not the same as the enum type '{1}'.", flags.GetType(), mask.GetType()));
#endif
            return ((int)(IConvertible)mask & (int)(IConvertible)flags) != 0;
        }

        public static void MarkCurrentSceneDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
#endif
        }

        public static void MarkObjectDirty(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(obj);
            }
#endif
        }

#if UNITY_EDITOR
        public static void _EditorSetAllBeamGeomDirty()
        {
            VolumetricLightBeamSD._EditorSetAllBeamGeomDirty();
            VolumetricLightBeamHD._EditorSetAllBeamGeomDirty();
        }

        public static void _EditorSetAllMeshesDirty()
        {
            VolumetricLightBeamSD._EditorSetAllMeshesDirty();
            VolumetricLightBeamHD._EditorSetAllMeshesDirty();
        }

        public static bool IsEditorCamera(Camera cam)
        {
            var sceneView = UnityEditor.SceneView.currentDrawingSceneView;
            if (sceneView)
            {
                return cam == sceneView.camera;
            }
            return false;
        }

        public static void SetSameSceneVisibilityStatesThan(this GameObject self, GameObject model)
        {
            // SceneVisibilityManager is a feature available from 2019.2, but fixed for transient objects only from 2019.3.14f1
            // https://issuetracker.unity3d.com/issues/toggling-of-picking-and-visibility-flags-of-a-gameobject-is-ignored-when-gameobject-dot-hideflags-is-set-to-hideflags-dot-dontsave
    #if UNITY_2019_3_OR_NEWER
            bool pickingDisabled = UnityEditor.SceneVisibilityManager.instance.IsPickingDisabled(model);
            self.SetScenePickabilityState(!pickingDisabled);

            bool hidden = UnityEditor.SceneVisibilityManager.instance.IsHidden(model);
            self.SetSceneVisibilityState(!hidden);
    #endif // UNITY_2019_3_OR_NEWER
        }

#if UNITY_2019_3_OR_NEWER
        public static void SetScenePickabilityState(this GameObject self, bool pickable)
        {
            if (pickable) UnityEditor.SceneVisibilityManager.instance.EnablePicking(self, true);
            else UnityEditor.SceneVisibilityManager.instance.DisablePicking(self, true);
        }

        public static void SetSceneVisibilityState(this GameObject self, bool visible)
        {
            if (visible) UnityEditor.SceneVisibilityManager.instance.Show(self, true);
            else UnityEditor.SceneVisibilityManager.instance.Hide(self, true);
        }
#endif // UNITY_2019_3_OR_NEWER
#endif // UNITY_EDITOR
    }

}
