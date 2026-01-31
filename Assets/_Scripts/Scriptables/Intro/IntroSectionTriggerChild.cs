using UnityEngine;

public class IntroSectionTriggerChild : MonoBehaviour
{
    private IntroSectionTrigger _parent;
    private int _cachedInstanceID;

    private void Awake()
    {
        _cachedInstanceID = gameObject.GetInstanceID();
    }

    public void SetParent(IntroSectionTrigger parent)
    {
        _parent = parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _parent?.OnPlayerEnterCollider(_cachedInstanceID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _parent?.OnPlayerExitCollider(_cachedInstanceID);
        }
    }
}