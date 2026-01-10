using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public void GenerateShake(float intensity)
    {
        if (Core.Player.CameraImpulseSource == null) return;
        if (Core.GameManager.cameraShaking == false) return;

        Vector3 impulseVelocity = new Vector3(intensity, intensity, intensity);
        Core.Player.CameraImpulseSource.GenerateImpulseWithVelocity(impulseVelocity);
    }

    public void GenerateShakeWithVector3(Vector3 impulseVelocity)
    {
        if (Core.Player.CameraImpulseSource == null) return;
        if (Core.GameManager.cameraShaking == false) return;

        Core.Player.CameraImpulseSource.GenerateImpulseWithVelocity(impulseVelocity);
    }
}