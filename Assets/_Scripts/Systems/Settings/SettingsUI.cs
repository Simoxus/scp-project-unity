using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FMODUnity;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string tabName;
        public GameObject panel;    // panel this button controls
        public Button button;       // existing button in scene
        public TMP_Text buttonText;
        public Outline buttonOutline;
    }

    [SerializeField] private EventReference uiPressEvent;

    [SerializeField] private List<Tab> tabs = new List<Tab>();

    private int activeTab = -1;

    private void Awake()
    {
        SetupTabs();
    }

    private void SetupTabs()
    {
        // Hook up all buttons
        for (int i = 0; i < tabs.Count; i++)
        {
            // Hide all panels by default
            if (tabs[i].panel != null)
                tabs[i].panel.SetActive(false);

            // Ensure Outline is initially disabled
            if (tabs[i].buttonOutline != null)
                tabs[i].buttonOutline.enabled = false;

            int capturedIndex = i;
            tabs[i].button.onClick.AddListener(() => ShowTab(capturedIndex));
        }

        // Show the first tab automatically
        if (tabs.Count > 0)
            ShowTab(0, settingUp: true);
    }

    public void ShowTab(int index, bool settingUp = false)
    {
        if (index < 0 || index >= tabs.Count) return;

        for (int i = 0; i < tabs.Count; i++)
        {
            bool isActiveTab = i == index;

            if (tabs[i].panel != null)
                tabs[i].panel.SetActive(i == index);

            if (tabs[i].buttonText != null)
            {
                // Toggle bold style
                if (isActiveTab)
                {
                    // This assumes the font has a bold variant
                    tabs[i].buttonText.fontStyle = FontStyles.Bold;
                }
                else
                {
                    tabs[i].buttonText.fontStyle = FontStyles.Normal;
                }
            }

            if (tabs[i].buttonOutline != null)
            {
                // Enable outline for the active tab, disable for others
                tabs[i].buttonOutline.enabled = isActiveTab;
            }
        }

        if (!settingUp) { FMODHelper.PlayOneShot(uiPressEvent); }

        activeTab = index;
    }
}
