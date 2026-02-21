using Cysharp.Threading.Tasks;
using FMODUnity;
using PrimeTween;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private Transform mainButtonsHolder;
    [SerializeField] private Submenu[] submenus;
    [SerializeField] private EventReference uiPressEvent;

    [Header("New Game Settings")]
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private Button startGameButton;

    [Header("Other Buttons")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button moddingButton;
    [SerializeField] private Button quitButton;

    [Serializable]
    public class Submenu
    {
        public string panelName;
        public GameObject panel;
        public CanvasGroup panelCanvasGroup;
        public Button buttonTo;
        public Button buttonBack;
    }

    private const string MAP_GEN_SCENE = "Facility_MapGen";
    private int _currentSubmenuIndex = -1;
    private bool _isTransitioning = false;
    private CanvasGroup _mainButtonsCanvasGroup;

    private void Awake()
    {
        Debug.Log("[MainMenuUI] Awake started");
        
        if (versionText != null)
        {
            versionText.text = $"v{Application.version}";
        }

        // Always try to find Buttons holder by name first to handle broken references
        Transform foundButtons = transform.Find("Buttons");
        if (foundButtons == null)
        {
            // Try recursive search
            foundButtons = FindChildRecursive(transform, "Buttons");
        }
        
        if (foundButtons != null)
        {
            mainButtonsHolder = foundButtons;
            Debug.Log($"[MainMenuUI] Found Buttons holder: {mainButtonsHolder.name}");
        }
        else if (mainButtonsHolder != null && mainButtonsHolder)
        {
            // Use serialized reference only if it's valid
            Debug.Log($"[MainMenuUI] Using serialized Buttons holder: {mainButtonsHolder.name}");
        }
        else
        {
            Debug.LogError("[MainMenuUI] Could not find Buttons holder! Menu will not work.");
            return;
        }

        // Ensure main buttons holder has a CanvasGroup
        _mainButtonsCanvasGroup = mainButtonsHolder.GetComponent<CanvasGroup>();
        if (_mainButtonsCanvasGroup == null)
        {
            _mainButtonsCanvasGroup = mainButtonsHolder.gameObject.AddComponent<CanvasGroup>();
        }

        InitializeSubmenus();
        InitializeButtons();
        
        Debug.Log("[MainMenuUI] Awake completed successfully");
    }

    private void InitializeButtons()
    {
        Debug.Log("[MainMenuUI] InitializeButtons started");
        
        // Auto-find buttons if not assigned
        FindButtonsIfNeeded();

        // Wire up start game button if assigned
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(() => StartNewGame());
            Debug.Log("[MainMenuUI] Wired up Start Game button");
        }

        // Wire up load game button if assigned
        if (loadGameButton != null)
        {
            loadGameButton.onClick.AddListener(OpenLoadGame);
            Debug.Log("[MainMenuUI] Wired up Load Game button");
        }

        // Wire up modding button if assigned
        if (moddingButton != null)
        {
            moddingButton.onClick.AddListener(OpenModsFolder);
            Debug.Log("[MainMenuUI] Wired up Modding button");
        }

        // Wire up quit button if assigned
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            Debug.Log("[MainMenuUI] Wired up Quit button");
        }
    }

    private void FindButtonsIfNeeded()
    {
        if (mainButtonsHolder == null) 
        {
            Debug.LogWarning("[MainMenuUI] FindButtonsIfNeeded: mainButtonsHolder is null!");
            return;
        }

        Debug.Log($"[MainMenuUI] FindButtonsIfNeeded: Searching in {mainButtonsHolder.name} with {mainButtonsHolder.childCount} children");

        // Find LoadGame button
        if (loadGameButton == null || !loadGameButton)
        {
            var loadGameTransform = mainButtonsHolder.Find("LoadGame");
            if (loadGameTransform != null)
            {
                loadGameButton = loadGameTransform.GetComponent<Button>();
                Debug.Log($"[MainMenuUI] Found LoadGame button: {loadGameButton != null}");
            }
        }

        // Find Modding button
        if (moddingButton == null || !moddingButton)
        {
            var moddingTransform = mainButtonsHolder.Find("Modding");
            if (moddingTransform != null)
            {
                moddingButton = moddingTransform.GetComponent<Button>();
                Debug.Log($"[MainMenuUI] Found Modding button: {moddingButton != null}");
            }
        }

        // Find Quit button
        if (quitButton == null || !quitButton)
        {
            var quitTransform = mainButtonsHolder.Find("Quit");
            if (quitTransform != null)
            {
                quitButton = quitTransform.GetComponent<Button>();
                Debug.Log($"[MainMenuUI] Found Quit button: {quitButton != null}");
            }
        }
    }

    private void InitializeSubmenus()
    {
        Debug.Log($"[MainMenuUI] InitializeSubmenus: {submenus?.Length ?? 0} submenus");
        
        for (int i = 0; i < submenus.Length; i++)
        {
            int index = i;
            var submenu = submenus[i];

            if (submenu.panelCanvasGroup != null)
            {
                submenu.panelCanvasGroup.alpha = 0f;
                submenu.panelCanvasGroup.blocksRaycasts = false;
            }

            // Special handling for "New Game" submenu - start the game directly instead of opening panel
            if (submenu.panelName == "New Game" && submenu.buttonTo != null)
            {
                Debug.Log("[MainMenuUI] Found 'New Game' submenu - wiring button to start game directly");
                submenu.buttonTo.onClick.AddListener(() => StartNewGame());
                continue; // Skip normal submenu behavior
            }

            if (submenu.buttonTo != null)
            {
                submenu.buttonTo.onClick.AddListener(() => OpenSubmenu(index));
            }

            if (submenu.buttonBack != null)
            {
                submenu.buttonBack.onClick.AddListener(CloseCurrentSubmenu);
            }
        }
    }

    public void OpenSubmenu(int index)
    {
        if (_isTransitioning || index < 0 || index >= submenus.Length) return;
        if (_currentSubmenuIndex == index) return;

        _isTransitioning = true;
        PlayUISound();

        // Hide main buttons
        if (_mainButtonsCanvasGroup != null)
        {
            Tween.Alpha(_mainButtonsCanvasGroup, 0f, 0.2f);
            _mainButtonsCanvasGroup.blocksRaycasts = false;
        }

        // Show submenu
        var submenu = submenus[index];
        if (submenu.panelCanvasGroup != null)
        {
            Tween.Alpha(submenu.panelCanvasGroup, 1f, 0.2f).OnComplete(() =>
            {
                submenu.panelCanvasGroup.blocksRaycasts = true;
                _isTransitioning = false;
            });
        }
        else
        {
            _isTransitioning = false;
        }

        _currentSubmenuIndex = index;
    }

    public void CloseCurrentSubmenu()
    {
        if (_isTransitioning || _currentSubmenuIndex < 0) return;

        _isTransitioning = true;
        PlayUISound();

        // Hide current submenu
        var submenu = submenus[_currentSubmenuIndex];
        if (submenu.panelCanvasGroup != null)
        {
            submenu.panelCanvasGroup.blocksRaycasts = false;
            Tween.Alpha(submenu.panelCanvasGroup, 0f, 0.2f);
        }

        // Show main buttons
        if (_mainButtonsCanvasGroup != null)
        {
            Tween.Alpha(_mainButtonsCanvasGroup, 1f, 0.2f).OnComplete(() =>
            {
                _mainButtonsCanvasGroup.blocksRaycasts = true;
                _isTransitioning = false;
            });
        }
        else
        {
            _isTransitioning = false;
        }

        _currentSubmenuIndex = -1;
    }

    /// <summary>
    /// Starts a new game by loading the map generation scene.
    /// Optionally uses a seed from the input field.
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("[MainMenuUI] StartNewGame called!");
        PlayUISound();
        StartNewGameAsync().Forget();
    }

    private async UniTaskVoid StartNewGameAsync()
    {
        Debug.Log("[MainMenuUI] StartNewGameAsync - loading scene...");
        
        // Set seed if input field has value
        if (seedInputField != null && !string.IsNullOrWhiteSpace(seedInputField.text))
        {
            // Store seed for FacilityGenerator to use
            PlayerPrefs.SetString("MapSeed", seedInputField.text.Trim());
            PlayerPrefs.Save();
            Debug.Log($"[MainMenuUI] Set seed: {seedInputField.text.Trim()}");
        }
        else
        {
            // Clear any existing seed so random one is used
            PlayerPrefs.DeleteKey("MapSeed");
            Debug.Log("[MainMenuUI] Using random seed");
        }

        // Load the map generation scene
        if (Core.LoadingManager != null)
        {
            Debug.Log($"[MainMenuUI] Loading scene via LoadingManager: {MAP_GEN_SCENE}");
            await Core.LoadingManager.LoadSceneAsync(MAP_GEN_SCENE);
        }
        else
        {
            // Fallback if LoadingManager not available
            Debug.Log($"[MainMenuUI] Loading scene directly: {MAP_GEN_SCENE}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(MAP_GEN_SCENE);
        }

        // Wait a frame for scene to fully initialize
        await UniTask.Yield();

        // Trigger facility generation since generateOnStart is disabled
        Debug.Log("[MainMenuUI] Scene loaded, triggering facility generation...");
        if (Core.FacilityGenerator != null)
        {
            Debug.Log("[MainMenuUI] Calling FacilityGenerator.GenerateFacilityAsync()");
            await Core.FacilityGenerator.GenerateFacilityAsync();
            Debug.Log("[MainMenuUI] Facility generation complete!");
        }
        else
        {
            Debug.LogError("[MainMenuUI] FacilityGenerator not found after scene load!");
        }
    }

    /// <summary>
    /// Opens the load game panel/functionality.
    /// </summary>
    public void OpenLoadGame()
    {
        PlayUISound();
        
        // TODO: Implement load game UI
        // For now, try to quick load if available
        QuickLoadAsync().Forget();
    }

    private async UniTaskVoid QuickLoadAsync()
    {
        if (Core.PersistenceManager != null)
        {
            try
            {
                await Core.PersistenceManager.QuickLoad();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to quick load: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Opens the mods folder in the file explorer.
    /// </summary>
    public void OpenModsFolder()
    {
        PlayUISound();

        try
        {
            string modsPath = null;
            
            if (Core.ModManager != null)
            {
                modsPath = Core.ModManager.GetModsFolderPath();
            }
            else
            {
                // Fallback: construct the path manually
                string gameDirectory = System.IO.Directory.GetParent(Application.dataPath).FullName;
                modsPath = System.IO.Path.Combine(gameDirectory, "Mods");
            }

            // Create directory if it doesn't exist
            if (!System.IO.Directory.Exists(modsPath))
            {
                System.IO.Directory.CreateDirectory(modsPath);
            }

            // Open in file explorer
            Application.OpenURL("file:///" + modsPath.Replace("\\", "/"));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to open mods folder: {e.Message}");
        }
    }

    public void QuitGame()
    {
        PlayUISound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayUISound()
    {
        if (!uiPressEvent.IsNull)
        {
            FMODHelper.PlayOneShot(uiPressEvent);
        }
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            
            var found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }
}
