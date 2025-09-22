using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("References")]
    public Player player;

    [Header("FMOD Setup")]
    public StudioEventEmitter footstepEventEmitter;
    public string surfaceParameterName = "Surface";

    [Header("Raycast Settings")]
    public Transform raycastOrigin;
    public float raycastDistance;
    public LayerMask groundLayer;
  
    public List<FootstepData> footstepSurfaces = new List<FootstepData>();

    [Header("Footstep Cooldown")]
    public float footstepCooldown = 0.3f; // Default cooldown, can be adjusted in Inspector

    private Dictionary<string, FootstepData> _surfaceDefinitionsByTextureID = new Dictionary<string, FootstepData>();
    private string _currentFootstepParameterLabel;
    private float _lastFootstepTime; // To track when the last footstep played

    private void Awake()
    {
        _surfaceDefinitionsByTextureID.Clear();

        foreach (FootstepData surface in footstepSurfaces)
        {
            if (surface == null) { Debug.LogWarning("FootstepSurface reference is null. Please check your setup.", this); continue; }

            foreach (Texture texture in surface.textures)
            {
                if (texture != null)
                {
                    string identifier = texture.name;

                    if (!string.IsNullOrEmpty(identifier))
                    {
                        if (!_surfaceDefinitionsByTextureID.ContainsKey(identifier))
                        {
                            _surfaceDefinitionsByTextureID.Add(identifier, surface);
                        }
                    }
                }
            }
        }  

        _lastFootstepTime = -footstepCooldown; // Initialize to allow immediate footstep
    }

    private void Start()
    {
        player = player != null ? player : Player.Instance;
    }

    // Removed Update() from PlayerFootsteps, as PlayerController will now drive it.

    public void RequestFootstep()
    {
        // Check cooldown/values
        if (!player.playerController.isMoving) return;
        if (Time.time < _lastFootstepTime + footstepCooldown) { return; }

        // If not on cooldown, proceed to detect and play
        DetectGroundSurface();

        if (footstepEventEmitter == null || footstepEventEmitter.EventReference.IsNull)
        {
            Debug.LogWarning("Footstep emitter is not set in the inspector. No sound will play.", this);
            return;
        }

        // Get the FootstepData associated with the current label
        FootstepData currentFootstepData = null;
        foreach (var entry in _surfaceDefinitionsByTextureID)
        {
            if (entry.Value.fmodParameterLabel == _currentFootstepParameterLabel)
            {
                currentFootstepData = entry.Value;
                break;
            }
        }

        if (currentFootstepData != null)
        {
            EventReference footstepReference = footstepEventEmitter.EventReference;

            FMODHelper.PlayOneShotWithParameters(
                footstepReference, // Use the path from EventReference
                footstepEventEmitter.gameObject.transform.position,
                (surfaceParameterName, currentFootstepData.fmodParameterValue) // Use the float value from FootstepData
            );

            _lastFootstepTime = Time.time;
        }
    }


    public void DetectGroundSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(raycastOrigin.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            Debug.DrawRay(raycastOrigin.position, Vector3.down * raycastDistance, Color.green, 0.1f);

            Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
            MeshCollider hitMeshCollider = hit.collider as MeshCollider;

            Material detectedMaterial = null;

            if (hitMeshCollider != null && hitRenderer != null && hitMeshCollider.sharedMesh != null && hitRenderer.sharedMaterials != null && hitRenderer.sharedMaterials.Length > 0)
            {
                // This is a MeshCollider with potentially multiple materials
                Mesh mesh = hitMeshCollider.sharedMesh;
                int submeshIndex = -1;

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    var submeshDescriptor = mesh.GetSubMesh(i);
                    if (hit.triangleIndex * 3 >= submeshDescriptor.indexStart &&
                        hit.triangleIndex * 3 < (submeshDescriptor.indexStart + submeshDescriptor.indexCount))
                    {
                        submeshIndex = i;
                        break;
                    }
                }

                if (submeshIndex != -1 && submeshIndex < hitRenderer.sharedMaterials.Length)
                {
                    detectedMaterial = hitRenderer.sharedMaterials[submeshIndex];
                }
                else if (hitRenderer.sharedMaterials.Length > 0)
                {
                    detectedMaterial = hitRenderer.sharedMaterials[0];
                }
            }
            else if (hitRenderer != null && hitRenderer.sharedMaterial != null)
            {
                // This is likely a primitive collider (Box, Sphere, Capsule)
                // or a simple MeshRenderer with a single material.
                detectedMaterial = hitRenderer.sharedMaterial;
            }
            // ELSE: No valid Renderer or no materials found on the hit object.

            if (detectedMaterial != null)
            {
                Texture detectedTexture = null;
                if (detectedMaterial.HasProperty("_BaseMap"))
                {
                    detectedTexture = detectedMaterial.GetTexture("_BaseMap");
                }
                else if (detectedMaterial.HasProperty("_MainTex"))
                {
                    detectedTexture = detectedMaterial.GetTexture("_MainTex");
                }

                if (detectedTexture != null)
                {
                    bool surfaceFound = false;
                    if (_surfaceDefinitionsByTextureID.TryGetValue(detectedTexture.name, out FootstepData foundSurfaceDef))
                    {
                        _currentFootstepParameterLabel = foundSurfaceDef.fmodParameterLabel;
                        surfaceFound = true;
                    }
                    else
                    {
                        foreach (var entry in _surfaceDefinitionsByTextureID)
                        {
                            string identifier = entry.Key;
                            FootstepData surfaceDef = entry.Value;

                            if (detectedTexture.name.Contains(identifier))
                            {
                                _currentFootstepParameterLabel = surfaceDef.fmodParameterLabel;
                                surfaceFound = true;
                               
                                break;
                            }
                        }
                    }

                    if (!surfaceFound)
                    {
                        _currentFootstepParameterLabel = "";
                    }
                }
                else
                {
                    _currentFootstepParameterLabel = "";
                }
            }
            else
            {
                _currentFootstepParameterLabel = "";
            }
        }
        else
        {
            Debug.DrawRay(raycastOrigin.position, Vector3.down * raycastDistance, Color.red, 0.1f);
            _currentFootstepParameterLabel = "";
        }
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