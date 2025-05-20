using UnityEngine;

public interface IPlayerEffect
{
    EffectType EffectType { get; }

    void PlayEffect(float strength = 1f);
}
