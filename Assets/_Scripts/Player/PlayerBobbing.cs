using UnityEngine;

public class PlayerBobbing : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public PlayerInputs playerInputs;

    [Header("Headbob Settings")]
    public float BobWalkSpeed = 5f;
    public float BobSprintSpeed = 10f;
    public float BobAmount = 0.1f;
    public float BobStrength = 5f;
    public float BobMaxAngle = 5f;
    public float BobRotSpeed = 2f;

    private float _bobTimer;
    private bool _hasPlayedFootstep;
    private float _defaultYPos;
    private Vector3 _defaultRotation;

    private void Start()
    {
        _defaultYPos = playerController.CameraMain.transform.localPosition.y;
        _defaultRotation = playerController.CameraMain.transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        float movementSpeed = playerInputs.IsSprinting ? BobSprintSpeed : BobWalkSpeed;

        if (playerController.IsMoving)
        {
            _bobTimer += Time.deltaTime * movementSpeed;

            float bobOffset = Mathf.Sin(_bobTimer) * BobAmount;
            playerController.CameraMain.transform.localPosition = new Vector3(
                playerController.CameraMain.transform.localPosition.x,
                _defaultYPos + bobOffset,
                playerController.CameraMain.transform.localPosition.z
            );

            float rotationOffset = Mathf.Sin(_bobTimer * BobRotSpeed) * BobMaxAngle * BobRotSpeed;

            playerController.CameraMain.transform.localRotation = Quaternion.Euler(
                _defaultRotation.x,
                _defaultRotation.y,
                _defaultRotation.z + rotationOffset
            );

            if (bobOffset < 0 && !_hasPlayedFootstep)
            {
                _hasPlayedFootstep = true;
            }
            else if (bobOffset >= 0)
            {
                _hasPlayedFootstep = false;
            }
        }
    }
}
