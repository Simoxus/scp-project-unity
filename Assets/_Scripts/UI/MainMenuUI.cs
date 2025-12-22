using FMODUnity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class MainMenuUI : MonoBehaviour
{
    [System.Serializable]
    public class Submenu
    {
        public string panelName;
        public GameObject panel;
        public CanvasGroup panelCanvasGroup;
        public Button buttonTo;
        public Button buttonBack;
    }

    [Header("References")]
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private GameObject mainButtonsHolder;
    [SerializeField] private List<Submenu> submenus = new List<Submenu>();
    [SerializeField] private EventReference uiPressEvent;

    private void Awake()
    {
        for (int i = 0; i < submenus.Count; i++)
        {
            int capturedIndex = i;

            submenus[i].buttonTo.onClick.AddListener(() => GoTo(capturedIndex));
            submenus[i].buttonBack.onClick.AddListener(() => GoBack(capturedIndex));
        }
    }

    private void ShowPanel(CanvasGroup cg)
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void HidePanel(CanvasGroup cg)
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    private void GoTo(int index)
    {
        FMODHelper.PlayOneShot(uiPressEvent);

        mainButtonsHolder.SetActive(false);
        ShowPanel(submenus[index].panelCanvasGroup);
    }

    private void GoBack(int index)
    {
        FMODHelper.PlayOneShot(uiPressEvent);

        HidePanel(submenus[index].panelCanvasGroup);
        mainButtonsHolder.SetActive(true);
    }

    public void QuitGame()
    {
        if (SettingsManager.Instance)
        {
            SettingsManager.Instance.Save();
        }

        Application.Quit();
    }
}
