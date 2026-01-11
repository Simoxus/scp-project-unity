using System.Collections.Generic;
using UnityEngine;

namespace Facility.Generation
{
    public class ObjectCuller : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objectsToCull = new List<GameObject>();
        [SerializeField] private List<MeshRenderer> renderersToCull = new List<MeshRenderer>();
        [SerializeField] private List<Collider> collidersToCull = new List<Collider>();

        [SerializeField] private bool cullOnStart = true;
        [SerializeField] private bool autoRegisterWithRoom = true;

        private RoomInstance _parentRoom;
        private bool _isCulled = false;

        public bool IsCulled => _isCulled;

        private void Awake()
        {
            if (autoRegisterWithRoom)
            {
                _parentRoom = GetComponentInParent<RoomInstance>();
            }
        }

        private void Start()
        {
            if (cullOnStart)
            {
                SetCulled(true);
            }
        }

        private void OnDestroy()
        {
            objectsToCull.Clear();
            renderersToCull.Clear();
            collidersToCull.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            objectsToCull.RemoveAll(obj => obj == null);
            renderersToCull.RemoveAll(renderer => renderer == null);
            collidersToCull.RemoveAll(collider => collider == null);
        }
#endif

        public void SetCulled(bool culled)
        {
            if (_isCulled == culled) return;

            _isCulled = culled;

            SetObjectsCulled(culled);
            SetRenderersCulled(culled);
            SetCollidersCulled(culled);

            Log.VerboseInfo($"ObjectCuller on {gameObject.name} culled state: {culled}");
        }

        public void ToggleCulling()
        {
            SetCulled(!_isCulled);
        }

        public void AddObjectToCull(GameObject obj)
        {
            if (obj != null && !objectsToCull.Contains(obj))
            {
                objectsToCull.Add(obj);

                if (_isCulled)
                {
                    obj.SetActive(false);
                }
            }
        }

        public void AddRendererToCull(MeshRenderer renderer)
        {
            if (renderer != null && !renderersToCull.Contains(renderer))
            {
                renderersToCull.Add(renderer);

                if (_isCulled)
                {
                    renderer.enabled = false;
                }
            }
        }

        public void AddColliderToCull(Collider collider)
        {
            if (collider != null && !collidersToCull.Contains(collider))
            {
                collidersToCull.Add(collider);

                if (_isCulled)
                {
                    collider.enabled = false;
                }
            }
        }

        private void SetObjectsCulled(bool culled)
        {
            for (int i = objectsToCull.Count - 1; i >= 0; i--)
            {
                if (objectsToCull[i] == null)
                {
                    objectsToCull.RemoveAt(i);
                    continue;
                }

                objectsToCull[i].SetActive(!culled);
            }
        }

        private void SetRenderersCulled(bool culled)
        {
            for (int i = renderersToCull.Count - 1; i >= 0; i--)
            {
                if (renderersToCull[i] == null)
                {
                    renderersToCull.RemoveAt(i);
                    continue;
                }

                renderersToCull[i].enabled = !culled;
            }
        }

        private void SetCollidersCulled(bool culled)
        {
            for (int i = collidersToCull.Count - 1; i >= 0; i--)
            {
                if (collidersToCull[i] == null)
                {
                    collidersToCull.RemoveAt(i);
                    continue;
                }

                collidersToCull[i].enabled = !culled;
            }
        }
    }
}