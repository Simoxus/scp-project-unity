using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

// Attach to the door for this one :)
public class HandleVisual : MonoBehaviour
{
    [Header("Handle Settings")]
    public GameObject handle1;
    public GameObject handle2;

    [Header("Tween Settings")]
    public float tweenTurnTime = 0.32f;

    [Header("Handle Offsets")]
    public Quaternion normalRotation = Quaternion.identity; // Default to no rotation :)
    public Quaternion turnedRotation;

    public async void PlayTween()
    {
        // When tweening, use localPosition instead of global position
        await UniTask.WhenAll(
            Tween.LocalRotation(handle1.transform, turnedRotation, tweenTurnTime, Ease.InOutCubic).ToYieldInstruction().ToUniTask(),
            Tween.LocalRotation(handle2.transform, turnedRotation, tweenTurnTime, Ease.InOutCubic).ToYieldInstruction().ToUniTask()
        );

        await UniTask.WaitForSeconds(tweenTurnTime + 0.02f, ignoreTimeScale: false);

        await UniTask.WhenAll(
            Tween.LocalRotation(handle1.transform, normalRotation, tweenTurnTime, Ease.InOutCubic).ToYieldInstruction().ToUniTask(),
            Tween.LocalRotation(handle2.transform, normalRotation, tweenTurnTime, Ease.InOutCubic).ToYieldInstruction().ToUniTask()
        );
    }
}