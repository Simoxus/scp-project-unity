using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class WakeUpCutscene : MonoBehaviour
{
    [Space]
    [SerializeField] private float wakeUpFov = 60f;
    [Space]
    [SerializeField] private Animator wakeUpAnimator;
    [SerializeField] private AnimationClip wakeUpAnimationClip;
    [SerializeField] private Camera wakeUpCamera;
    [SerializeField] private Transform footstepPosition;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform playerCameraSpawnPoint;

    private bool _isIntroCutsceneActive = false;

    private void Awake()
    {
        wakeUpCamera.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (Core.GameManager != null)
        {
            Core.GameManager.OnPauseStateChanged += HandlePauseStateChanged;
        }
    }

    private void OnDisable()
    {
        if (Core.GameManager != null)
        {
            Core.GameManager.OnPauseStateChanged -= HandlePauseStateChanged;
        }
    }

    public async UniTask PlayCutscene(IntroSectionTrigger[] triggersToReset = null)
    {
        _isIntroCutsceneActive = true;

        TeleportPlayerToSpawn(triggersToReset);
        DisablePlayerForCutscene();
        PlayFootstepsAsync().Forget();

        wakeUpCamera.fieldOfView = wakeUpFov;
        wakeUpCamera.gameObject.SetActive(true);

        wakeUpAnimator.Play(wakeUpAnimationClip.name);

        await UniTask.WhenAll(
            UniTask.WaitForSeconds(wakeUpAnimationClip.length),
            CrossfadeToPlayerCamera(),
            ZoomToPlayerFOVAsync()
        );

        EndCutscene();
    }

    private void EndCutscene()
    {
        _isIntroCutsceneActive = false;

        wakeUpCamera.gameObject.SetActive(false);
        Core.Player.CameraRoot.SetActive(true);

        Core.GameManager.RequestDisableControls(this, shouldDisable: false);
        Core.GameManager.ReleaseCursorControl(this);

        Core.Player.Bobbing.ResetBobbing();
    }

    private void TeleportPlayerToSpawn(IntroSectionTrigger[] triggersToReset)
    {
        if (Core.Player.CharacterController != null && playerSpawnPoint != null)
        {
            if (triggersToReset != null)
            {
                foreach (var trigger in triggersToReset)
                {
                    if (trigger != null)
                    {
                        trigger.ForceResetTriggers();
                    }
                }
            }

            Core.Player.CharacterController.enabled = false;

            Core.Player.transform.position = playerSpawnPoint.position;
            Core.Player.transform.rotation = playerSpawnPoint.rotation;
            Core.Player.Controller.ResetMoveDirection();
            Core.Player.Controller.ResetLookRotation();

            Core.Player.CharacterController.enabled = true;
        }
    }

    private void DisablePlayerForCutscene()
    {
        Core.GameManager.RequestDisableControls(this, shouldDisable: true, updateCursor: false);
        Core.GameManager.RequestCursorControl(this);
        Core.GameManager.SetCursorState(this, visible: false, CursorLockMode.Locked);
        Core.Player.CameraRoot.SetActive(false);
    }

    private async UniTask PlayFootstepsAsync()
    {
        await UniTask.WaitForSeconds(5.1f);
        FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Player.GetUpFootstepSound, footstepPosition.position);
        await UniTask.WaitForSeconds(0.8f);
        FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Player.GetUpFootstepSound, footstepPosition.position);
    }

    private async UniTask CrossfadeToPlayerCamera()
    {
        await UniTask.WaitForSeconds(6.263f);
        await Tween.Position(
            wakeUpCamera.transform,
            playerCameraSpawnPoint.transform.position,
            0.6f,
            Ease.Linear
        ).ToYieldInstruction().ToUniTask();
    }

    private async UniTask ZoomToPlayerFOVAsync()
    {
        if (wakeUpCamera.fieldOfView == Core.Player.CameraMain.Lens.FieldOfView) return;

        await UniTask.WaitForSeconds(6.08f);
        await Tween.Custom(
            wakeUpCamera.fieldOfView,
            Core.Player.CameraMain.Lens.FieldOfView,
            1.2f,
            onValueChange: fov =>
            {
                wakeUpCamera.fieldOfView = fov;
            },
            Ease.InOutCubic
        ).ToYieldInstruction().ToUniTask();
    }

    private void HandlePauseStateChanged(bool isPaused, object requester)
    {
        if (!ReferenceEquals(requester, this) && _isIntroCutsceneActive)
        {
            if (isPaused)
            {
                Core.GameManager.ReleaseCursorControl(this);
            }
            else
            {
                Core.GameManager.RequestCursorControl(this);
                Core.GameManager.SetCursorState(this, visible: false, CursorLockMode.Locked);
            }
        }
    }
}
