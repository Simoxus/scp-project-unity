using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IPlayerTimedEffect : IPlayerEffect
{
    UniTask PlayEffectTimed(float strength, float duration);
}

