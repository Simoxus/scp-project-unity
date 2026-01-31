using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Collections.Generic;
using UnityEngine;

public class KeypadDoorVisual : BaseDoorVisual
{
    [Header("Keypad Keys")]
    public GameObject[] keyMeshes;
    public GameObject enterKeyMesh;
    public GameObject clearKeyMesh;

    [Header("Key Animation Settings")]
    public float tweenPushTime = 0.15f;
    public float meshPushedOffset = -0.03f;

    private Dictionary<GameObject, float> _originalKeyPositions;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _originalKeyPositions = new Dictionary<GameObject, float>();

        // Store original positions for number keys
        foreach (GameObject key in keyMeshes)
        {
            if (key != null)
            {
                _originalKeyPositions[key] = key.transform.localPosition.z;
            }
        }

        // Store original positions for special keys
        if (enterKeyMesh != null)
        {
            _originalKeyPositions[enterKeyMesh] = enterKeyMesh.transform.localPosition.z;
        }

        if (clearKeyMesh != null)
        {
            _originalKeyPositions[clearKeyMesh] = clearKeyMesh.transform.localPosition.z;
        }
    }

    private async UniTask PlayKeyPressAnimation(GameObject keyMesh)
    {
        if (keyMesh == null || !_originalKeyPositions.ContainsKey(keyMesh)) return;

        float originalZ = _originalKeyPositions[keyMesh];
        float pushedZ = originalZ + meshPushedOffset;

        // Push key in
        await Tween.LocalPositionZ(
            keyMesh.transform,
            pushedZ,
            duration: tweenPushTime,
            ease: Ease.Linear
        ).ToYieldInstruction().ToUniTask();

        await UniTask.WaitForSeconds(tweenPushTime + 0.015f, ignoreTimeScale: false);

        // Push key out
        await Tween.LocalPositionZ(
            keyMesh.transform,
            originalZ,
            duration: tweenPushTime,
            ease: Ease.Linear
        ).ToYieldInstruction().ToUniTask();
    }

    public async UniTask PlayNumberKeyTween(GameObject keyMesh)
    {
        await PlayKeyPressAnimation(keyMesh);
    }

    public async UniTask PlayEnterKeyTween()
    {
        await PlayKeyPressAnimation(enterKeyMesh);
    }

    public async UniTask PlayClearKeyTween()
    {
        await PlayKeyPressAnimation(clearKeyMesh);
    }
}