using UnityEngine;

/// Interface for all status effects.
/// Defines the essential methods and properties that every effect must implement.
public interface IStatusEffect
{
    /// Gets the display name of the status effect.
    string Name { get; }

    /// This is useful for preventing stacking of certain effects or for targeted removal.
    string EffectID { get; }

    /// Gets a value indicating whether the status effect is currently active.
    bool IsActive { get; }

    /// Gets the target GameObject this effect is applied to.
    GameObject Target { get; }

    /// Initializes the status effect with its target.
    /// This method is called by the manager when an effect is instantiated.
    void Initialize(GameObject target);

    /// Applies the initial impact or changes of the status effect to the target.
    void Apply();

    /// Removes the status effect and reverts any changes made to the target.
    void Remove();

    /// Updates the status effect over time. This is primarily used by TimedEffects
    /// or effects that require continuous updates.
    void UpdateEffect(float deltaTime);

    /// Called when the effect is paused.
    void Pause();

    /// Called when the effect is resumed after being paused.
    void Resume();
}
