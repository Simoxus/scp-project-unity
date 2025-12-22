using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [System.Serializable]
    public class Submenu
    {
        public string panelName;
        public GameObject panel;
        public CanvasGroup panelCanvasGroup;
        public Button buttonMainPanel;
        public Button buttonBack;
    }

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private CanvasGroup mainPanelCanvasGroup;
    [SerializeField] private Button exitButton;
    [SerializeField] private List<Submenu> submenus = new List<Submenu>();
    [SerializeField] private EventReference uiPressEvent;

    private PlayerInputs _inputs;

    private void Awake()
    {
        if (player != null)
            _inputs = player.playerInputs;

        exitButton.onClick.AddListener(() => ClosePauseMenu(true));

        for (int i = 0; i < submenus.Count; i++)
        {
            int capturedIndex = i;

            submenus[i].buttonMainPanel.onClick.AddListener(() => GoTo(capturedIndex));
            submenus[i].buttonBack.onClick.AddListener(() => GoBack(capturedIndex));
        }

        mainPanel.SetActive(true);
        foreach (var tab in submenus)
        {
            tab.panel.SetActive(true);
        }

        // Initialize all panels as hidden (menu starts closed)
        HideAllPanels();
    }

    private bool IsMenuOpen()
    {
        if (mainPanelCanvasGroup.alpha > 0) return true;

        foreach (var submenu in submenus)
            if (submenu.panelCanvasGroup.alpha > 0) return true;

        return false;
    }

    private void OnEnable()
    {
        _inputs.OnPauseUI += TogglePauseMenu;
    }

    private void OnDisable()
    {
        _inputs.OnPauseUI -= TogglePauseMenu;

        if (GameManager.Instance != null && GameManager.Instance.HasPauseRequest(this))
        {
            GameManager.Instance.ReleasePause(this);
            HideAllPanels();
        }
    }

    public void ShowPanel(CanvasGroup cg)
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public void HidePanel(CanvasGroup cg)
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    private void HideAllPanels()
    {
        HidePanel(mainPanelCanvasGroup);
        foreach (var tab in submenus)
            HidePanel(tab.panelCanvasGroup);
    }

    public void TogglePauseMenu()
    {
        bool menuOpen = IsMenuOpen();

        if (menuOpen)
        {
            ClosePauseMenu(playSound: false);
        }
        else
        {
            OpenPauseMenu();
        }
    }

    private void OpenPauseMenu()
    {
        ShowPanel(mainPanelCanvasGroup);
        foreach (var tab in submenus)
            HidePanel(tab.panelCanvasGroup);

        GameManager.Instance.RequestPause(this);
    }

    private void ClosePauseMenu(bool playSound = false)
    {
        if (playSound)
            FMODHelper.PlayOneShot(uiPressEvent);

        HideAllPanels();

        GameManager.Instance.ReleasePause(this);
    }

    public void GoTo(int index)
    {
        FMODHelper.PlayOneShot(uiPressEvent);

        HidePanel(mainPanelCanvasGroup);
        ShowPanel(submenus[index].panelCanvasGroup);
    }

    public void GoBack(int index)
    {
        FMODHelper.PlayOneShot(uiPressEvent);

        HidePanel(submenus[index].panelCanvasGroup);
        ShowPanel(mainPanelCanvasGroup);
    }

    public void ForceClose()
    {
        if (IsMenuOpen())
        {
            ClosePauseMenu(playSound: false);
        }
    }
}