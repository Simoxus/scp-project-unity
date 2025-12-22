using UnityEngine;

public abstract class BaseStatusEffects : IStatusEffect
{
    public abstract string EffectID { get; }
    public abstract float Duration { get; }

    public abstract void Apply(GameObject target);
    public abstract void Remove(GameObject target);
    public abstract void Tick(GameObject target);
}