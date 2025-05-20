using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private Dictionary<EffectType, IPlayerEffect> effects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Find all effects attached to this GameObject & descendents
        foreach (var effect in GetComponentsInChildren<IPlayerEffect>(true))
        {
            effects[effect.EffectType] = effect;
        }
    }

    public void PlayEffect(EffectType effectType, float strength = 1f)
    {
        if (effects.TryGetValue(effectType, out var effect))
        {
            effect.PlayEffect(strength);
        }
        else
        {
            Debug.LogWarning($"Effect '{effectType}' not found.");
        }
    }

    public async UniTask PlayTimedEffect(EffectType effectType, float strength, float duration)
    {
        if (effects.TryGetValue(effectType, out var effect))
        {
            if (effect is IPlayerTimedEffect timed)
            {
                await timed.PlayEffectTimed(strength, duration);
            }
            else
            {
                Debug.LogWarning($"Effect '{effectType}' is not a timed effect.");
            }
        }
        else
        {
            Debug.LogWarning($"Effect '{effectType}' not found.");
        }
    }
}
