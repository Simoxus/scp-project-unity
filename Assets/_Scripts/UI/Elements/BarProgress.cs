using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BarProgress : MonoBehaviour
{
    private GameObject[] bars; // array for the bars

    // Cache bars in the GameObject's children (ayo?)
    // note from future me: it was like 2 AM when i wrote the comment above
    void Awake()
    {
        // Get all children and store them in an array
        bars = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            bars[i] = transform.GetChild(i).gameObject;
        }
    }

    // Set progress as fraction, from 0 to 1
    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        int activeBars = Mathf.RoundToInt(progress * bars.Length);
        UpdateBars(activeBars);
    }

    // Set progress directly, by bar count
    public void SetProgressBars(int count)
    {
        count = Mathf.Clamp(count, 0, bars.Length);
        UpdateBars(count);
    }

    private void UpdateBars(int activeCount)
    {
        // Deactivate bars that are no longer needed starting from the last active bar
        for (int i = bars.Length - 1; i >= activeCount; i--)
        {
            if (bars[i].activeSelf)
            {
                bars[i].SetActive(false);
            }
        }

        // Activate bars that are now needed up to the new active count
        for (int i = 0; i < activeCount; i++)
        {
            if (!bars[i].activeSelf)
            {
                bars[i].SetActive(true);
            }
        }
    }
}