using UnityEngine;
using EditorAttributes;

public class BoxTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string requiredTag = "Player";

    [Header("State"), ReadOnly]
    public bool isTriggered = false;
    public GameObject currentObject;

    private void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
        {
            isTriggered = true;
            currentObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
        {
            isTriggered = false;
            currentObject = null;
        }
    }

    public bool GetState()
    {
        return isTriggered;
    }
}