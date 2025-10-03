using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class SnappingSlider : MonoBehaviour
{
    public Slider targetSlider;
    public float[] snapPoints; // Array of desired snap values
    public float snapThreshold = 0.1f; // How close the slider needs to be to snap

    public EventReference snapSound; // Sound to play on snap

    private void Awake()
    {
        targetSlider = GetComponent<Slider>();
        targetSlider.onValueChanged.AddListener(delegate { SnapToNearestPoint(); });
    }

    public void SnapToNearestPoint()
    {
        float currentValue = targetSlider.value;
        float nearestSnapPoint = snapPoints[0];
        float minDistance = Mathf.Abs(currentValue - nearestSnapPoint);

        foreach (float snapPoint in snapPoints)
        {
            float distance = Mathf.Abs(currentValue - snapPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestSnapPoint = snapPoint;
            }
        }

        if (minDistance <= snapThreshold)
        {
            FMODHelper.PlayOneShot(snapSound);
            targetSlider.value = nearestSnapPoint;
        }
    }
}