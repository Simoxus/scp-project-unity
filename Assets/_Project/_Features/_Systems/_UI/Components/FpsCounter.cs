using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [Space]
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private float pollingTime = 0.5f;

    private float _timeAccumulator;
    private int _frameCount;

    private void Update()
    {
        _timeAccumulator += Time.unscaledDeltaTime;
        _frameCount++;

        if (_timeAccumulator >= pollingTime)
        {
            int fps = Mathf.RoundToInt(_frameCount / _timeAccumulator);
            fpsText.text = $"{fps} FPS";

            _timeAccumulator = 0f;
            _frameCount = 0;
        }
    }
}
