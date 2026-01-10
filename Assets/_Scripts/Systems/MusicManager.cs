/*
using System.Collections;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public bool useDebug;

    public static MusicManager instance { get; private set; }

    public AudioData_Music musicAudioData;
    [SerializeField] bool playMusicOnStart;

    private EventInstance musicLevelInstance;
    //private EventInstance musicVictoryDefeatInstance;

    private string sceneName;

    private PLAYBACK_STATE musicPlayBackState;
    private bool isPlaying = false;
    private bool isFadingOut = false;

    public bool IsPlaying { get => isPlaying; }

    private void Awake()
    {
        if (instance != null)
        {
            UseDebug("Found more than one Music Manager in the scene.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        isPlaying = false;
    }

    private void Start()
    {
        CheckIfSceneChange();

        if (playMusicOnStart && string.IsNullOrEmpty(sceneName))
        {
            SetMusicEventForScene();
            UseDebug("Play On Start called");
        }

        UseDebug($"Music is playing: {isPlaying}");
    }

    public void InitializeMusic(EventReference musicReference)
    {
        musicLevelInstance = AudioM.instance.CreateEventInstance(musicReference);
        musicLevelInstance.start();

        isPlaying = true;
        UseDebug("Music Initialized and PLAYING");
    }

    public void PlayMusic()
    {
        musicLevelInstance.getPlaybackState(out musicPlayBackState);
        UseDebug("Music Playback State: " + musicPlayBackState);

        if (musicPlayBackState != PLAYBACK_STATE.PLAYING)
        {
            SetMusicEventForScene();
        }
        else
        {
            Debug.LogWarning("Music instance already playing");
        }
    }

    public void StopMusic()
    {
        musicLevelInstance.getPlaybackState(out musicPlayBackState);
        UseDebug("Music Playback State: " + musicPlayBackState);

        if (musicPlayBackState != PLAYBACK_STATE.PLAYING)
        {
            musicLevelInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicLevelInstance.release();
        }
        else
        {
            Debug.LogWarning("No music instance playing");
        }
    }

    public void SetMusicEventForScene()
    {
        sceneName = SceneManager.GetActiveScene().name;
        UseDebug("Current scene name: " + sceneName);

        switch (sceneName)
        {
            case "Scene_Battle_Skeletons":
                InitializeMusic(FMODMusicEvents.skeletonLevelMusic);
                break;
            case "Scene_Battle_Dragon":
                InitializeMusic(FMODMusicEvents.dragonLevelMusic);
                break;
            case "Scene_Game_Menu":
                InitializeMusic(FMODMusicEvents.skeletonLevelMusic);
                break;
            default:
                Debug.LogWarning("Music returning, check if scene names have changed");
                return;
        }
    }

    public void FadeOutMusicOnSceneLoad()
    {
        musicLevelInstance.getPlaybackState(out musicPlayBackState);

        if (musicPlayBackState == PLAYBACK_STATE.PLAYING && isPlaying)
        {
            StartCoroutine(FadeOutCoroutine());
        }
        else
        {
            Debug.LogWarning("musicPlayBackState is not playing anything");
            return;
        }
    }

    private IEnumerator FadeOutCoroutine()
    {
        isFadingOut = true;
        EventInstance musicFadeOutSnapshot = FMODAudioManager.instance.CreateEventInstance(FMODAudioManager.instance.FMODSnapshots.MusicFadeOut);
        musicFadeOutSnapshot.start();

        float fadeDuration = 5.0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        musicLevelInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicLevelInstance.release();
        isPlaying = false;

        musicFadeOutSnapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicFadeOutSnapshot.release();

        isFadingOut = false;
    }

    public void CheckIfSceneChange()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Scene_Game_Menu")
        {
            SetMusicEventForScene();
        }
    }

    private void UseDebug(string debugMessage)
    {
        if (useDebug)
        {
            Debug.Log(debugMessage);
        }
    }
}
*/