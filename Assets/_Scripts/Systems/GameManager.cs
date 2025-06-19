using UnityEngine;
using FMODUnity;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("FMOD Pause Emitters")]
    public StudioEventEmitter[] studioEventEmitters; // This array contains which sounds you want to pause :)

    [Header("Global Values")]
    public bool gamePaused;
    public bool disablePlayerInputs;
    public bool hidePlayerHUD;

    [Header("Inherited QOL")]
    public bool inventoryPausesGame;
    public bool skipIntroSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PauseGame()
    {
        gamePaused = Time.timeScale == 0f;

        foreach (var emitter in studioEventEmitters)
        {
            var instance = emitter.EventInstance;
            
            if (instance.isValid()) // Skip invalid FMOD emitters
            {
                instance.setPaused(gamePaused);
            }
        }

        Time.timeScale = gamePaused ? 1.0f : 0f;
    }

    public void TogglePlayerInput(bool alsoToggleMouse)
    {
        disablePlayerInputs = !disablePlayerInputs;

        if (alsoToggleMouse == true)
        {
            if (disablePlayerInputs)
            {
                UpdateCursorVisiblity();
            }
            else
            {
                UpdateCursorVisiblity();
            }
        }
    }

    public void UpdateCursorVisiblity()
    {
        Cursor.lockState = disablePlayerInputs ? CursorLockMode.None : CursorLockMode.Locked; 
        Cursor.visible = disablePlayerInputs;
    }
}
