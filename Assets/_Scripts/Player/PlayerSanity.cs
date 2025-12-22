using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSanity : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float currentSanity = 100f;
    public float maxSanity = 100f;
    public float minSanity = 0f;
    public float sanityThreshold = 30f;
    public float sanityChangeRate = 0f;
}