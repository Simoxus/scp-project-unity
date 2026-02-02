using UnityEngine;
using UnityEngine.UI;

public class ImageSlider : MonoBehaviour
{
    [Space]
    [SerializeField] private Image barImage;

    [Header("Bar Settings")]
    [SerializeField] private int totalBars = 20;
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private float currentValue = 100f;

    private void Update()
    {
        // Clamp the stuff
        currentValue = Mathf.Clamp(currentValue, 0f, maxValue);

        // Normalize
        float normalized = currentValue / maxValue;

        float stepSize = 1f / totalBars;
        barImage.fillAmount = Mathf.Floor(normalized / stepSize) * stepSize;
    }

    // Call these functions externally :)
    public void SetValue(float value)
    {
        currentValue = value;
    }

    public void AddValue(float amount)
    {
        currentValue = Mathf.Clamp(currentValue + amount, 0f, maxValue);
    }
}
