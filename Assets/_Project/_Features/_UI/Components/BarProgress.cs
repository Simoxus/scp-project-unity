using System.Linq;
using UnityEngine;

public class BarProgress : MonoBehaviour
{
    private GameObject[] bars; // array for the bars

    private void Awake()
    {
        InitializeBars();
    }

    private void InitializeBars()
    {
        if (bars != null) return;

        // Get all children and store them in an array
        bars = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            bars[i] = transform.GetChild(i).gameObject;
        }
    }

    // Get progress from 0 to 1
    public float GetProgress()
    {
        InitializeBars();

        if (bars == null || bars.Length == 0) return 0f;

        int activeBars = bars.Count(b => b != null && b.activeSelf);
        return (float)activeBars / bars.Length;
    }

    // Get progress by bar count
    public int GetProgressBars()
    {
        InitializeBars();

        if (bars == null || bars.Length == 0) return 0;

        return bars.Count(b => b != null && b.activeSelf);
    }

    // Set progress from 0 to 1
    public void SetProgress(float progress)
    {
        InitializeBars();

        if (bars == null || bars.Length == 0) return;

        progress = Mathf.Clamp01(progress);
        int activeBars = Mathf.RoundToInt(progress * bars.Length);
        UpdateBars(activeBars);
    }

    // Set progress by bar count
    public void SetProgressBars(int count)
    {
        InitializeBars();

        if (bars == null || bars.Length == 0) return;

        count = Mathf.Clamp(count, 0, bars.Length);
        UpdateBars(count);
    }

    private void UpdateBars(int activeCount)
    {
        for (int i = bars.Length - 1; i >= activeCount; i--)
        {
            if (bars[i] != null && bars[i].activeSelf)
            {
                bars[i].SetActive(false);
            }
        }

        for (int i = 0; i < activeCount; i++)
        {
            if (bars[i] != null && !bars[i].activeSelf)
            {
                bars[i].SetActive(true);
            }
        }
    }
}