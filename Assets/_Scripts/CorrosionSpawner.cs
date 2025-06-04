/*
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Cysharp.Threading.Tasks;
using PrimeTween;

public class CorrosionSpawner : MonoBehaviour
{
    [Header("Decal Settings")]
    [SerializeField] private DecalProjector decalPrefab;
    [SerializeField] private float targetSize = 30f;           // Target width & height
    [SerializeField] private float spawnDuration = 1f;         // Tween duration in seconds
    [SerializeField] private bool destroyAfterSpawn = false;   // Destroy after lifetime
    [SerializeField] private float lifetime = 5f;              // Lifetime in seconds

    public static CorrosionSpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public static UniTask SpawnAsync(Vector3 position, bool spin = false)
    {
        if (Instance == null)
        {
            Debug.LogError("CorrosionDecalSpawner: No instance found in scene.");
            return UniTask.CompletedTask;
        }
        return Instance.SpawnInternalAsync(position, spin);
    }

    private async UniTask SpawnInternalAsync(Vector3 position, bool spin)
    {
        // Base rotation to face downward (X = 90 degrees)
        Quaternion baseRotation = Quaternion.Euler(90f, 0f, 0f);

        // Optional random spin around Y axis
        Quaternion spinRotation = spin ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Quaternion.identity;

        // Final rotation combines facing downward + spin
        Quaternion finalRotation = baseRotation * spinRotation;

        // Instantiate decal prefab at position and rotation
        DecalProjector decal = Instantiate(decalPrefab, position, finalRotation);

        // Cache initial depth (Z) size so we keep it stable
        float initialZ = decal.size.z;

        // Immediately set decal size to zero width/height, keep depth same
        decal.size = new Vector3(0f, 0f, initialZ);

        // Tween size from zero to target size (width & height), keep depth fixed
        Tween sizeTween = Tween.Custom(
            new Vector3(0f, 0f, initialZ),
            new Vector3(targetSize, targetSize, initialZ),
            spawnDuration,
            onValueChange: val => decal.size = val,
            ease: Ease.OutCubic
        );

        // Optional spin tween: spin 360 degrees around Y during spawnDuration
        Tween rotationTween = default;
        if (spin)
        {
            rotationTween = Tween.LocalRotation(
                decal.transform,
                finalRotation * Quaternion.Euler(0f, 360f, 0f),
                spawnDuration,
                Ease.OutCubic
            );
        }

        // Wait for all tweens to finish
        await UniTask.WhenAll(
            sizeTween.ToUniTask(),
            spin ? rotationTween.ToUniTask() : UniTask.CompletedTask
        );

        // Destroy decal after lifetime if requested
        if (destroyAfterSpawn)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(lifetime));
            Destroy(decal.gameObject);
        }
    }
}
*/
using UnityEngine;

public class CorrosionSpawner : MonoBehaviour
{

}