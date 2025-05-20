using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public static class LoadingManager
{
    public static async UniTask LoadSceneAsync(string sceneName)
    {
        await SceneManager.LoadSceneAsync(sceneName).ToUniTask();
    }
}
