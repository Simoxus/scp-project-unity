using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("References")]
    public Player player;

    [Header("Behavior Settings")]
    public List<FootstepData> footstepSurfaces = new List<FootstepData>();
    public bool useBobSyncedFootsteps = true;

    [Header("FMOD Setup")]
    public Transform footstepPlayTransform;
    public EventReference walkFootstepEvent;
    public EventReference runFootstepEvent;
    public string surfaceParameterName = "Surface";

    [Header("Raycast Settings")]
    public Transform raycastOrigin;
    public float raycastDistance = 4f;
    public LayerMask groundLayer;

    private Dictionary<string, FootstepData> _surfaceByTextureName = new Dictionary<string, FootstepData>();
    private FootstepData _currentFootstepData;

    private void Awake()
    {
        player = player != null ? player : Player.Instance;

        if (raycastOrigin == null)
        {
            raycastOrigin = transform;
        }

        if (player == null)
        {
            Log.VerboseWarning("Player could not be found. Footsteps may not work.");
        }

        BuildSurfaceDictionary();
    }

    private void OnEnable()
    {
        if (useBobSyncedFootsteps && player && player.playerBobbing != null)
        {
            player.playerBobbing.OnFootstepTrigger += PlayFootstepAudio;
        }
    }

    private void OnDisable()
    {
        if (player && player.playerBobbing != null)
        {
            player.playerBobbing.OnFootstepTrigger -= PlayFootstepAudio;
        }
    }

    public void PlayFootstepAudio()
    {
        if (player == null || player.playerController == null) return;
        if (!player.playerController.isMoving) return;

        DetectGroundSurface();

        if (_currentFootstepData == null) return;

        EventReference footstepEvent = GetFootstepEventForState(player.currentState);

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
                return runFootstepEvent;
            case PlayerState.Walking:
            case PlayerState.Crouching:
            case PlayerState.Idle:
            case PlayerState.Freefall:
            default:
                return walkFootstepEvent;
        }
    }

    public void DetectGroundSurface()
    {
        if (!Physics.Raycast(raycastOrigin.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            Debug.DrawRay(raycastOrigin.position, Vector3.down * raycastDistance, Color.red, 0.1f);
            _currentFootstepData = null;
            return;
        }

        Debug.DrawRay(raycastOrigin.position, Vector3.down * raycastDistance, Color.green, 0.1f);

        Material detectedMaterial = GetMaterialFromHit(hit);
        if (detectedMaterial == null)
        {
            _currentFootstepData = null;
            return;
        }

        Texture detectedTexture = GetTextureFromMaterial(detectedMaterial);
        if (detectedTexture == null)
        {
            _currentFootstepData = null;
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

        _currentFootstepData = null;
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