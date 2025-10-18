using TMPro;
using UnityEngine;

public class FPSCounterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text fpsText;

    [Header("Update Settings")]
    [SerializeField] private float pollingTime = 0.5f;

    private float _timeAccumulator;
    private int _frameCount;

    void Update()
    {
        _timeAccumulator += Time.unscaledDeltaTime;
        _frameCount++;

        if (_timeAccumulator >= pollingTime)
        {
            // Calculate frames per second.
            int fps = Mathf.RoundToInt(_frameCount / _timeAccumulator);

            if (fpsText != null)
            {
                fpsText.text = $"{fps} FPS";
            }

            // Reset the counters for the next polling period.
            _timeAccumulator = 0f;
            _frameCount = 0;
        }
    }
}
