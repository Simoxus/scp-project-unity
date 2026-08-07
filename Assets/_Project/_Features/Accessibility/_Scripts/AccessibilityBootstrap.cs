using UnityEngine;

/// <summary>
/// Creates the accessibility layer automatically after the first scene loads, in any scene,
/// so no scene or prefab in the base game needs editing.
/// </summary>
public static class AccessibilityBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (AccessibilityManager.Instance != null) return;

        var accessibilityObject = new GameObject("[Accessibility]");
        accessibilityObject.AddComponent<AccessibilityManager>();
        Debug.Log("[Accessibility] Layer initialized (sonar + settings).");
    }
}
