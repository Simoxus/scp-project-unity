using UnityEngine;

public class LookTarget : MonoBehaviour
{
    [Space]
    [SerializeField] private int priority = 0;
    [SerializeField] private bool isActive = true;

    public int Priority => priority;
    public bool IsActive => isActive;

    public void SetActive(bool active)
    {
        isActive = active;
    }

    public void SetPriority(int newPriority)
    {
        priority = newPriority;
    }
}