using EditorAttributes;
using UnityEngine;

public class PlayerSprint : MonoBehaviour
{
    [Space]
    [SerializeField] private float sprintDrainRate = 0.26f;
    [SerializeField] private float sprintRegenRateMoving = 0.194f;
    [SerializeField] private float sprintRegenRateIdle = 0.2f;
    [SerializeField] private float minSprintThreshold = 0.0f;
    [SerializeField] private float tiredSoundThreshold = 0.07f;

    [Header("Runtime")]
    [ReadOnly] public float currentSprint = 1f;

    private bool _isSprinting;
    private bool _isMoving;

    private void Start()
    {
        currentSprint = 1f;
    }

    private void Update()
    {
        if (Core.GameManager == null || Core.GameManager.gamePaused) return;

        HandleSprint();
        UpdateUI();
    }

    private void HandleSprint()
    {
        if (_isSprinting && _isMoving && currentSprint > minSprintThreshold)
        {
            currentSprint -= (sprintDrainRate / 100f) * Time.deltaTime * 60f;

            if (currentSprint <= 0f)
            {
                currentSprint = -0.2f;
            }
        }
        else
        {
            float regenRate = _isMoving ? sprintRegenRateMoving : sprintRegenRateIdle;
            currentSprint += (regenRate / 100f) * Time.deltaTime * 60f;
            currentSprint = Mathf.Min(currentSprint, 1f);
        }

        HandleTiredSounds();
    }

    private void HandleTiredSounds()
    {
        bool shouldPlayTired = currentSprint < tiredSoundThreshold;

        if (shouldPlayTired)
        {
            FMODHelper.PlayOneShot(Core.AudioDataAccess.Player.TiredSound);
        }
    }

    public bool CanSprint()
    {
        if (Core.Player.CurrentState == PlayerState.Noclip) return false;
        return currentSprint > minSprintThreshold;
    }

    public void SetCurrentState(bool isSprinting, bool isMoving, bool isCrouching)
    {
        _isSprinting = isSprinting;
        _isMoving = isMoving;
    }

    private void UpdateUI()
    {
        if (Core.UI.Indicators != null)
        {
            Core.UI.Indicators.SetSprintProgress(currentSprint);
        }
    }
}