using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(LensFlare))]
public class LightEntity : MonoBehaviour
{
    public RMeshData room;
    public Color color;
    public float range;
    public float intensity;
    public float innerSpotAngle;
    public float spotAngle;
    public LightType type;
    public LensFlare flare;

    public void RefreshData()
    {
        Light src = GetComponent<Light>();
        src.type = type;
        src.range = range * RMeshLoader.Scale;
        src.intensity = intensity;// * 0.8f;
        src.innerSpotAngle = innerSpotAngle;
        src.spotAngle = spotAngle;
        src.color = color;
        flare = GetComponent<LensFlare>();
        flare.brightness = intensity;
        flare.color = color;
        flare.flare = Resources.Load<Flare>("LightFlare");
    }

    public void OnEnable()
    {
        flare = GetComponent<LensFlare>();
    }

    public void Update()
    {
        flare.brightness = Random.Range(intensity * 0.9f, intensity * 1.1f);
    }
}