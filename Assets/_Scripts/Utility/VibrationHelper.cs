using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

public static class VibrationHelper
{
    public static bool IsVibrationEnabled { get; set; } = true;

    public static void Vibrate() => Vibrate(0.5f, 0.5f, 0.2f);
    public static void VibrateTap() => Vibrate(0.05f, 0.06f, 0.07f);
    public static void VibrateLight() => Vibrate(0.1f, 0.1f, 0.12f);
    public static void VibrateHeavy() => Vibrate(1f, 1f, 0.3f);

    public static void OnApplicationQuit() => Stop();

    public static void Vibrate(float lowFrequency, float highFrequency, float duration)
    {
        if (!IsVibrationEnabled) return;

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            StopVibrationAfter(duration).Forget();
        }
    }

    public static void Stop()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    private static async UniTaskVoid StopVibrationAfter(float duration)
    {
        await UniTask.WaitForSeconds(duration, ignoreTimeScale: true);
        Stop();
    }
}