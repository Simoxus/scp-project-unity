using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq; // Required for .Distinct()

public class MaterialCopierWindow : EditorWindow
{
    // The GameObject from which materials were copied. This will be nullified if not locked and selection changes.
    private GameObject sourceGameObject;
    // An array to store the materials copied from the source GameObject's MeshRenderer.
    private Material[] copiedMaterials;
    // For scrolling if there are many materials.
    private Vector2 scrollPos;
    // Flag to indicate if the material list is locked to the current sourceGameObject.
    private bool isLocked = false;

    /// <summary>
    /// Opens the Material Copier window in the Unity Editor.
    /// This method is called from the Unity Editor menu.
    /// </summary>
    [MenuItem("Tools/Production Tools/Material Copier")]
    public static void ShowWindow()
    {
        // Get existing open window or create a new one.
        GetWindow<MaterialCopierWindow>("Material Copier").Show();
    }

    /// <summary>
    /// Called when the editor window is enabled.
    /// Used to subscribe to Unity's selection change event.
    /// </summary>
    private void OnEnable()
    {
        // Subscribe to selection changes so the window updates automatically.
        Selection.selectionChanged += OnSelectionChange;
        // Perform an initial update based on the current selection (if any).
        OnSelectionChange();
    }

    /// <summary>
    /// Called when the editor window is disabled.
    /// Used to unsubscribe from Unity's selection change event to prevent memory leaks.
    /// </summary>
    private void OnDisable()
    {
        // Unsubscribe from selection changes.
        Selection.selectionChanged -= OnSelectionChange;
    }

    /// <summary>
    /// This method is called by Unity whenever the selection in the editor changes.
    /// It updates the `sourceGameObject` and the `copiedMaterials` only if not locked.
    /// </summary>
    private void OnSelectionChange()
    {
        // Only update if the window is not locked and the active selection has changed.
        if (!isLocked && Selection.activeGameObject != sourceGameObject)
        {
            sourceGameObject = Selection.activeGameObject; // Update the source GameObject.
            UpdateMaterials(); // Refresh the list of materials.
            Repaint(); // Force the editor window to redraw its content.
        }
        else if (isLocked)
        {
            // If locked, just repaint to ensure the GUI accurately reflects the locked state
            // even if another object is selected in the editor.
            Repaint();
        }
    }

    /// <summary>
    /// Updates the `copiedMaterials` array based on the `sourceGameObject`'s MeshRenderer.
    /// </summary>
    private void UpdateMaterials()
    {
        copiedMaterials = null; // Clear any previously copied materials.

        if (sourceGameObject != null)
        {
            // Try to get the MeshRenderer component from the source GameObject.
            MeshRenderer meshRenderer = sourceGameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                // Copy the shared materials from the MeshRenderer.
                // Using .Distinct() to show each unique material only once, even if it's used multiple times on the same renderer.
                copiedMaterials = meshRenderer.sharedMaterials.Distinct().ToArray();
            }
        }
    }

    /// <summary>
    /// This method is called by Unity to draw the window's GUI.
    /// </summary>
    void OnGUI()
    {
        EditorGUILayout.LabelField("Material Copier", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal(); // Start horizontal group for selected object and lock button.

        // Display the source GameObject in a read-only ObjectField.
        EditorGUI.BeginDisabledGroup(true); // Disable editing for this field.
        string displayObjectName = isLocked ? $"{sourceGameObject?.name ?? "None"} (LOCKED)" : (sourceGameObject?.name ?? "None");
        EditorGUILayout.ObjectField("Source Object", sourceGameObject, typeof(GameObject), true);
        EditorGUI.EndDisabledGroup(); // Re-enable editing for subsequent fields.

        // Toggle button for locking/unlocking the selection.
        if (GUILayout.Button(isLocked ? "Unlock" : "Lock", GUILayout.Width(60)))
        {
            isLocked = !isLocked; // Toggle the lock state.
            if (!isLocked)
            {
                // If unlocked, immediately update based on the current selection.
                OnSelectionChange();
            }
        }
        EditorGUILayout.EndHorizontal(); // End horizontal group.


        EditorGUILayout.Space();

        // Provide feedback based on the selection state.
        if (sourceGameObject == null)
        {
            EditorGUILayout.HelpBox("Select a GameObject with a Mesh Renderer to copy its materials.", MessageType.Info);
        }
        else if (copiedMaterials == null || copiedMaterials.Length == 0)
        {
            EditorGUILayout.HelpBox("Source GameObject has no Mesh Renderer or no materials assigned.", MessageType.Warning);
        }
        else
        {
            // Display the header for copied materials.
            EditorGUILayout.LabelField("Copied Materials:", EditorStyles.largeLabel);

            // Start a scroll view for the list of materials.
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Iterate through the copied materials and display each one.
            for (int i = 0; i < copiedMaterials.Length; i++)
            {
                Material currentMaterial = copiedMaterials[i];
                if (currentMaterial != null)
                {
                    EditorGUILayout.BeginHorizontal(); // Start a horizontal group for the material field and button.

                    // Display the material in an ObjectField.
                    // Dragging from this field will allow you to drag the Material asset itself.
                    EditorGUILayout.ObjectField($"Material {i + 1}", currentMaterial, typeof(Material), true);

                    // Add a button to directly apply this material to the currently selected GameObject.
                    if (GUILayout.Button("Apply to Selected", GUILayout.Width(120)))
                    {
                        ApplyMaterialToSelectedObject(currentMaterial);
                    }
                    EditorGUILayout.EndHorizontal(); // End the horizontal group.
                }
            }
            EditorGUILayout.EndScrollView(); // End the scroll view.

            EditorGUILayout.Space();
            string helpMessage = "Drag and drop any of the above material fields onto another GameObject in the Hierarchy or Scene view to assign it. " +
                                 "Alternatively, use the 'Apply to Selected' button for the currently selected object.";
            if (isLocked)
            {
                helpMessage += "\n\nCurrently LOCKED: Materials will remain from the source object even if you select other GameObjects.";
            }
            else
            {
                helpMessage += "\n\nCurrently UNLOCKED: Selecting a new GameObject will update the copied materials.";
            }
            EditorGUILayout.HelpBox(helpMessage, MessageType.Info);
        }
    }

    /// <summary>
    /// Applies a given material to the MeshRenderer of the currently selected GameObject.
    /// If the GameObject doesn't have a MeshRenderer, it attempts to add one along with a MeshFilter.
    /// </summary>
    /// <param name="materialToApply">The material to be assigned.</param>
    private void ApplyMaterialToSelectedObject(Material materialToApply)
    {
        if (Selection.activeGameObject != null)
        {
            MeshRenderer targetRenderer = Selection.activeGameObject.GetComponent<MeshRenderer>();

            // If no MeshRenderer is found, try to add one. A MeshFilter is also usually required.
            if (targetRenderer == null)
            {
                Debug.LogWarning($"No MeshRenderer found on '{Selection.activeGameObject.name}'. Attempting to add one.");
                targetRenderer = Selection.activeGameObject.AddComponent<MeshRenderer>();
                // A MeshFilter is necessary for a MeshRenderer to render anything.
                // Assuming the user will provide a mesh via a MeshFilter later, or it's implicitly part of a Model.
                if (Selection.activeGameObject.GetComponent<MeshFilter>() == null)
                {
                    Selection.activeGameObject.AddComponent<MeshFilter>();
                }
            }

            if (targetRenderer != null)
            {
                // Record the change for Undo functionality in the editor.
                Undo.RecordObject(targetRenderer, "Assign Material");
                // Assign the material. Using sharedMaterial applies it to the asset/prefab too if it's an instance.
                targetRenderer.sharedMaterial = materialToApply;
                Debug.Log($"Assigned material '{materialToApply.name}' to '{Selection.activeGameObject.name}'.");
            }
            else
            {
                Debug.LogError($"Could not find or add MeshRenderer to '{Selection.activeGameObject.name}'. Material assignment failed.");
            }
        }
        else
        {
            Debug.LogWarning("No GameObject selected to apply the material to. Please select a GameObject in the Hierarchy or Scene view.");
        }
    }
}
