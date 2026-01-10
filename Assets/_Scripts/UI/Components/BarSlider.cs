using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BarSlider : MonoBehaviour
{
    [Header("Slider Bar Variables")]
    [SerializeField] private Image[] barSegments;
    [SerializeField] private Slider barSlider;

    private int _lastFilledCount = -1;

    private void Start()
    {
        barSlider.maxValue = 100f;

        for (int i = 0; i < barSegments.Length; i++)
            barSegments[i].enabled = false;
    }

    private void Update()
    {
        UpdateSliderBar(barSlider.value);
    }

    private void UpdateSliderBar(float rawValue)
    {
        // Normalize from 0–100 to 0–1
        float progress = Mathf.Clamp01(rawValue / 100f);
        int filledCount = Mathf.FloorToInt(progress * barSegments.Length);

        if (filledCount == _lastFilledCount)
            return;

        _lastFilledCount = filledCount;

        for (int i = 0; i < barSegments.Length; i++)
        {
            barSegments[i].enabled = i < filledCount;
        }
    }
}
