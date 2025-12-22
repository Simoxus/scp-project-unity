using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMODUnity;

public class QuitGameUI : MonoBehaviour
{
    [SerializeField] private Button saveQuitButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private EventReference uiPressEvent;

    private void Awake()
    {
        //saveQuitButton.onClick.AddListener(SaveAndQuit);
        quitButton.onClick.AddListener(Quit);
    }

    private void Quit()
    {
        FMODHelper.PlayOneShot(uiPressEvent);

        SceneManager.LoadScene("MainMenu");
    }
}
