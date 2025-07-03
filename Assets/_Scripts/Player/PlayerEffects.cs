// PlayerEffects.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq; // For LINQ extension methods

/// <summary>
/// Manages all active status effects on a player or any GameObject.
/// Automatically discovers and initializes all available status effect types.
/// </summary>
public class PlayerEffects : MonoBehaviour
{
    [Tooltip("The GameObject this PlayerEffects instance will apply effects to.")]
    public GameObject targetGameObject;

    private List<IStatusEffect> activeEffects = new List<IStatusEffect>();
    private Dictionary<string, ConstructorInfo> availableEffectConstructors = new Dictionary<string, ConstructorInfo>();
    private GameObject selfTarget;

    void Awake()
    {
        // If no target is explicitly set, this GameObject itself will be the target.
        if (targetGameObject == null)
        {
            selfTarget = gameObject;
            Debug.Log($"PlayerEffects: No explicit target set. Using {selfTarget.name} as target.");
        }
        else
        {
            selfTarget = targetGameObject;
        }

        DiscoverStatusEffectTypes();
    }

    /// <summary>
    /// Discovers all classes that implement IStatusEffect (or inherit from BaseStatusEffect)
    /// and makes their constructors available for instantiation.
    /// </summary>
    private void DiscoverStatusEffectTypes()
    {
        // Get all types in the current assembly that are concrete classes and implement IStatusEffect
        // We look for IStatusEffect directly, so it covers BaseStatusEffect, TimedEffect, ConstantEffect, and concrete effects.
        var effectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IStatusEffect).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        Debug.Log($"PlayerEffects: Discovered {effectTypes.Count()} concrete status effect types.");

        foreach (Type type in effectTypes)
        {
            // Create a dummy instance to get the EffectID (could be improved by using attributes if needed)
            // This requires all effect classes to have a parameterless constructor initially or find a suitable constructor.
            // For now, we'll try to get the constructor with the most parameters, assuming it's the primary one.
            ConstructorInfo constructor = type.GetConstructors()
                                            .OrderByDescending(c => c.GetParameters().Length)
                                            .FirstOrDefault();

            if (constructor != null)
            {
                IStatusEffect tempEffect = null;
                try
                {
                    // Attempt to create a temporary instance to get the EffectID.
                    // This relies on constructors being callable with null/default values for parameters.
                    ParameterInfo[] parameters = constructor.GetParameters();
                    object[] args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        args[i] = parameters[i].ParameterType.IsValueType ? Activator.CreateInstance(parameters[i].ParameterType) : null;
                    }
                    tempEffect = (IStatusEffect)constructor.Invoke(args);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to instantiate temporary {type.Name} to get EffectID. Ensure constructors can be called with default/null values for discovery. Error: {e.Message}");
                    continue;
                }


                if (tempEffect != null && !availableEffectConstructors.ContainsKey(tempEffect.EffectID))
                {
                    availableEffectConstructors.Add(tempEffect.EffectID, constructor);
                    Debug.Log($"PlayerEffects: Registered effect type: {type.Name} with ID: {tempEffect.EffectID}");
                }
                else if (tempEffect != null)
                {
                    Debug.LogWarning($"PlayerEffects: Duplicate EffectID '{tempEffect.EffectID}' found for type {type.Name}. Already registered with {availableEffectConstructors[tempEffect.EffectID].DeclaringType.Name}. Skipping.");
                }
            }
            else
            {
                Debug.LogWarning($"PlayerEffects: Could not find a suitable constructor for effect type: {type.Name}. It will not be available for application.");
            }
        }

        if (availableEffectConstructors.Count == 0)
        {
            Debug.LogWarning("PlayerEffects: No status effects were discovered. Make sure your effect classes are in the same assembly and derive correctly.");
        }
    }

    void Update()
    {
        // Create a temporary list to hold effects that need to be removed
        List<IStatusEffect> effectsToRemove = new List<IStatusEffect>();

        foreach (IStatusEffect effect in activeEffects)
        {
            if (effect.IsActive)
            {
                effect.UpdateEffect(Time.deltaTime);
            }
            else
            {
                effectsToRemove.Add(effect); // Mark for removal if no longer active (e.g., timed out)
            }
        }

        // Remove inactive effects and call their Remove method
        foreach (IStatusEffect effect in effectsToRemove)
        {
            RemoveEffect(effect);
        }
    }

    public void ApplyEffect(string effectID, params object[] constructorArgs)
    {
        if (availableEffectConstructors.TryGetValue(effectID, out ConstructorInfo constructor))
        {
            // Optional: Prevent stacking of effects with the same ID.
            // If you want multiple instances of the same effect (e.g., multiple burn stacks), remove this block.
            IStatusEffect existingEffect = activeEffects.FirstOrDefault(e => e.EffectID == effectID);
            if (existingEffect != null)
            {
                Debug.Log($"PlayerEffects: {effectID} is already active on {selfTarget.name}. Reapplying/Refreshing.");
                existingEffect.Remove(); // Remove old instance before applying new one
                activeEffects.Remove(existingEffect);
            }

            try
            {
                // Instantiate the effect using the constructor and provided arguments
                IStatusEffect newEffect = (IStatusEffect)constructor.Invoke(constructorArgs);

                // Initialize the effect with the actual target GameObject
                newEffect.Initialize(selfTarget);

                // Add to active effects list and apply it
                activeEffects.Add(newEffect);
                newEffect.Apply();
                Debug.Log($"PlayerEffects: Successfully applied {newEffect.Name} ({effectID}) to {selfTarget.name}.");
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerEffects: Failed to instantiate or apply effect '{effectID}'. Error: {e.Message}\nStackTrace: {e.StackTrace}");
            }
        }
        else
        {
            Debug.LogError($"PlayerEffects: Effect with ID '{effectID}' not found or not registered. Did you define it and does it inherit from StatusEffectBase?", this);
        }
    }

    public void RemoveEffect(IStatusEffect effect)
    {
        if (activeEffects.Contains(effect))
        {
            effect.Remove();
            activeEffects.Remove(effect);
            Debug.Log($"PlayerEffects: Removed {effect.Name} from {effect.Target.name}.");
        }
    }

    public bool RemoveEffect(string effectID)
    {
        List<IStatusEffect> effectsToRemove = activeEffects.Where(e => e.EffectID.Equals(effectID, StringComparison.OrdinalIgnoreCase)).ToList();
        bool removedAny = false;
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect); // Uses the existing RemoveEffect(IStatusEffect) method
            removedAny = true;
        }
        if (!removedAny)
        {
            Debug.LogWarning($"PlayerEffects: No active effect with ID '{effectID}' found to remove.");
        }
        return removedAny;
    }

    public void ClearAllEffects()
    {
        List<IStatusEffect> effectsToClear = new List<IStatusEffect>(activeEffects);
        foreach (IStatusEffect effect in effectsToClear)
        {
            RemoveEffect(effect);
        }
        Debug.Log("PlayerEffects: All effects cleared.");
    }

    public List<IStatusEffect> GetActiveEffects()
    {
        return new List<IStatusEffect>(activeEffects);
    }
}
