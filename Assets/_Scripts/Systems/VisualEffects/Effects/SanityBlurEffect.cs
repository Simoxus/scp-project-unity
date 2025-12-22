/*
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PostProcessing.Effects
{
    /// <summary>
    /// Accumulation blur effect that responds to player injury/sanity levels.
    /// Automatically increases blur intensity as the player takes damage.
    /// </summary>
    public class SanityBlurEffect : IVisualEffect
    {
        public string EffectId => "sanity_blur";
        public bool IsEnabled { get; private set; }

        [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.Linear(0f, 3f, 1f, 32f);

        private AccumulationBlur _blur;
        private Player _player;
        private bool _enableOnInjury = true;
        private bool _isSubscribed = false;

        public bool Initialize(VolumeProfile profile)
        {
            // Get the accumulation blur component
            if (!profile.TryGet(out _blur))
            {
                Log.Error("AccumulationBlur component not found in Volume profile!");
                return false;
            }

            // Set up default curve if needed
            intensityCurve = AnimationCurve.Linear(0f, 3f, 1f, 32f);

            // Enable override and set to minimum (inactive)
            _blur.sampleCount.overrideState = true;
            _blur.sampleCount.value = 2; // Start at minimum (effect inactive)

            // Get player reference
            _player = Player.Instance;
            if (_player == null)
            {
                Log.VerboseWarning("Player instance not found during initialization. Will retry on Enable.");
            }

            Log.VerboseInfo("Initialized successfully.");
            return true;
        }

        public void Enable()
        {
            if (IsEnabled) return;

            // Try to get player if we don't have it yet
            if (_player == null)
            {
                _player = Player.Instance;
            }

            if (_player == null || _player.playerHealth == null)
            {
                Log.Warning("Cannot enable - Player or PlayerHealth not found.");
                return;
            }

            // Subscribe to health events
            if (!_isSubscribed)
            {
                _player.playerHealth.OnInjuryChanged += OnInjuryChanged;
                _player.playerHealth.OnHealthLevelChanged += OnHealthLevelChanged;
                _isSubscribed = true;
            }

            IsEnabled = true;
            _enableOnInjury = true;

            // Initialize with current injury state
            OnInjuryChanged(_player.playerHealth.GetInjuryFactor());

            Log.VerboseInfo("Enabled.");
        }

        public void Disable()
        {
            if (!IsEnabled) return;

            IsEnabled = false;
            _enableOnInjury = false;

            // Reset blur to inactive
            if (_blur != null)
            {
                _blur.sampleCount.value = 2;
            }

            Log.VerboseInfo("Disabled.");
        }

        public void Update()
        {
            // No per-frame updates needed - event-driven
        }

        public void Cleanup()
        {
            // Unsubscribe from events
            if (_isSubscribed && _player != null && _player.playerHealth != null)
            {
                _player.playerHealth.OnInjuryChanged -= OnInjuryChanged;
                _player.playerHealth.OnHealthLevelChanged -= OnHealthLevelChanged;
                _isSubscribed = false;
            }

            // Reset to inactive state
            if (_blur != null)
            {
                _blur.sampleCount.value = 2;
            }
        }

        private void OnInjuryChanged(float injuryFactor)
        {
            if (!_enableOnInjury || _blur == null) return;

            float curveValue = intensityCurve.Evaluate(injuryFactor / 3f);
            int targetSampleCount = Mathf.RoundToInt(curveValue);
            bool shouldBeActive = injuryFactor > 0.1f;

            if (shouldBeActive)
            {
                _blur.sampleCount.value = Mathf.Max(3, targetSampleCount);
            }
            else
            {
                _blur.sampleCount.value = 2;
            }

            _blur.sampleCount.overrideState = true;

            // FORCE camera to reload post-processing
            var cam = Camera.main;
            if (cam != null)
            {
                var cameraData = cam.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = false;
                cameraData.renderPostProcessing = true;
            }
        }

        private void OnHealthLevelChanged(PlayerHealth.HealthLevel level)
        {
            // Disable effect when dead
            if (level == PlayerHealth.HealthLevel.Dead)
            {
                if (_blur != null)
                {
                    _blur.sampleCount.value = 2;
                }
            }
        }

        #region Public API

        /// <summary>
        /// Set whether the blur should respond to injury changes
        /// </summary>
        public void SetInjuryResponseEnabled(bool enabled)
        {
            _enableOnInjury = enabled;

            if (!enabled && _blur != null)
            {
                _blur.sampleCount.value = 2;
            }
            else if (enabled && _player != null && _player.playerHealth != null)
            {
                // Re-evaluate current injury when re-enabling
                OnInjuryChanged(_player.playerHealth.GetInjuryFactor());
            }
        }

        /// <summary>
        /// Manually set blur intensity (0-1 range, where 0 is no blur)
        /// </summary>
        public void SetManualIntensity(float intensity)
        {
            if (_blur == null) return;

            float curveValue = intensityCurve.Evaluate(intensity);
            int targetSampleCount = Mathf.RoundToInt(curveValue);

            if (intensity > 0.01f)
            {
                _blur.sampleCount.value = Mathf.Max(3, targetSampleCount);
            }
            else
            {
                _blur.sampleCount.value = 2;
            }

            _blur.sampleCount.overrideState = true;
            ForceRefresh();
        }

        /// <summary>
        /// Set the intensity curve used for mapping injury to blur
        /// </summary>
        public void SetIntensityCurve(AnimationCurve curve)
        {
            intensityCurve = curve;

            // Re-evaluate if currently enabled
            if (IsEnabled && _player != null && _player.playerHealth != null)
            {
                OnInjuryChanged(_player.playerHealth.GetInjuryFactor());
            }
        }

        /// <summary>
        /// Force refresh the blur based on current player injury
        /// </summary>
        public void ForceRefresh()
        {
            if (_player != null && _player.playerHealth != null)
            {
                OnInjuryChanged(_player.playerHealth.GetInjuryFactor());
            }
        }

        #endregion
    }
}
*/