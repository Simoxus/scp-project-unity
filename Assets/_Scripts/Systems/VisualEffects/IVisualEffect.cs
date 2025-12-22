using UnityEngine.Rendering;

public interface IVisualEffect
{
    string EffectId { get; }

    bool Initialize(VolumeProfile profile);

    void Enable();

    void Disable();

    void Update();

    void Cleanup();

    bool IsEnabled { get; }
}