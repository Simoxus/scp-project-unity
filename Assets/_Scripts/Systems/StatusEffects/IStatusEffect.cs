using UnityEngine;

public interface IStatusEffect
{
    string EffectID { get; } // Must be unique for each effect type.
    float Duration { get; } // Total time the effect takes (in seconds).

    void Apply(GameObject target); // Called when the effect is first applied to a target.
    void Remove(GameObject target); // Called when the effect expires or is removed.
    void Tick(GameObject target); // Called every frame while the effect is active.
}