using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

public class UITooltips : MonoBehaviour
{
    [Header("Tooltip Elements")]
    public Canvas canvas;
    public TextMeshProUGUI tooltipText;
    public RectTransform backgroundPanel;

    [Header("Settings")]
    public Vector2 offset = new Vector2(10f, 10f);
    public Vector2 padding = new Vector2(20f, 16f);
    public float maxWidth = 400f;

    private CancellationTokenSource _showCts;
    private bool _isVisible;

    public bool IsVisible => _isVisible;

    private void Awake()
    {
        Hide();
    }

    private void OnDestroy()
    {
        CancelAndDisposeCts();
    }

    private void LateUpdate()
    {
        if (IsVisible && canvas != null)
        {
            UpdatePosition();
        }
    }

    public async UniTaskVoid Show(string message)
    {
        if (string.IsNullOrEmpty(message) || canvas == null)
        {
            return;
        }

        CancelAndDisposeCts();
        _showCts = new CancellationTokenSource();

        try
        {
            tooltipText.text = message;
            ShowInternal();

            await UniTask.Yield(cancellationToken: _showCts.Token);

            ResizeTooltip();
            UpdatePosition();
        }
        catch (System.OperationCanceledException)
        {
            Hide();
        }
    }

    public void Hide()
    {
        backgroundPanel.gameObject.SetActive(false);
        _isVisible = false;
        CancelAndDisposeCts();
    }

    private void ShowInternal()
    {
        backgroundPanel.gameObject.SetActive(true);
        _isVisible = true;
    }

    public void Toggle()
    {
        if (_isVisible)
            Hide();
        else
            ShowInternal();
    }

    private void CancelAndDisposeCts()
    {
        if (_showCts != null)
        {
            _showCts.Cancel();
            _showCts.Dispose();
            _showCts = null;
        }
    }

    private void ResizeTooltip()
    {
        Canvas.ForceUpdateCanvases();

        tooltipText.rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            maxWidth - padding.x * 2
        );

        tooltipText.ForceMeshUpdate();
        Vector2 textSize = tooltipText.GetRenderedValues(false);
        textSize.x = Mathf.Min(textSize.x, maxWidth - padding.x * 2);

        Vector2 panelSize = textSize + padding * 2;
        backgroundPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelSize.x);
        backgroundPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelSize.y);
    }

    private void UpdatePosition()
    {
        if (canvas == null) return;
        backgroundPanel.position = (Vector2)Input.mousePosition + offset;
    }
}