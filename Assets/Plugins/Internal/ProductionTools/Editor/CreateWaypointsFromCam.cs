using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor tool to create a GameObject based on the Scene Camera's position and rotation.
/// It allows selecting which axes to apply and whether to parent it to the selected object.
/// </summary>
public class CreateWaypointsFromCam : EditorWindow
{
    // --- Editor Window State Variables ---
    private string objectName = "CutsceneObject";
    private bool useCameraPosition = true;
    private bool useCameraRotation = true;
    private bool positionX = true;
    private bool positionY = true;
    private bool positionZ = true;
    private bool rotationX = true;
    private bool rotationY = true;
    private bool rotationZ = true;
    private bool createAsChild = false;

    // --- Menu Item to Open the Tool ---
    [MenuItem("Tools/Production Tools/Create Cutscene Object From Camera")]
    public static void ShowWindow()
    {
        // Get existing open window or create a new one
        GetWindow<CreateWaypointsFromCam>("Cutscene Object Creator");
    }

    // --- GUI Layout for the Editor Window ---
    void OnGUI()
    {
        GUILayout.Label("Cutscene Object Creation Settings", EditorStyles.boldLabel);

        // Object Name Input
        objectName = EditorGUILayout.TextField("Object Name", objectName);
        EditorGUILayout.Space();

        // Position Settings
        useCameraPosition = EditorGUILayout.Toggle("Use Camera Position", useCameraPosition);
        if (useCameraPosition)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20); // Indent for better readability
            positionX = EditorGUILayout.ToggleLeft("X", positionX, GUILayout.Width(40));
            positionY = EditorGUILayout.ToggleLeft("Y", positionY, GUILayout.Width(40));
            positionZ = EditorGUILayout.ToggleLeft("Z", positionZ, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        // Rotation Settings
        useCameraRotation = EditorGUILayout.Toggle("Use Camera Rotation", useCameraRotation);
        if (useCameraRotation)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20); // Indent for better readability
            rotationX = EditorGUILayout.ToggleLeft("X", rotationX, GUILayout.Width(40));
            rotationY = EditorGUILayout.ToggleLeft("Y", rotationY, GUILayout.Width(40));
            rotationZ = EditorGUILayout.ToggleLeft("Z", rotationZ, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        // Create as Child Option
        createAsChild = EditorGUILayout.Toggle("Create as Child of Selected", createAsChild);
        EditorGUILayout.Space();

        // Create Button
        if (GUILayout.Button("Create Cutscene Object"))
        {
            CreateObject();
        }
    }

    // --- Core Logic: Create the GameObject ---
    void CreateObject()
    {
        // Get the current Scene View camera
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null || sceneView.camera == null)
        {
            Debug.LogError("No active Scene View found or Scene View camera is null. Please open a Scene View.");
            return;
        }

        Transform cameraTransform = sceneView.camera.transform;

        // Create the new GameObject
        GameObject newGameObject = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(newGameObject, "Create Cutscene Object"); // For Undo functionality

        // Apply Position
        Vector3 newPosition = newGameObject.transform.position;
        if (useCameraPosition)
        {
            Vector3 camPos = cameraTransform.position;
            if (positionX) newPosition.x = camPos.x;
            if (positionY) newPosition.y = camPos.y;
            if (positionZ) newPosition.z = camPos.z;
        }
        newGameObject.transform.position = newPosition;

        // Apply Rotation
        Quaternion newRotation = newGameObject.transform.rotation;
        if (useCameraRotation)
        {
            // Convert camera's Euler angles to apply selectively
            Vector3 camEuler = cameraTransform.rotation.eulerAngles;
            Vector3 finalEuler = newGameObject.transform.rotation.eulerAngles; // Start with current object's rotation

            if (rotationX) finalEuler.x = camEuler.x;
            if (rotationY) finalEuler.y = camEuler.y;
            if (rotationZ) finalEuler.z = camEuler.z;

            newRotation = Quaternion.Euler(finalEuler);
        }
        newGameObject.transform.rotation = newRotation;

        // Parent to selected GameObject if option is checked and an object is selected
        if (createAsChild && Selection.activeGameObject != null)
        {
            newGameObject.transform.SetParent(Selection.activeGameObject.transform);
            // Reset local position/rotation if desired, or keep world values
            // newGameObject.transform.localPosition = Vector3.zero;
            // newGameObject.transform.localRotation = Quaternion.identity;
        }

        // Select the newly created GameObject in the Hierarchy
        Selection.activeGameObject = newGameObject;

        Debug.Log($"Created '{objectName}' at camera location with specified axes.");
    }
}
