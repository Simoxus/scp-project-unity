using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPauseMenu : MonoBehaviour
{
    [System.Serializable]
    public class Submenu
    {
        public string panelName;
        public CanvasGroup panelCanvasGroup;
        public Button buttonTo;
        public Button buttonBack;
    }

    [Space]
    public CanvasGroup canvasGroup;
    public CanvasGroup mainPanelCanvasGroup;
    public List<Submenu> submenus = new List<Submenu>();
    public Button exitButton;

    private PlayerInputs _inputs;
    private bool _isVisible;

    public bool IsVisible => _isVisible;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (Core.Player != null)
            _inputs = Core.Player.Inputs;

        InitializeSubmenuButtons();

        if (exitButton != null)
            exitButton.onClick.AddListener(() => Hide(true));

        // Ensure all panels start hidden
        HideAllPanels();
    }

    private void OnEnable()
    {
        if (_inputs != null)
            _inputs.OnPauseUI += Toggle;
    }

    private void OnDisable()
    {
        if (_inputs != null)
            _inputs.OnPauseUI -= Toggle;
        ReleasePauseIfNeeded();
    }

    public void Show()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        _isVisible = true;

        ShowMainPanel();
        if (Core.GameManager != null)
            Core.GameManager.RequestPause(this);
    }

    public void Hide()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        _isVisible = false;

        HideAllPanels();
        ReleasePauseIfNeeded();
    }

    public void Hide(bool playSound)
    {
        if (playSound)
            PlayButtonSound();
        Hide();
    }

    public void Toggle()
    {
        if (_isVisible)
            Hide();
        else
            Show();
    }

    public void ForceClose()
    {
        if (IsVisible)
            Hide();
    }

    private void InitializeSubmenuButtons()
    {
        for (int i = 0; i < submenus.Count; i++)
        {
            int capturedIndex = i;
            if (submenus[i].buttonTo != null)
                submenus[i].buttonTo.onClick.AddListener(() => NavigateToSubmenu(capturedIndex));
            if (submenus[i].buttonBack != null)
                submenus[i].buttonBack.onClick.AddListener(() => NavigateBack(capturedIndex));
        }
    }

    private void NavigateToSubmenu(int index)
    {
        PlayButtonSound();
        if (mainPanelCanvasGroup != null)
            HideCanvasGroup(mainPanelCanvasGroup);
        ShowCanvasGroup(submenus[index].panelCanvasGroup);
    }

    private void NavigateBack(int index)
    {
        PlayButtonSound();
        HideCanvasGroup(submenus[index].panelCanvasGroup);
        if (mainPanelCanvasGroup != null)
            ShowCanvasGroup(mainPanelCanvasGroup);
    }

    private void ShowMainPanel()
    {
        if (mainPanelCanvasGroup != null)
            ShowCanvasGroup(mainPanelCanvasGroup);
        HideAllSubmenus();
    }

    private void HideMainPanel()
    {
        if (mainPanelCanvasGroup != null)
            HideCanvasGroup(mainPanelCanvasGroup);
    }

    private void HideAllSubmenus()
    {
        foreach (var submenu in submenus)
            HideCanvasGroup(submenu.panelCanvasGroup);
    }

    private void HideAllPanels()
    {
        HideMainPanel();
        HideAllSubmenus();
    }

    private void ReleasePauseIfNeeded()
    {
        if (Core.GameManager != null && Core.GameManager.HasPauseRequest(this))
            Core.GameManager.ReleasePause(this);
    }

    private void PlayButtonSound()
    {
        if (Core.AudioDataAccess.UI != null)
            FMODHelper.PlayOneShot(Core.AudioDataAccess.UI.PressSound);
    }

    private static void ShowCanvasGroup(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private static void HideCanvasGroup(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}