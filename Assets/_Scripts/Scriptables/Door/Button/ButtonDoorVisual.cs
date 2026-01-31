using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class ButtonDoorVisual : BaseDoorVisual
{
    [Header("Button Animation")]
    public GameObject buttonMesh;

    [Header("Tween Settings")]
    public float tweenPushTime = 0.15f;
    public float meshPushedOffset = 0.01f;

    private float _originalMeshLocalPositionZ;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (buttonMesh != null)
        {
            _originalMeshLocalPositionZ = buttonMesh.transform.localPosition.z;
        }
    }

    public async UniTask PlayTween()
    {
        if (buttonMesh == null) return;

        await Tween.LocalPositionZ(
            buttonMesh.transform,
            _originalMeshLocalPositionZ + meshPushedOffset,
            duration: tweenPushTime
        );

        await UniTask.WaitForSeconds(tweenPushTime + 0.015f, ignoreTimeScale: false);

        await Tween.LocalPositionZ(
            buttonMesh.transform,
            _originalMeshLocalPositionZ,
            duration: tweenPushTime
        );
    }
}