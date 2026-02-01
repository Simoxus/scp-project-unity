using EditorAttributes;
using UnityEngine;

public abstract class BaseSettingsApplier : MonoBehaviour
{
    [ReadOnly] public bool inBatchMode = false;

    protected abstract void InitializeReferences();

    protected virtual void Awake()
    {
        inBatchMode = true;
        InitializeReferences();
    }

    protected virtual void Reset()
    {
        InitializeReferences();
    }
}