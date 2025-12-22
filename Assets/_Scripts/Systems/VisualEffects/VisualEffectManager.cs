
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Manages all visual effects.
/// Automatically discovers and registers effects implementing IVisualEffect.
/// </summary>
public class VisualEffectManager : MonoBehaviour
{
    public static VisualEffectManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Volume postProcessVolume;

    private Dictionary<string, IVisualEffect> effects = new();
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Log.VerboseWarning($"Duplicate instance of {GetType().Name} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeVolumeProfile();
        InitializeEffects();
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Update all enabled effects
        foreach (var effect in effects.Values)
        {
            if (effect.IsEnabled)
            {
                effect.Update();
            }
        }
    }

    private void OnDestroy()
    {
        // Cleanup all effects
        foreach (var effect in effects.Values)
        {
            effect.Cleanup();
        }
        effects.Clear();
    }

    private void InitializeVolumeProfile()
    {
        if (postProcessVolume == null)
            postProcessVolume = FindFirstObjectByType<Volume>();

        if (postProcessVolume == null)
        {
            Log.Error("No Volume found in scene!");
            enabled = false;
            return;
        }

        // CRITICAL: Create a runtime instance of the profile
        // This ensures we're modifying a copy, not the asset itself
        if (postProcessVolume.profile != null)
        {
            postProcessVolume.profile = Instantiate(postProcessVolume.profile);
            Log.Info("Created runtime volume profile instance");
        }
        else
        {
            Log.Error("Volume has no profile assigned!");
            enabled = false;
            return;
        }
    }

    private void InitializeEffects()
    {
        // Scan for all effects in the PostProcessing.Effects namespace
        var effectTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IVisualEffect).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Where(t => t.Namespace == "PostProcessing.Effects");

        foreach (var type in effectTypes)
        {
            try
            {
                IVisualEffect effect = (IVisualEffect)Activator.CreateInstance(type);

                if (effect.Initialize(postProcessVolume.profile))
                {
                    RegisterEffect(effect);
                }
                else
                {
                    Log.Warning($"Effect '{effect.EffectId}' failed to initialize.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to register effect {type.Name}: {ex}");
            }
        }

        isInitialized = true;
        Log.Info($"Initialized {effects.Count} effects.");
    }

    private void RegisterEffect(IVisualEffect effect)
    {
        string key = effect.EffectId.ToLower();

        if (effects.ContainsKey(key))
        {
            Log.VerboseWarning($"Effect '{key}' has already been registered. Overwriting.");
            effects[key] = effect;
        }
        else
        {
            effects.Add(key, effect);
            Log.VerboseInfo($"Registered effect '{key}'.");
        }
    }

    #region Public API

    /// <summary>
    /// Enable an effect by its ID
    /// </summary>
    public void EnableEffect(string effectId)
    {
        if (effects.TryGetValue(effectId.ToLower(), out var effect))
        {
            effect.Enable();
        }
        else
        {
            Log.Warning($"Effect '{effectId}' not found.");
        }
    }

    /// <summary>
    /// Disable an effect by its ID
    /// </summary>
    public void DisableEffect(string effectId)
    {
        if (effects.TryGetValue(effectId.ToLower(), out var effect))
        {
            effect.Disable();
        }
        else
        {
            Log.Warning($"Effect '{effectId}' not found.");
        }
    }

    /// <summary>
    /// Get an effect by its ID (for custom control)
    /// </summary>
    public T GetEffect<T>(string effectId) where T : class, IVisualEffect
    {
        if (effects.TryGetValue(effectId.ToLower(), out var effect))
        {
            return effect as T;
        }
        return null;
    }

    /// <summary>
    /// Check if an effect exists
    /// </summary>
    public bool HasEffect(string effectId)
    {
        return effects.ContainsKey(effectId.ToLower());
    }

    /// <summary>
    /// Get all registered effects
    /// </summary>
    public IReadOnlyDictionary<string, IVisualEffect> GetAllEffects()
    {
        return effects;
    }

    #endregion
}