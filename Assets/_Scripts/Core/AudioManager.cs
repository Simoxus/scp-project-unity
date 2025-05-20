using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;
    private Dictionary<string, AudioClip> clipCache = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySFX(AudioClip sfx, float volumeScale)
    {
        if (sfx != null)
        {
            sfxSource.PlayOneShot(sfx, volumeScale);
        }
    }

    public void PlayAmbience(AudioClip music, bool loop = true)
    {
        musicSource.clip = music;
        musicSource.loop = loop;
        musicSource.Play();
    }
}
