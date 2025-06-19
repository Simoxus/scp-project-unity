using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using System.Collections.Generic;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("FMOD")]
    public StudioEventEmitter footstepEmitter;

    [Header("References")]
    public CharacterController characterController;

    [Header("Raycast Settings")]
    public Vector3 raycastOriginOffset = Vector3.zero;
    public Vector3 raycastDirection = Vector3.down;
    public float raycastDistance = 2.0f;

    [System.Serializable]
    public class TextureToSurface
    {
        public List<Texture> textures = new List<Texture>();
        public SurfaceType surfaceType;
        public override string ToString() => surfaceType.ToString();
    }

    public enum SurfaceType
    {
        Tile,
        Metal,
        Carpet,
        Grass,
        PD
        // Add more as needed
    }

    [Header("Texture to Surface Mappings")]
    public List<TextureToSurface> footstepMappings = new List<TextureToSurface>();

    private float footstepTimer = 0f;
    private float footstepInterval = 0.7f;
    private bool isMoving = false;
    private bool isSprinting = false;

    public void Initialize(CharacterController controller)
    {
        characterController = controller;
    }

    public void UpdateFootsteps(bool moving, bool sprinting)
    {
        isMoving = moving;
        isSprinting = sprinting;

        footstepInterval = isSprinting ? 0.5f : 0.7f;
        footstepTimer += Time.deltaTime;

        if (footstepTimer >= footstepInterval)
        {
            PlayFootstepAudio();
            footstepTimer = 0f;
        }
    }

    private void PlayFootstepAudio()
    {
        if (footstepEmitter == null) return;

        Texture texture = GetCurrentTextureUnderPlayer();
        if (texture == null) return;

        SurfaceType surface = GetSurfaceTypeForTexture(texture);

        // Set parameters on the emitter
        Debug.Log($"Setting Surface parameter to: {(int)surface} ({surface})");
        footstepEmitter.SetParameter("Surface", (float)surface);
        footstepEmitter.SetParameter("Speed", isSprinting ? 1f : 0f);
        footstepEmitter.Play();
    }

    private Texture GetCurrentTextureUnderPlayer()
    {
        Vector3 rayOrigin = transform.position + raycastOriginOffset;
        if (Physics.Raycast(rayOrigin, raycastDirection.normalized, out RaycastHit hit, raycastDistance))
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null)
            {
                int submeshIndex = GetSubmeshIndex(meshCollider.sharedMesh, hit.triangleIndex);
                if (submeshIndex != -1)
                {
                    Material material = meshCollider.GetComponent<Renderer>()?.materials[submeshIndex];
                    return material?.mainTexture;
                }
            }
        }
        return null;
    }

    private int GetSubmeshIndex(Mesh mesh, int triangleIndex)
    {
        int triangleCount = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int subMeshTriangleCount = mesh.GetTriangles(i).Length / 3;
            if (triangleIndex < triangleCount + subMeshTriangleCount)
                return i;
            triangleCount += subMeshTriangleCount;
        }
        return -1;
    }

    private SurfaceType GetSurfaceTypeForTexture(Texture texture)
    {
        foreach (var mapping in footstepMappings)
        {
            if (mapping.textures.Contains(texture))
                return mapping.surfaceType;
        }
        return SurfaceType.Tile; // Fallback
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 rayOrigin = transform.position + raycastOriginOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + raycastDirection.normalized * raycastDistance);
        Gizmos.DrawSphere(rayOrigin + raycastDirection.normalized * raycastDistance, 0.05f);
    }
}
