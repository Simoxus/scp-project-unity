using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class BaseSettings : MonoBehaviour
{
    public abstract string CATEGORY { get; }
    protected bool _isWaitingToSave = false;

    protected virtual void Awake()
    {
        InitializeReferences();
    }

    protected abstract void InitializeReferences();
    public abstract void SaveSettings();
    public abstract void LoadSettings();

    public virtual async UniTask LoadSettingsAsync()
    {
        await UniTask.CompletedTask;
        LoadSettings();
    }

    public void ResetCategorySettings()
    {
        ResetCategorySettingsAsync().Forget();
    }

    public async UniTask ResetCategorySettingsAsync()
    {
        if (Core.SettingsManager == null) return;
        Core.SettingsManager.ResetCategory(CATEGORY);
        await LoadSettingsAsync();
        SaveSettings();
    }

    public bool CheckIfMainMenu()
    {
        return Core.GameManager && Core.GameManager.IsInMainMenu;
    }

    public async void SaveSettingsWithDelay(float delay = 0.5f)
    {
        if (_isWaitingToSave) return;
        _isWaitingToSave = true;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true);
            SaveSettings();
        }
        finally
        {
            _isWaitingToSave = false;
        }
    }
}