using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : Singleton<LoadingManager>
{
    protected override bool PersistAcrossScenes => true;

    [SerializeField] private UILoading loadingUI;

    public async UniTask LoadSceneAsync(string sceneName, Action<float> onProgress = null)
    {
        if (loadingUI == null)
        {
            await SceneManager.LoadSceneAsync(sceneName).ToUniTask();
            return;
        }

        loadingUI.Show();
        loadingUI.SetProgress(0f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = operation.progress / 0.9f;
            loadingUI.SetProgress(progress);
            loadingUI.UpdateLoadingText(progress);
            onProgress?.Invoke(progress);
            await UniTask.Yield();
        }

        loadingUI.SetProgress(1f);
        loadingUI.UpdateLoadingText(1f);
        onProgress?.Invoke(1f);

        FMODHelper.PlayOneShot(Core.AudioDataAccess.UI.LoadSound);

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        operation.allowSceneActivation = true;

        await UniTask.WaitUntil(() => operation.isDone);
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

        loadingUI.Hide();
    }

    public async UniTask LoadSceneWithPressAnyKey(string sceneName, Action<float> onProgress = null)
    {
        if (loadingUI == null)
        {
            await SceneManager.LoadSceneAsync(sceneName).ToUniTask();
            return;
        }

        loadingUI.Show();
        loadingUI.SetProgress(0f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = operation.progress / 0.9f;
            loadingUI.SetProgress(progress);
            loadingUI.UpdateLoadingText(progress);
            onProgress?.Invoke(progress);
            await UniTask.Yield();
        }

        loadingUI.SetProgress(1f);
        loadingUI.UpdateLoadingText(1f);
        onProgress?.Invoke(1f);

        FMODHelper.PlayOneShot(Core.AudioDataAccess.UI.LoadSound);

        operation.allowSceneActivation = true;

        // Start flashing the text and pause the game
        Core.GameManager.RequestPause(this);
        loadingUI.ShowPressAnyKey();
        await UniTask.WaitUntil(() => Input.anyKeyDown);
        loadingUI.HidePressAnyKey();
        Core.GameManager.ReleasePause(this);

        await UniTask.WaitUntil(() => operation.isDone);
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

        // Fade out the main canvas
        await loadingUI.FadeOut();
    }

    public async UniTask ShowLoadingScreen(float duration = 0.3f)
    {
        if (loadingUI != null)
        {
            await loadingUI.FadeIn(duration);
        }
    }

    public async UniTask HideLoadingScreen(float duration = 0.3f)
    {
        if (loadingUI != null)
        {
            await loadingUI.FadeOut(duration);
        }
    }

    public void ShowLoadingScreenImmediate()
    {
        if (loadingUI != null)
        {
            loadingUI.Show();
        }
    }

    public void HideLoadingScreenImmediate()
    {
        if (loadingUI != null)
        {
            loadingUI.HideImmediate();
        }
    }

    public void SetProgress(float progress)
    {
        if (loadingUI != null)
        {
            loadingUI.SetProgress(progress);
            loadingUI.UpdateLoadingText(progress);
        }
    }

    public void SetLoadingText(string text)
    {
        if (loadingUI != null)
        {
            loadingUI.SetLoadingText(text);
        }
    }

    public void SetFactText(string header, string description)
    {
        if (loadingUI != null)
        {
            loadingUI.SetFactText(header, description);
        }
    }
}