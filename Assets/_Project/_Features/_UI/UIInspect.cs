using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIInspect : MonoBehaviour
{
    [Space]
    public Image image;
    [SerializeField] private GameObject inspectPanel;

    private bool isInspecting = false;

    private void Awake()
    {
        if (inspectPanel != null)
        {
            inspectPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isInspecting)
        {
            // Check for right-click or ESC to close
            if ((Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                Hide();
            }
        }
    }

    public void ShowDocument(Sprite documentSprite)
    {
        if (image == null)
        {
            Debug.LogError("Image component not assigned!");
            return;
        }

        // Set the sprite
        image.sprite = documentSprite;

        // Close inventory first
        if (Core.UI.Inventory != null)
        {
            Core.UI.Inventory.Hide();
        }

        // Show the panel
        if (inspectPanel != null)
        {
            inspectPanel.SetActive(true);
        }

        isInspecting = true;

        if (Core.GameManager != null)
        {
            Core.GameManager.RequestCursorControl(this);
            Core.GameManager.SetCursorState(this, visible: false, CursorLockMode.Locked);
        }
    }

    public void Hide()
    {
        if (inspectPanel != null)
        {
            inspectPanel.SetActive(false);
        }

        isInspecting = false;

        // Release controls and cursor - this will restore normal state
        if (Core.GameManager != null)
        {
            Core.GameManager.RequestDisableControls(this, shouldDisable: false);
            Core.GameManager.ReleaseCursorControl(this);
        }
    }
}