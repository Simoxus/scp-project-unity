using UnityEngine;

public class UIDebugPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private UIAccess uiAccess;

    void Awake()
    {
        if (player == null)
        {
            player = Player.Instance;
        }

        if (uiAccess == null)
        {
            uiAccess = UIAccess.Instance;
        }

        uiAccess.canvasDebuggers.SetActive(false);
    }

    void OnEnable()
    {
        player.playerInputs.OnDebugUI += ToggleDebugMenu;
    }

    void OnDisable()
    {
        player.playerInputs.OnDebugUI -= ToggleDebugMenu;

        if (GameManager.Instance != null && GameManager.Instance.HasPauseRequest(this))
        {
            GameManager.Instance.ReleasePause(this);
        }
    }

    public void ToggleDebugMenu()
    {
        bool canvasIsActive = uiAccess.canvasDebuggers.activeSelf;

        if (!canvasIsActive)
        {
            OpenDebugMenu();
        }
        else
        {
            CloseDebugMenu();
        }
    }

    private void OpenDebugMenu()
    {
        uiAccess.canvasDebuggers.SetActive(true);
        uiAccess.consoleUI.FocusOnInput();
        
        GameManager.Instance.RequestPause(this);
    }

    private void CloseDebugMenu()
    {
        uiAccess.canvasDebuggers.SetActive(false);

        GameManager.Instance.ReleasePause(this);
    }

    public void ForceClose()
    {
        if (uiAccess.canvasDebuggers.activeSelf)
        {
            CloseDebugMenu();
        }
    }
}