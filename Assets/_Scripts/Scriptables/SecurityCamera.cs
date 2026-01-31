using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Space]
    public Camera securityCamera;
    public Animator cameraAnimator;

    [Header("Monitor Settings")]
    public MeshRenderer monitorScreen;
    public TMP_Text monitorText;
    public int materialIndex = 0;

    [Header("Texture Settings")]
    public int renderWidth = 200;
    public int renderHeight = 160;
    public float updateInterval = 0.2f;

    private RenderTexture _renderTexture;
    private CancellationTokenSource _renderCts;
    private Texture2D _monitorTexture;
    private Material _monitorMaterial;

    private void Start()
    {
        SetupSecurityCamera();
        _renderCts = new CancellationTokenSource();
        UpdateTextureLoop(_renderCts.Token).Forget();
    }

    void SetupSecurityCamera()
    {
        _renderTexture = new RenderTexture(renderWidth, renderHeight, 8);
        _renderTexture.Create();

        _monitorTexture = new Texture2D(renderWidth, renderHeight, TextureFormat.RGB24, false);
        securityCamera.targetTexture = _renderTexture;

        Material[] materials = monitorScreen.materials;
        if (materialIndex < materials.Length)
        {
            _monitorMaterial = materials[materialIndex];
            _monitorMaterial.mainTexture = _monitorTexture;
        }
    }

    private async UniTaskVoid UpdateTextureLoop(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested == false)
        {
            try
            {
                await UniTask.WaitForSeconds(updateInterval, cancellationToken: cancellationToken);

                if (_renderTexture != null && _monitorTexture != null && securityCamera.enabled)
                {
                    RenderTexture.active = _renderTexture;
                    _monitorTexture.ReadPixels(new Rect(0, 0, renderWidth, renderHeight), 0, 0);
                    _monitorTexture.Apply();
                    RenderTexture.active = null;
                }
            }
            catch (System.OperationCanceledException)
            {
                break;
            }
        }
    }

    private void OnDestroy()
    {
        _renderCts?.Cancel();
        _renderCts?.Dispose();

        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }

        if (_monitorTexture != null)
        {
            Destroy(_monitorTexture);
        }
    }

    public void ToggleCamera(bool enabled)
    {
        if (securityCamera != null)
        {
            securityCamera.enabled = enabled;
        }
    }

    public void ChangeMonitorText(string text)
    {
        if (text != null)
        {
            monitorText.text = text;
        }
    }
}