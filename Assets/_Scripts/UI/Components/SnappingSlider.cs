using UnityEngine;
using UnityEngine.UI;

public class SnappingSlider : MonoBehaviour
{
    [System.Serializable]
    public class SnapPoint
    {
        public float value;
        public RectTransform marker;
    }

    public Slider targetSlider;
    public SnapPoint[] snapPoints;

    [Header("Marker Animation Settings")]
    public float scaleUpSize = 1.3f;
    public float scaleThreshold = 0.1f;

    private int _lastActiveIndex = -1;

    private void Awake()
    {
        targetSlider = GetComponent<Slider>();

        targetSlider.onValueChanged.AddListener(OnSliderValueChange);

        foreach (var snapPoint in snapPoints)
        {
            if (snapPoint.marker != null)
            { snapPoint.marker.localScale = Vector3.one; }
        }

        OnSliderValueChange(targetSlider.value); // Initialize marker states
    }

    public void OnSliderValueChange(float currentValue)
    {
        // Find the nearest marker
        int nearestIndex = -1;
        float minDistance = float.MaxValue;

        for (int i = 0; i < snapPoints.Length; i++)
        {
            float distance = Mathf.Abs(currentValue - snapPoints[i].value);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        // Only scale if within the specific threshold
        if (minDistance <= scaleThreshold)
        {
            // Play the snap sound ONLY if snapping to a new different marker
            if (nearestIndex != _lastActiveIndex)
                FMODHelper.PlayOneShot(Core.AudioDataAccess.UI.SliderSnapSound);

            UpdateMarkers(nearestIndex);
        }
        else
        {
            UpdateMarkers(-1);
        }
    }

    private void UpdateMarkers(int activeIndex)
    {
        if (_lastActiveIndex == activeIndex) return;

        for (int i = 0; i < snapPoints.Length; i++)
        {
            if (snapPoints[i].marker != null)
            {
                Vector3 targetScale = (i == activeIndex) ? Vector3.one * scaleUpSize : Vector3.one;
                snapPoints[i].marker.localScale = targetScale;
            }
        }

        _lastActiveIndex = activeIndex;
    }
}