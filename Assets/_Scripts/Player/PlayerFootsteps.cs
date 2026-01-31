using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Behavior Settings")]
    public List<FootstepData> footstepSurfaces = new List<FootstepData>();
    public bool useBobSyncedFootsteps = true;

    [Header("FMOD Setup")]
    public Transform footstepPlayTransform;
    public string surfaceParameterName = "Surface";

    [Header("Raycast Settings")]
    public Transform raycastOrigin;
    public float raycastDistance = 4f;
    public LayerMask groundLayer;

    private Dictionary<string, FootstepData> _surfaceByTextureName = new Dictionary<string, FootstepData>();
    private FootstepData _currentFootstepData;

    private void Awake()
    {
        if (raycastOrigin == null)
        {
            raycastOrigin = transform;
        }

        BuildSurfaceDictionary();
    }

    private void OnEnable()
    {
        if (Core.Player.Bobbing != null)
        {
            Core.Player.Bobbing.OnFootstepTrigger += PlayFootstepAudio;
        }
    }

    private void OnDisable()
    {
        if (Core.Player.Bobbing != null)
        {
            Core.Player.Bobbing.OnFootstepTrigger -= PlayFootstepAudio;
        }
    }

    public void PlayFootstepAudio()
    {
        if (Core.Player.Controller == null) return;
        if (!Core.Player.IsMoving() || Core.Player.IsInState(PlayerState.Noclip)) return;

        DetectGroundSurface();

        if (_currentFootstepData == null) return;

        EventReference footstepEvent = GetFootstepEventForState(Core.Player.CurrentState);

        if (footstepEvent.IsNull)
            return;

        FMODHelper.PlayOneShotWithParameters(
            footstepEvent,
            footstepPlayTransform.position,
            (surfaceParameterName, _currentFootstepData.fmodParameterValue)
        );
    }

    private void BuildSurfaceDictionary()
    {
        _surfaceByTextureName.Clear();

        foreach (FootstepData surface in footstepSurfaces)
        {
            if (surface == null)
            {
                Log.VerboseWarning($"A FootstepData entry is null. Make sure to check your list in {name}.");
                continue;
            }

            foreach (Texture texture in surface.textures)
            {
                if (texture == null) continue;

                string identifier = texture.name;
                if (string.IsNullOrEmpty(identifier)) continue;

                if (!_surfaceByTextureName.ContainsKey(identifier))
                {
                    _surfaceByTextureName.Add(identifier, surface);
                }
                else
                {
                    Log.VerboseInfo($"Duplicate texture identifier '{identifier}' found. Ignoring duplicate.");
                }
            }
        }
    }

    private EventReference GetFootstepEventForState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Sprinting:
                return AudioDataAccess.Instance.Player.RunFootstepSound;
            case PlayerState.Walking:
            case PlayerState.Crouching:
            case PlayerState.Idle:
            case PlayerState.Freefall:
            default:
                return AudioDataAccess.Instance.Player.WalkFootstepSound;
        }
    }

    public void DetectGroundSurface()
    {
        if (!Physics.Raycast(raycastOrigin.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            Debug.DrawRay(raycastOrigin.position, Vector3.down * raycastDistance, Color.red, 0.1f);
            _currentFootstepData = footstepSurfaces.Count > 0 ? footstepSurfaces[0] : null;
            return;
        }

        Debug.DrawRay(raycastOrigin.position, Vector3.down * raycastDistance, Color.green, 0.1f);

        Material detectedMaterial = GetMaterialFromHit(hit);
        if (detectedMaterial == null)
        {
            _currentFootstepData = footstepSurfaces.Count > 0 ? footstepSurfaces[0] : null;
            return;
        }

        Texture detectedTexture = GetTextureFromMaterial(detectedMaterial);
        if (detectedTexture == null)
        {
            _currentFootstepData = footstepSurfaces.Count > 0 ? footstepSurfaces[0] : null;
            return;
        }

        if (_surfaceByTextureName.TryGetValue(detectedTexture.name, out FootstepData foundSurfaceDef))
        {
            _currentFootstepData = foundSurfaceDef;
            return;
        }

        foreach (var entry in _surfaceByTextureName)
        {
            if (detectedTexture.name.Contains(entry.Key))
            {
                _currentFootstepData = entry.Value;
                return;
            }
        }

        _currentFootstepData = footstepSurfaces.Count > 0 ? footstepSurfaces[0] : null;
    }

    private Material GetMaterialFromHit(RaycastHit hit)
    {
        Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
        if (hitRenderer == null)
            return null;

        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (meshCollider != null && meshCollider.sharedMesh != null && hitRenderer.sharedMaterials != null)
        {
            Mesh mesh = meshCollider.sharedMesh;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var submesh = mesh.GetSubMesh(i);
                if (hit.triangleIndex * 3 >= submesh.indexStart &&
                    hit.triangleIndex * 3 < submesh.indexStart + submesh.indexCount)
                {
                    if (i < hitRenderer.sharedMaterials.Length)
                        return hitRenderer.sharedMaterials[i];
                }
            }

            if (hitRenderer.sharedMaterials.Length > 0)
                return hitRenderer.sharedMaterials[0];
        }
        else if (hitRenderer.sharedMaterial != null)
        {
            return hitRenderer.sharedMaterial;
        }

        return null;
    }

    private Texture GetTextureFromMaterial(Material mat)
    {
        if (mat == null) return null;

        if (mat.HasProperty("_BaseMap"))
            return mat.GetTexture("_BaseMap");

        if (mat.HasProperty("_MainTex"))
            return mat.GetTexture("_MainTex");

        return null;
    }

    void OnDrawGizmos()
    {
        if (raycastOrigin != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(raycastOrigin.position, Vector3.down * raycastDistance);
        }
    }
}