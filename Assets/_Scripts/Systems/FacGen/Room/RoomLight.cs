using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Threading;
using UnityEngine;

public class RoomLight : MonoBehaviour
{
    [SerializeField] private Light mainLight;
    [SerializeField] private Light topLight;
    [SerializeField] private Light emissionLight;
    [SerializeField] private GameObject beam;
    [SerializeField] private LightBeamSettings beamSettings;

    [Header("Behavior Settings")]
    [SerializeField] private bool affectedByColorChange = true;
    [SerializeField] private bool canFlicker = true;
    [SerializeField] private bool isZoinkedLight = false;

    private float _originalIntensity;
    private Color _originalColor;
    private Tween _flickerTween;
    private Tween _beamTween;
    private CancellationTokenSource _flickerCts;
    private int _zoinkedMusicHandle = -1;

    public Light MainLight => mainLight;
    public Light TopLight => topLight;
    public Light EmissionLight => emissionLight;
    public GameObject Beam => beam;
    public LightBeamSettings BeamSettings => beamSettings;
    public bool AffectedByColorChange => affectedByColorChange;
    public bool CanFlicker => canFlicker;
    public bool IsZoinkedLight => isZoinkedLight;
    public float OriginalIntensity => _originalIntensity;

    private void Awake()
    {
        if (mainLight != null)
        {
            _originalIntensity = mainLight.intensity;
            _originalColor = mainLight.color;
        }

        if (beam != null && beamSettings == null)
        {
            beamSettings = beam.GetComponent<LightBeamSettings>();
        }
    }

    private void OnEnable()
    {
        Core.FacilityManager?.RegisterLight(this);

        if (isZoinkedLight)
        {
            _zoinkedMusicHandle = FMODHelper.PlayInstance(
                Core.AudioDataAccess.Special.SCP420JSong,
                beam,
                useOcclusion: true
            );

            StartRainbow(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    private void OnDisable()
    {
        Core.FacilityManager?.UnregisterLight(this);

        StopFlicker();

        if (isZoinkedLight)
        {
            FMODHelper.StopInstance(_zoinkedMusicHandle);
            _zoinkedMusicHandle = -1;

            StopRainbow();
        }
    }

    private void OnDestroy()
    {
        StopFlicker();
        _flickerCts?.Cancel();
        _flickerCts?.Dispose();
    }

    public async UniTask TweenIntensityAsync(float targetIntensity, float duration, CancellationToken cancellationToken, Ease ease = Ease.Linear)
    {
        if (mainLight == null) return;

        _flickerTween.Stop();
        float startIntensity = mainLight.intensity;

        _flickerTween = Tween.Custom(
            startIntensity,
            targetIntensity,
            duration,
            onValueChange: newIntensity =>
            {
                mainLight.intensity = newIntensity;
            },
            ease: ease
        );

        await _flickerTween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
    }

    public async UniTask SetIntensityInstant(float intensity)
    {
        if (mainLight != null)
        {
            mainLight.intensity = intensity;
        }
        await UniTask.Yield();
    }

    public async UniTask TransitionColorAsync(Color targetColor, float duration, CancellationToken cancellationToken = default)
    {
        if (!affectedByColorChange || mainLight == null) return;

        _flickerTween.Stop();
        Color startColor = mainLight.color;

        _flickerTween = Tween.Custom(
            startColor,
            targetColor,
            duration,
            onValueChange: newColor =>
            {
                mainLight.color = newColor;
            },
            ease: Ease.InOutSine
        );

        await _flickerTween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
    }

    public void SetColor(Color color)
    {
        if (affectedByColorChange && mainLight != null)
        {
            mainLight.color = color;
        }
    }

    public void ResetColor()
    {
        if (affectedByColorChange && mainLight != null)
        {
            mainLight.color = _originalColor;
        }
    }

    public void SetIntensity(float intensity)
    {
        if (mainLight != null)
        {
            mainLight.intensity = intensity;
        }
    }

    public void ResetIntensity()
    {
        if (mainLight != null)
        {
            mainLight.intensity = _originalIntensity;
        }
    }

    public void UpdateOriginalIntensity()
    {
        if (mainLight != null)
        {
            _originalIntensity = mainLight.intensity;
        }
    }

    public void StopFlicker()
    {
        _flickerTween.Stop();
        _beamTween.Stop();
        _flickerCts?.Cancel();
        _flickerCts?.Dispose();
        _flickerCts = null;
    }

    public void ResetToOriginal()
    {
        StopFlicker();
        if (mainLight != null)
        {
            mainLight.intensity = _originalIntensity;
            mainLight.color = _originalColor;
        }
    }

    public async UniTask StartRainbow(CancellationToken cancellationToken)
    {
        if (!isZoinkedLight) return;

        StopFlicker();
        _flickerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        float hue = 0f;

        while (!_flickerCts.Token.IsCancellationRequested)
        {
            hue = (hue + Time.deltaTime * 0.2f) % 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 0.8f, 1f);

            if (mainLight != null) mainLight.color = rainbowColor;
            if (topLight != null) topLight.color = rainbowColor;
            if (emissionLight != null) emissionLight.color = rainbowColor;

            await UniTask.Yield(_flickerCts.Token);
        }
    }

    public void StopRainbow()
    {
        _flickerCts?.Cancel();
        _flickerCts?.Dispose();
        _flickerCts = null;
        ResetToOriginal();
    }

    public CancellationTokenSource GetFlickerCts()
    {
        if (_flickerCts == null)
        {
            _flickerCts = new CancellationTokenSource();
        }
        return _flickerCts;
    }
}