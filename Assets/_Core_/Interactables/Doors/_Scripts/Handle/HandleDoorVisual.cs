using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class HandleDoorVisual : MonoBehaviour
{
    [Space]
    public GameObject handleMesh;

    [Header("Rotation Settings")]
    public float rotationAngle = 45f;
    public Vector3 rotationAxis = Vector3.forward;
    public float rotateTime = 0.15f;

    private Quaternion _originalHandleLocalRotation;

    private void Start()
    {
        if (handleMesh != null)
        {
            _originalHandleLocalRotation = handleMesh.transform.localRotation;
        }
    }

    public async UniTask PlayTween()
    {
        if (handleMesh == null) return;

        Quaternion targetRotation = _originalHandleLocalRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis);
        await Tween.LocalRotation(
            handleMesh.transform,
            targetRotation,
            duration: rotateTime,
            ease: Ease.OutQuad
        );

        await UniTask.WaitForSeconds(rotateTime + 0.015f, ignoreTimeScale: false);

        // Return to original rotation
        await Tween.LocalRotation(
            handleMesh.transform,
            _originalHandleLocalRotation,
            duration: rotateTime,
            ease: Ease.InOutQuad
        );
    }
}