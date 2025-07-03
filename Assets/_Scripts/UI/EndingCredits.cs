using UnityEngine;
using UnityEngine.SceneManagement;
using PrimeTween;
using Cysharp.Threading.Tasks;

public class EndingCredits : MonoBehaviour
{
    [SerializeField] private Transform creditsText;

    [Header("Tween Settings")]
    private float _creditsTweenTime = 150f;
    private float _creditsEndpoint = 6000;
   
    private void Start()
    {
        RunCreditsRoll().Forget();
    }

    private async UniTaskVoid RunCreditsRoll()
    {
        await Tween.PositionY(
            creditsText,
            endValue: _creditsEndpoint, // float
            duration: _creditsTweenTime, // float
            Ease.Linear
        );
        await UniTask.WaitForSeconds(5f, ignoreTimeScale: true);
        await SceneManager.LoadSceneAsync("_Scenes/Testing_Core");
    }
}
