using UnityEngine;
using EditorAttributes;

public abstract class BaseSettingsApplier : MonoBehaviour
{
    [ReadOnly] public bool inBatchMode = false;

    protected abstract void InitializeReferences();

    protected virtual void Awake()
    {
        InitializeReferences();
    }

    protected virtual void Reset()
    {
        InitializeReferences();
    }
}