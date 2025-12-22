using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    private static Dictionary<string, IStatusEffect> registeredEffects;
    private readonly List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    private static bool _isInitialized = false;

    private class ActiveEffect
    {
        public IStatusEffect Effect { get; }
        public float RemainingDuration { get; set; }

        public ActiveEffect(IStatusEffect effect)
        {
            Effect = effect;
            RemainingDuration = effect.Duration;
        }
    }

    private void Awake()
    {
        if (!_isInitialized)
        {
            InitializeEffects();
        }
    }

    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var activeEffect = activeEffects[i];
            activeEffect.RemainingDuration -= Time.deltaTime;

            activeEffect.Effect.Tick(gameObject);

            if (activeEffect.RemainingDuration <= 0)
            {
                activeEffect.Effect.Remove(gameObject);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void AddEffect(string effectID)
    {
        effectID = effectID.ToLower();

        if (!registeredEffects.TryGetValue(effectID, out IStatusEffect effectTemplate))
        {
            Log.Error($"Effect with ID '{effectID}' not found.");
            return;
        }

        var existingEffect = activeEffects.FirstOrDefault(e => e.Effect.EffectID.Equals(effectID, StringComparison.OrdinalIgnoreCase));
        if (existingEffect != null)
        {
            existingEffect.RemainingDuration = effectTemplate.Duration;
        }
        else
        {
            var newActiveEffect = new ActiveEffect(effectTemplate);
            newActiveEffect.Effect.Apply(gameObject);
            activeEffects.Add(newActiveEffect);
        }
    }

    private void InitializeEffects()
    {
        registeredEffects = new Dictionary<string, IStatusEffect>();

        var effectTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IStatusEffect).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Where(t => t.Namespace == "Game.StatusEffects.Effects");

        foreach (var type in effectTypes)
        {
            try
            {
                IStatusEffect effect = (IStatusEffect)Activator.CreateInstance(type);
                string id = effect.EffectID.ToLower();

                if (!registeredEffects.ContainsKey(id))
                {
                    registeredEffects.Add(id, effect);
                    Log.VerboseInfo($"Registered status effect '{id}'.");
                }
                else
                {
                    Log.VerboseWarning($"Status effect with ID '{id}' is already registered.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to register status effect {type.Name}: {ex.Message}");
            }
        }

        _isInitialized = true;
    }
}
