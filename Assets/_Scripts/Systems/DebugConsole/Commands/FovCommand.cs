using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

namespace Console.Commands
{
    public class FovCommand : BaseConsole
    {
        public override string CommandWord => "fov";
        public override string Description => "Adjusts the vertical field of view.";
        protected override string RawUsage => "fov <value>";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            CinemachineCamera cinemachineCamera = Core.Player.CameraMain;
            if (cinemachineCamera == null) return;

            if (float.TryParse(args[0], out float targetFOV))
            {
                if (targetFOV <= 0 || targetFOV >= 200)
                {
                    ConsoleManager.LogToConsole("Invalid FOV value. Must be between 0 and 200.".AsError());
                    return;
                }

                float currentFOV = cinemachineCamera.Lens.FieldOfView;

                Tween.Custom(
                    currentFOV,
                    targetFOV,
                    0.55f,
                    onValueChange: fov =>
                    {
                        var lens = cinemachineCamera.Lens;
                        lens.FieldOfView = fov;
                        cinemachineCamera.Lens = lens;
                    },
                    Ease.InOutCirc
                ).OnComplete(() =>
                {
                    // Save to settings after animation completes
                    SaveFOVToSettings(targetFOV);
                });

                ConsoleManager.LogToConsole($"Field of view set to {targetFOV}.".AsSuccess());
            }
            else
            {
                ConsoleManager.LogToConsole(Usage.AsError());
            }
        }

        private void SaveFOVToSettings(float fovValue)
        {
            if (Core.SettingsManager == null) return;

            // Save the FOV value using the same category/key as SettingsGraphics
            Core.SettingsManager.SaveFloat("Graphics", "FieldOfView", fovValue);

            // Update the UI slider if it exists
            SettingsGraphics settingsGraphics = Object.FindFirstObjectByType<SettingsGraphics>();
            if (settingsGraphics != null && settingsGraphics.fieldOfViewSlider != null)
            {
                settingsGraphics.fieldOfViewSlider.SetValueWithoutNotify(fovValue);
            }
        }
    }
}