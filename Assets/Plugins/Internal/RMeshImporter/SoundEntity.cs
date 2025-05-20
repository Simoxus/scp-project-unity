using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEntity : MonoBehaviour
{
    public RMeshData room;
    public int soundId;
    public float range;

    public void RefreshData()
    {
        AudioSource src = GetComponent<AudioSource>();
        src.dopplerLevel = 0f;
        src.loop = true;
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 0.75f * range;
        src.maxDistance = range * 3f;
        src.clip = null;//GameData.instance.roomAmbientAudio[soundId];
        src.Stop();
        src.Play();
    }
}